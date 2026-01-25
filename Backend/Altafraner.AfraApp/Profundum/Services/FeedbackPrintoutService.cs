using System.Collections.Immutable;
using System.Text.RegularExpressions;
using Altafraner.AfraApp.Profundum.Domain.DTO;
using Altafraner.AfraApp.Profundum.Domain.Models.Bewertung;
using Altafraner.AfraApp.User.Domain.DTO;
using Altafraner.AfraApp.User.Domain.Models;
using Altafraner.AfraApp.User.Services;
using Microsoft.EntityFrameworkCore;

namespace Altafraner.AfraApp.Profundum.Services;

internal partial class FeedbackPrintoutService
{
    private readonly AfraAppContext _dbContext;
    private readonly Altafraner.Typst.Typst _typstService;
    private readonly UserService _userService;

    public FeedbackPrintoutService(Altafraner.Typst.Typst typstService,
        UserService userService,
        AfraAppContext dbContext)
    {
        _typstService = typstService;
        _userService = userService;
        _dbContext = dbContext;
    }

    public async Task<byte[]> GenerateFileForPerson(Guid personId)
    {
        var user = await _userService.GetUserByIdAsync(personId);
        var userGm = await _dbContext.Personen.Where(p =>
                _dbContext.MentorMenteeRelations.Where(e => e.StudentId == user.Id && e.Type == MentorType.GM)
                    .Select(e => e.MentorId)
                    .Contains(p.Id))
            .FirstOrDefaultAsync();

        var feedback = await _dbContext.ProfundumFeedbackEntries
            .AsSplitQuery()
            .Include(e => e.Anker)
            .ThenInclude(a => a.Kategorie)
            .ThenInclude(e => e.Fachbereiche)
            .Include(e => e.Instanz)
            .ThenInclude(e => e.Profundum)
            .Include(e => e.Instanz)
            .ThenInclude(e => e.Slots)
            .Include(e => e.Instanz)
            .ThenInclude(e => e.Verantwortliche)
            .Where(e => e.BetroffenePerson == user)
            .ToArrayAsync();

        var profunda = feedback.Select(e => e.Instanz)
            .DistinctBy(e => e.Profundum)
            .Select(e => new ProfundumFeedbackPdfData.Profundum(e.Profundum.Bezeichnung,
                e.Verantwortliche.Select(v => new PersonInfoMinimal(v))));

        var feedbackByKategorie = feedback.GroupBy(e => e.Anker, e => e.Grad)
            .GroupBy(e => e.Key.Kategorie)
            .ToArray();

        List<IGrouping<ProfundumFeedbackKategorie, IGrouping<ProfundumFeedbackAnker, int>>> allgemein =
            new(feedbackByKategorie.Length / 2);
        List<IGrouping<ProfundumFeedbackKategorie, IGrouping<ProfundumFeedbackAnker, int>>> fachlich =
            new(feedbackByKategorie.Length / 2);

        foreach (var kategorie in feedbackByKategorie)
        {
            if (kategorie.Key.IsFachlich)
            {
                fachlich.Add(kategorie);
                continue;
            }

            allgemein.Add(kategorie);
        }

        var allgemeinSorted = allgemein
            .OrderByDescending(g => g.Key.Fachbereiche.Count)
            .ThenBy(g => g.Key.Label)
            .ToImmutableSortedDictionary(k => k.Key.Label,
                k => k.ToDictionary(e => ExtractFirstMarkdownBoldIfExists(e.Key.Label), e => e.ToArray()));

        var fachlichSorted = fachlich
            .OrderByDescending(g => g.Key.Fachbereiche.Count)
            .ThenBy(g => g.Key.Label)
            .ToImmutableSortedDictionary(k => k.Key.Label,
                k => k.ToDictionary(e => ExtractFirstMarkdownBoldIfExists(e.Key.Label), e => e.ToArray()));

        ProfundumFeedbackPdfData[] data =
        [
            new()
            {
                Meta = new ProfundumFeedbackPdfData.MetaData("25.01.2026", 25),
                Person = new PersonInfoMinimal(user),
                GM = userGm is not null ? new PersonInfoMinimal(userGm) : null,
                Schulleiter = new PersonInfoMinimal
                {
                    Vorname = "Annabell",
                    Nachname = "Hecht"
                },
                Profunda = profunda,
                FeedbackAllgemein = allgemeinSorted,
                FeedbackFachlich = fachlichSorted
            }
        ];

        var file = _typstService.GeneratePdf(Altafraner.Typst.Templates.Profundum.Feedback, data);
        return file;
    }

    private static string ExtractFirstMarkdownBoldIfExists(string input)
    {
        var regex = ExtractBoldRegex();
        var match = regex.Match(input);
        return match.Success ? match.Groups[1].Value : input;
    }

    [GeneratedRegex(@"\*\*(.+?)[*,-.]*\*\*", RegexOptions.Compiled)]
    private static partial Regex ExtractBoldRegex();
}

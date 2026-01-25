using System.Collections.Immutable;
using System.Text.RegularExpressions;
using Altafraner.AfraApp.Profundum.Domain.DTO;
using Altafraner.AfraApp.User.Domain.DTO;
using Altafraner.AfraApp.User.Services;
using Microsoft.EntityFrameworkCore;

namespace Altafraner.AfraApp.Profundum.Services;

internal partial class FeedbackPrintoutService
{
    private readonly Altafraner.Typst.Typst _typstService;
    private readonly UserService _userService;
    private readonly AfraAppContext _dbContext;

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
            .Distinct()
            .Select(e => new ProfundumFeedbackPdfData.Profundum(e.Profundum.Bezeichnung,
                e.Verantwortliche.Select(v => new PersonInfoMinimal(v))));

        var feedbackByAnker = feedback.GroupBy(e => e.Anker)
            .ToDictionary(g => g.Key, g => g.Select(e => e.Grad).ToArray());

        var feedbackByAnkerAndCategory = feedbackByAnker.GroupBy(e => e.Key.Kategorie)
            .OrderByDescending(g => g.Key.Fachbereiche.Count)
            .ThenBy(g => g.Key.Label)
            .ToImmutableSortedDictionary(k => k.Key.Label,
                k => k.ToDictionary(e => ExtractFirstMarkdownBoldIfExists(e.Key.Label), e => e.Value));

        var data = new ProfundumFeedbackPdfData
        {
            Meta = new ProfundumFeedbackPdfData.MetaData("25.01.2026", 25),
            Person = new PersonInfoMinimal(user),
            Profunda = profunda,
            Feedback = feedbackByAnkerAndCategory
        };

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

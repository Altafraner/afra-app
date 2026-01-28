using System.Collections.Immutable;
using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;
using Altafraner.AfraApp.Domain.Configuration;
using Altafraner.AfraApp.Profundum.Domain.DTO;
using Altafraner.AfraApp.Profundum.Domain.Models;
using Altafraner.AfraApp.Profundum.Domain.Models.Bewertung;
using Altafraner.AfraApp.User.Domain.DTO;
using Altafraner.AfraApp.User.Domain.Models;
using Altafraner.AfraApp.User.Services;
using Altafraner.Backbone.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Quartz.Util;
using Person = Altafraner.AfraApp.User.Domain.Models.Person;

namespace Altafraner.AfraApp.Profundum.Services;

internal partial class FeedbackPrintoutService
{
    private readonly AfraAppContext _dbContext;
    private readonly Altafraner.Typst.Typst _typstService;
    private readonly UserService _userService;
    private readonly IOptions<GeneralConfiguration> _generalConfig;

    public FeedbackPrintoutService(Altafraner.Typst.Typst typstService,
        UserService userService,
        AfraAppContext dbContext,
        IOptions<GeneralConfiguration> generalConfig)
    {
        _typstService = typstService;
        _userService = userService;
        _dbContext = dbContext;
        _generalConfig = generalConfig;
    }

    public async Task<byte[]> GenerateFileForPerson(Person user, int schuljahr, bool halbjahr, DateOnly ausgabedatum)
    {
        var userGm = await _dbContext.Personen.Where(p =>
                _dbContext.MentorMenteeRelations.Where(e => e.StudentId == user.Id && e.Type == MentorType.GM)
                    .Select(e => e.MentorId)
                    .Contains(p.Id))
            .FirstOrDefaultAsync();

        var quartale = GetQuartaleForHalbjahr(halbjahr);

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
            .Where(e => e.Instanz.Slots.Any(s => s.Jahr == schuljahr && quartale.Contains(s.Quartal)))
            .ToArrayAsync();

        var meta = new ProfundumFeedbackPdfData.MetaData(ausgabedatum.ToShortDateString(), schuljahr, halbjahr, false);
        ProfundumFeedbackPdfData[] data =
            [FeedbackToInputData(feedback, user, userGm, meta)];

        var file = _typstService.GeneratePdf(Altafraner.Typst.Templates.Profundum.Feedback, data);
        return file;
    }

    private ProfundumFeedbackPdfData FeedbackToInputData(ProfundumFeedbackEntry[] feedback,
        Person user,
        Person? userGm,
        ProfundumFeedbackPdfData.MetaData meta)
    {
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

        var data =
            new ProfundumFeedbackPdfData
            {
                Meta = meta,
                Person = new PersonInfoMinimal(user),
                GM = userGm is not null ? new PersonInfoMinimal(userGm) : null,
                Schulleiter = _generalConfig.Value.Schulleiter,
                Profunda = profunda,
                FeedbackAllgemein = allgemeinSorted,
                FeedbackFachlich = fachlichSorted
            };
        return data;
    }

    public async Task<byte[]> GenerateFileBatched(BatchingModes mode,
        int schuljahr,
        bool halbjahr,
        DateOnly ausgabedatum,
        bool doublesided)
    {
        if (mode.HasFlag(BatchingModes.Single) && mode != BatchingModes.Single)
            throw new ArgumentException("Cannot batch and single at onec", nameof(mode));

        var warnings = new List<string>();

        var allUsers = await _dbContext.Personen.Where(e => e.Rolle == Rolle.Mittelstufe)
            .Include(e => e.MentorMenteeRelations.Where(m => m.Type == MentorType.GM))
            .OrderBy(e => e.FirstName)
            .ThenBy(e => e.LastName)
            .AsAsyncEnumerable()
            .GroupBy(e => (
                mode.HasFlag(BatchingModes.ByClass) ? e.Gruppe ?? "unbekannt" : "beliebig",
                mode.HasFlag(BatchingModes.ByGm)
                    ? e.MentorMenteeRelations.FirstOrDefault()?.MentorId
                    : Guid.AllBitsSet))
            .ToArrayAsync();

        var allMentors = (await _userService.GetUsersWithRoleAsync(Rolle.Tutor)).ToDictionary(e => e.Id, e => e)
            .AsReadOnly();

        var quartale = GetQuartaleForHalbjahr(halbjahr);

        var allFeedback = await _dbContext.ProfundumFeedbackEntries.AsSplitQuery()
            .Include(e => e.Anker)
            .ThenInclude(a => a.Kategorie)
            .ThenInclude(e => e.Fachbereiche)
            .Include(e => e.Instanz)
            .ThenInclude(e => e.Profundum)
            .Include(e => e.Instanz)
            .ThenInclude(e => e.Slots)
            .Include(e => e.Instanz)
            .ThenInclude(e => e.Verantwortliche)
            .Where(e => e.Instanz.Slots.Any(s => s.Jahr == schuljahr && quartale.Contains(s.Quartal)))
            .GroupBy(e => e.BetroffenePersonId)
            .ToDictionaryAsync(e => e.Key, e => e.ToArray());

        var meta = new ProfundumFeedbackPdfData.MetaData(ausgabedatum.ToShortDateString(),
            schuljahr,
            halbjahr,
            doublesided);

        using var zipStream = new MemoryStream();
        await using (var zip = new ZipArchive(zipStream, ZipArchiveMode.Create, true))
        {
            foreach (var grouping in allUsers)
            {
                List<ProfundumFeedbackPdfData> data = new(mode.HasFlag(BatchingModes.Single) ? 1 : grouping.Count());
                foreach (var person in grouping)
                {
                    var feedbackExists = allFeedback.TryGetValue(person.Id, out var feedback);
                    if (!feedbackExists)
                    {
                        warnings.Add(WarningForUser(person, "Kein Feedback für  gefunden."));
                        continue;
                    }

                    var gm = person.MentorMenteeRelations.Count != 0
                        ? allMentors[person.MentorMenteeRelations.First().MentorId]
                        : null;
                    if (gm == null) warnings.Add(WarningForUser(person, "Kein GM gefunden"));
                    if (person.MentorMenteeRelations.Count > 1)
                        warnings.Add(WarningForUser(person, "Mehrere GMs registriert, nehme ersten"));

                    data.Add(FeedbackToInputData(feedback!, person, gm, meta));
                    if (mode.HasFlag(BatchingModes.Single))
                    {
                        var file = _typstService.GeneratePdf(Altafraner.Typst.Templates.Profundum.Feedback, data);
                        var entry = zip.CreateEntry(
                            FilenameSanitizer.Sanitize($"{person.Gruppe}_{NicePersonName(person)}.pdf"));
                        await using var entryStream = await entry.OpenAsync();
                        await entryStream.WriteAsync(file);
                        data.Clear();
                    }
                }

                if (data.Count == 0)
                    continue;

                if (!mode.HasFlag(BatchingModes.Single))
                {
                    var file = _typstService.GeneratePdf(Altafraner.Typst.Templates.Profundum.Feedback, data);
                    var entry = zip.CreateEntry(
                        FilenameSanitizer.Sanitize(NiceFilename(grouping.Key.Item1, grouping.Key.Item2)));
                    await using var entryStream = await entry.OpenAsync();
                    await entryStream.WriteAsync(file);
                }
            }

            var warningsText = string.Join(Environment.NewLine, warnings.Order());
            if (!warningsText.IsNullOrWhiteSpace())
            {
                var bytes = Encoding.UTF8.GetBytes(warningsText);
                var entry = zip.CreateEntry("_warnings.txt");
                await using var entryStream = await entry.OpenAsync();
                await entryStream.WriteAsync(bytes);
            }
        }

        return zipStream.ToArray();

        string NiceFilename(string klasse, Guid? mentor)
        {
            return
                $"{klasse}-{(mentor is null
                    ? "unbekannt"
                    : mentor == Guid.AllBitsSet
                        ? "beliebig"
                        : NicePersonName(allMentors.GetValueOrDefault(mentor.Value)))}.pdf";
        }

        string NicePersonName(Person? user)
        {
            return user is null ? "unbekannt" : $"{user.LastName}_{user.FirstName}";
        }

        string WarningForUser(Person user, string warning)
        {
            return $"{user.LastName}, {user.FirstName} ({user.Gruppe}): {warning}";
        }
    }

    private static List<ProfundumQuartal> GetQuartaleForHalbjahr(bool halbjahr)
    {
        return halbjahr
            ? [ProfundumQuartal.Q1, ProfundumQuartal.Q2]
            : [ProfundumQuartal.Q1, ProfundumQuartal.Q2, ProfundumQuartal.Q3, ProfundumQuartal.Q4];
    }

    [Flags]
    public enum BatchingModes
    {
        ByGm = 1,
        ByClass = 2,
        Single = 4
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

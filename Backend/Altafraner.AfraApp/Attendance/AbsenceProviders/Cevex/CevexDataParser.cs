using System.Text;
using System.Text.Json;
using Altafraner.AfraApp.Attendance.Configuration;
using Altafraner.AfraApp.User.Domain.Models;
using Altafraner.Backbone.EmailOutbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Options;

namespace Altafraner.AfraApp.Attendance.AbsenceProviders.Cevex;

internal class CevexDataParser
{
    private const string CacheTag = "cevex";
    private const string CacheMatchesLabel = "cevex-matches";
    private const string CacheContentLabel = "cevex-content";
    private const string CacheAgeLabel = "cevex-fileage";

    private readonly AfraAppContext _dbContext;
    private readonly ILogger _logger;
    private readonly IEmailOutbox _outbox;
    private readonly HybridCache _cache;
    private readonly CevexConfig _cevexConfig;

    public CevexDataParser(AfraAppContext dbContext,
        ILogger<CevexDataParser> logger,
        IEmailOutbox outbox,
        HybridCache cache,
        IOptions<AttendanceConfiguration> cevexConfig)
    {
        _dbContext = dbContext;
        _logger = logger;
        _outbox = outbox;
        _cache = cache;
        _cevexConfig = cevexConfig.Value.Cevex!;
    }

    private async Task<IEnumerable<CevexUser>> ReadFileFromDisk()
    {
        await using var stream = File.OpenRead(_cevexConfig.FilePath);
        using var reader = new StreamReader(stream, Encoding.Unicode); // uft16le
        var contents = await reader.ReadToEndAsync();
        if (contents.StartsWith((char)0xFFFE)) contents = contents[1..]; // skip bom
        var data = JsonSerializer.Deserialize<CevexUserOriginal[]>(contents);
        return data?.Select(e => new CevexUser(e)) ?? [];
    }

    private ValueTask<IEnumerable<CevexUser>> ReadFileFromCache()
    {
        return _cache.GetOrCreateAsync(CacheContentLabel, async _ => await ReadFileFromDisk(), tags: [CacheTag]);
    }

    public async Task<IEnumerable<CevexUser>> ReadFile()
    {
        await EnsureCurrent();
        return await ReadFileFromCache();
    }

    private async Task EnsureCurrent()
    {
        var lastSync =
            await _cache.GetOrCreateAsync(CacheAgeLabel,
                _ => ValueTask.FromResult(DateTime.MinValue),
                tags: [CacheTag]);
        var lastModified = File.GetLastWriteTime(_cevexConfig.FilePath);
        await _cache.SetAsync(CacheAgeLabel, lastModified, tags: [CacheTag]);
        if (lastSync < lastModified) await _cache.RemoveByTagAsync(CacheTag);
    }

    private ValueTask<Dictionary<Guid, Missing[]>> ReadMatchesFromCache()
    {
        return _cache.GetOrCreateAsync(CacheMatchesLabel,
            async _ =>
            {
                var data = await ReadFileFromCache();
                return await TryCorrelateWithStudents(data);
            },
            tags: [CacheTag]);
    }

    public async Task<Dictionary<Guid, Missing[]>> GetMatches()
    {
        await EnsureCurrent();
        return await ReadMatchesFromCache();
    }

    private async Task<Dictionary<Guid, Missing[]>> TryCorrelateWithStudents(IEnumerable<CevexUser> cevexData)
    {
        var cevexDataArray = cevexData as CevexUser[] ?? cevexData.ToArray();
        var allStudents = await _dbContext.Personen
            .Where(p => p.Rolle == Rolle.Mittelstufe || p.Rolle == Rolle.Oberstufe)
            .ToListAsync();
        var studentsByLastname = allStudents
            .Select(s => (lastname: s.LastName.ToLowerInvariant().Trim(),
                entry: (student: s,
                    firstNames: s.FirstName.ToLowerInvariant()
                        .Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))))
            .GroupBy(s => s.lastname, s => s.entry)
            .ToDictionary(s => s.Key, s => s.ToArray());
        var matches = new Dictionary<Person, CevexUser>();
        foreach (var user in cevexDataArray)
        {
            var cevexLastname = user.Lastname.ToLowerInvariant().Trim();
            var lastNameExists = studentsByLastname.TryGetValue(cevexLastname, out var students);
            if (!lastNameExists) continue;
            var cevexFirstnames = user.Firstname.ToLowerInvariant()
                .Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            var student =
                students!.SingleOrDefault(s => s.firstNames.Any(cevexFirstnames.Contains));
            if (student.student is null) continue;
            if (!matches.TryAdd(student.student, user))
                _logger.LogWarning("Found multiple matches for user with guid {id}", student.student.Id);
        }

        var unknown = new List<Person>();
        foreach (var student in allStudents)
            if (matches.TryGetValue(student, out var match))
            {
                if (match.Classname != student.Gruppe)
                    _logger.LogWarning("Different Class for {firstName} {lastName}: cevex: {cevex}, ldap: {ldap}",
                        student.FirstName,
                        student.LastName,
                        match.Classname,
                        student.Gruppe);
            }
            else
            {
                unknown.Add(student);
            }

        var now = DateTime.UtcNow;
        var notify = new HashSet<Person>();
        foreach (var student in unknown.Where(e => !e.CevexSyncFailureTime.HasValue && !e.CevexIdManuallyEntered))
        {
            student.CevexSyncFailureTime = now;
            notify.Add(student);
        }

        foreach (var (student, match) in matches)
        {
            if (student.CevexIdManuallyEntered) continue;
            if (student.CevexId is null)
            {
                student.CevexId = match.Guid;
                continue;
            }

            if (student.CevexId == match.Guid) continue;
            _logger.LogWarning("Overwriting match for student: {guid} {firstName} {lastName}",
                student.Id,
                student.FirstName,
                student.LastName);
            student.CevexId = match.Guid;
        }

        var cevexIds = cevexDataArray.Select(e => e.Guid).ToHashSet();
        var registeredCevexIds = allStudents.Select(e => e.CevexId).ToHashSet();
        registeredCevexIds.ExceptWith(cevexIds);
        foreach (var cevexId in registeredCevexIds)
        {
            var student = allStudents.FirstOrDefault(s => s.CevexId == cevexId);
            student!.CevexId = null;
            student.CevexIdManuallyEntered = false;
            student.CevexSyncFailureTime = now;
            notify.Add(student);
        }

        await _dbContext.SaveChangesAsync();

        // reconstruct matches
        var matchesToSave = new Dictionary<Guid, Missing[]>();
        foreach (var student in allStudents)
        {
            if (student.CevexId is null) continue;
            var match = cevexDataArray.FirstOrDefault(e => e.Guid == student.CevexId);
            if (match is not null) matchesToSave.Add(student.Id, match.Missings);
        }

        // Send mail
        if (notify.Count > 0)
        {
            var body = $"""
                        Hallo,

                        die cevex-Synchronisierung ist für neue Nutzer:innen fehlgeschlagen. Bitte weisen sie die Schüler:innen manuell zu.

                        {notify.Aggregate("", (current, next) => current + $"- {next.FirstName} {next.LastName} ({next.Gruppe}){Environment.NewLine}")}
                        """;
            foreach (var recipient in _cevexConfig.SyncNotificationRecipients)
                await _outbox.SendReportAsync(recipient, "Cevex-Nutersynchronisierung fehlgeschlagen", body);
        }

        return matchesToSave;
    }
}

using System.Diagnostics;
using System.Text;
using Altafraner.AfraApp.Profundum.Configuration;
using Altafraner.AfraApp.Profundum.Domain.Contracts.Services;
using Altafraner.AfraApp.Profundum.Domain.DTO;
using Altafraner.AfraApp.Profundum.Domain.Models;
using Altafraner.AfraApp.User.Services;
using Altafraner.Backbone.EmailSchedulingModule;
using Google.OrTools.Sat;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Models_Person = Altafraner.AfraApp.User.Domain.Models.Person;

namespace Altafraner.AfraApp.Profundum.Services;

///
internal class ProfundumEinwahlWunschException : Exception
{
    ///
    public ProfundumEinwahlWunschException(string message)
        : base(message)
    {
    }
}

/// <summary>
///     A service for handling enrollments.
/// </summary>
internal class ProfundumEnrollmentService
{
    private readonly AfraAppContext _dbContext;
    private readonly ILogger _logger;
    private readonly INotificationService _notificationService;
    private readonly IOptions<ProfundumConfiguration> _profundumConfiguration;
    private readonly IRulesFactory _rulesFactory;
    private readonly UserService _userService;

    /// <summary>
    ///     Constructs the EnrollmentService. Usually called by the DI container.
    /// </summary>
    public ProfundumEnrollmentService(AfraAppContext dbContext,
        ILogger<ProfundumEnrollmentService> logger,
        UserService userService,
        IOptions<ProfundumConfiguration> profundumConfiguration,
        INotificationService notificationService,
        IRulesFactory rulesFactory)
    {
        _dbContext = dbContext;
        _logger = logger;
        _userService = userService;
        _profundumConfiguration = profundumConfiguration;
        _notificationService = notificationService;
        _rulesFactory = rulesFactory;
    }

    public bool IsProfundumBlockiert(Models_Person student, IEnumerable<ProfundumQuartal> quartale)
    {
        var klasse = _userService.GetKlassenstufe(student);
        var blockiertQuartale = _profundumConfiguration.Value.ProfundumBlockiert.GetValueOrDefault(klasse);
        if (blockiertQuartale is null) return false;

        var ret = blockiertQuartale.Intersect(quartale).Any();
        return ret;
    }

    public bool IsProfilPflichtig(Models_Person student, IEnumerable<ProfundumQuartal> quartale)
    {
        var klasse = _userService.GetKlassenstufe(student);
        var profilQuartale = _profundumConfiguration.Value.ProfilPflichtigkeit.GetValueOrDefault(klasse);
        if (profilQuartale is null) return false;

        var ret = profilQuartale.Intersect(quartale).Any();
        return ret;
    }

    public bool IsProfilZulässig(Models_Person student, IEnumerable<ProfundumQuartal> quartale)
    {
        var klasse = student.Gruppe;
        if (klasse is null) return false;

        var profilQuartale = _profundumConfiguration.Value.ProfilZulassung.GetValueOrDefault(klasse);
        if (profilQuartale is null) return false;

        var ret = profilQuartale.Intersect(quartale).Any();
        return ret;
    }

    public IEnumerable<ProfundumInstanz> GetAvailableProfundaInstanzen(Models_Person student,
        IEnumerable<ProfundumSlot> slots)
    {
        var klasse = _userService.GetKlassenstufe(student);
        var profundumSlots = slots as ProfundumSlot[] ?? slots.ToArray();
        var profilPflichtig = IsProfilPflichtig(student, profundumSlots.Select(s => s.Quartal));
        var profilZulässig = IsProfilZulässig(student, profundumSlots.Select(s => s.Quartal));
        var profundaInstanzen = _dbContext.ProfundaInstanzen
            .Include(p => p.Slots)
            .Include(p => p.Profundum)
            .ThenInclude(p => p.Kategorie)
            .Where(p => (p.Profundum.MinKlasse == null || klasse >= p.Profundum.MinKlasse)
                        && (p.Profundum.MaxKlasse == null || klasse <= p.Profundum.MaxKlasse))
            .Where(p => !p.Profundum.Kategorie.ProfilProfundum || profilPflichtig || profilZulässig)
            .ToArray();
        return profundaInstanzen;
    }

    public ProfundumEinwahlZeitraum? GetCurrentEinwahlZeitraum()
    {
        return _dbContext.ProfundumEinwahlZeitraeume
            .Include(ez => ez.Slots)
            .ToArray()
            .FirstOrDefault(defaultValue: null);
    }

    public BlockKatalog[] GetKatalog(Models_Person student)
    {
        var einwahlZeitraum = GetCurrentEinwahlZeitraum();
        if (einwahlZeitraum is null) return [];

        var blockiert = IsProfundumBlockiert(student, einwahlZeitraum.Slots.Select(s => s.Quartal));
        if (blockiert) return [];

        var slots = _dbContext.ProfundaSlots.ToArray().Order(new ProfundumSlotComparer());

        var angebote = GetAvailableProfundaInstanzen(student, slots).ToArray();

        return slots
            .Select(slot => new
            {
                slot,
                profundumInstanzenBeginningInSlot =
                    angebote.Where(p =>
                        p.Slots.Count != 0 && p.Slots.Min(new ProfundumSlotComparer())!.Id == slot.Id)
            })
            .Select(t => new BlockKatalog
            {
                Fixed = _dbContext.ProfundaEinschreibungen
                .Include(p => p.ProfundumInstanz).ThenInclude(i => i!.Slots)
                .Include(p => p.ProfundumInstanz).ThenInclude(i => i!.Profundum)
                    .Where(e => e.BetroffenePerson.Id == student.Id)
                    .Where(e => e.Slot.Id == t.slot.Id)
                    .Select(e => e.ProfundumInstanz)
                    .Select(p => p == null ? new BlockOption { Label = "-", Value = null } : new BlockOption
                    {
                        Label = p.Slots.Count <= 1
                            ? p.Profundum.Bezeichnung
                            : $"{p.Profundum.Bezeichnung} ({p.Slots.Count} Quartale)",
                        Value = p.Id,
                    })
                    .FirstOrDefault(),
                Label = $"{t.slot.Jahr} {t.slot.Quartal} {t.slot.Wochentag switch
                {
                    DayOfWeek.Monday => "Montag",
                    DayOfWeek.Tuesday => "Dienstag",
                    DayOfWeek.Wednesday => "Mittwoch",
                    DayOfWeek.Thursday => "Donnerstag",
                    DayOfWeek.Friday => "Freitag",
                    DayOfWeek.Saturday => "Samstag",
                    DayOfWeek.Sunday => "Sonntag",
                    _ => ""
                }}",
                Id = t.slot.ToString(),
                Options = t.profundumInstanzenBeginningInSlot.OrderBy(x => !x.Profundum.Kategorie.ProfilProfundum)
                    .ThenBy(x => x.Profundum.Bezeichnung)
                    .Select(p => new BlockOption
                    {
                        Label = p.Slots.Count <= 1
                            ? p.Profundum.Bezeichnung
                            : $"{p.Profundum.Bezeichnung} ({p.Slots.Count} Quartale)",
                        Value = p.Id,
                        AlsoIncludes = p.Slots.Order(new ProfundumSlotComparer())
                            .Skip(1)
                            .Select(s => s.ToString())
                            .ToArray()
                    })
                    .ToArray()
            })
            .ToArray();
    }

    /// <summary>
    ///     Register a set of Profundum Belegwuensche.
    ///     Validates that all currently open slots are filled
    /// </summary>
    /// <param name="student">The student wanting to enroll</param>
    /// <param name="wuensche">A dictionary containing the ordered ids of ProdundumInstanzen given the slot</param>
    public async Task RegisterBelegWunschAsync(Models_Person student, Dictionary<string, Guid[]> wuensche)
    {
        var einwahlZeitraum = GetCurrentEinwahlZeitraum();
        if (einwahlZeitraum is null) throw new ProfundumEinwahlWunschException("Momentan keine offene Einschreibung");

        var slots = einwahlZeitraum.Slots;
        if (slots is null) throw new ProfundumEinwahlWunschException("einwahlzeitraum hat keine slots");

        var blockiert = IsProfundumBlockiert(student, slots.Select(s => s.Quartal));
        if (blockiert) throw new ProfundumEinwahlWunschException("Klassenstufe vom Profundum ausgeschlossen.");

        var konflikte = _dbContext.ProfundaBelegWuensche
            .Include(bw => bw.ProfundumInstanz)
            .ThenInclude(pi => pi.Slots)
            .ThenInclude(profundumSlot => profundumSlot.EinwahlZeitraum)
            .Include(bw => bw.BetroffenePerson)
            .Where(p => p.BetroffenePerson.Id == student.Id)
            .AsEnumerable()
            .Where(p => p.ProfundumInstanz.Slots.Any(s => s.EinwahlZeitraum.Id == einwahlZeitraum.Id))
            .ToArray();
        if (konflikte.Length != 0) _dbContext.ProfundaBelegWuensche.RemoveRange(konflikte);

        var angebote = GetAvailableProfundaInstanzen(student, slots).ToHashSet();
        var angeboteUsed = new HashSet<ProfundumInstanz>();

        var wuenscheDict = new Dictionary<ProfundumBelegWunschStufe, HashSet<ProfundumInstanz>>
        {
            [ProfundumBelegWunschStufe.ErstWunsch] = [],
            [ProfundumBelegWunschStufe.ZweitWunsch] = [],
            [ProfundumBelegWunschStufe.DrittWunsch] = []
        };

        foreach (var (str, l) in wuensche)
        {
            var s = slots.FirstOrDefault(sm => sm.ToString() == str);
            if (s is null) throw new ProfundumEinwahlWunschException("Kein solcher Slot");

            if (l.Length != 3) throw new ProfundumEinwahlWunschException("Zu viele Wünsche für einen Slot");

            for (var i = 0; i < l.Length; ++i)
            {
                if (!Enum.IsDefined(typeof(ProfundumBelegWunschStufe), i + 1))
                    throw new ProfundumEinwahlWunschException("Belegwunschstufe nicht definiert.");

                var stufe = (ProfundumBelegWunschStufe)(i + 1);

                if (angeboteUsed.FirstOrDefault(a => a.Id == l[i]) is not null) continue;

                var angebot = angebote.FirstOrDefault(a => a.Id == l[i]);
                if (angebot is null) throw new ProfundumEinwahlWunschException($"Profundum nicht gefundum {l[i]}.");

                wuenscheDict[stufe].Add(angebot);
                angebote.Remove(angebot);
                angeboteUsed.Add(angebot);
            }
        }

        var einwahl = new Dictionary<ProfundumSlot, ProfundumInstanz?[]>();
        foreach (var s in slots) einwahl[s] = new ProfundumInstanz?[3];

        var belegWuensche = new HashSet<ProfundumBelegWunsch>();
        foreach (var (stufe, instanzen) in wuenscheDict)
            foreach (var angebot in instanzen)
                foreach (var angebotSlot in angebot.Slots)
                {
                    var stufeIndex = (int)stufe - 1;
                    if (einwahl[angebotSlot][stufeIndex] is not null)
                        throw new ProfundumEinwahlWunschException("Überlappende Slots in der Einwahl.");

                    einwahl[angebotSlot][stufeIndex] = angebot;
                }

        if (slots.SelectMany(s => einwahl[s]).Any(pi => pi is null))
            throw new ProfundumEinwahlWunschException("Leerer Slot in Einwahl.");

        foreach (var (stufe, instanzen) in wuenscheDict)
            foreach (var angebot in instanzen)
            {
                var belegWunsch = new ProfundumBelegWunsch
                {
                    BetroffenePerson = student,
                    ProfundumInstanz = angebot,
                    Stufe = stufe
                    // EinwahlZeitraum = einwahlZeitraum,
                };
                belegWuensche.Add(belegWunsch);
            }

        foreach (var r in _rulesFactory.GetIndividualRules())
        {
            var status = r.CheckForSubmission(student, slots, belegWuensche);
            if (!status.IsValid)
                throw new ProfundumEinwahlWunschException(status.Messages
                    .Aggregate(new StringBuilder(), (a, b) => a.AppendLine(b))
                    .ToString());
        }

        var kategorien = await _dbContext.ProfundaKategorien.Where(k => k.MaxProEinwahl != null).ToArrayAsync();
        foreach (var kat in kategorien)
        {
            var n = belegWuensche.Count(b => b.ProfundumInstanz.Profundum.Kategorie == kat);
            if (n > kat.MaxProEinwahl)
                throw new ProfundumEinwahlWunschException(
                    $"Nur {kat.MaxProEinwahl} Profunda der Kategorie {kat.Bezeichnung} wählbar");
        }

        await SendWuenscheEMail(student, einwahlZeitraum, belegWuensche);

        _dbContext.ProfundaBelegWuensche.AddRange(belegWuensche);
        await _dbContext.SaveChangesAsync();
    }

    private async Task SendWuenscheEMail(Models_Person student,
        ProfundumEinwahlZeitraum einwahlZeitraum,
        IEnumerable<ProfundumBelegWunsch> wuensche)
    {
        var wuenscheArray = wuensche as ProfundumBelegWunsch[] ?? wuensche.ToArray();
        var slots = einwahlZeitraum.Slots;

        var wuenscheString = new StringBuilder();
        wuenscheString.AppendLine("Du hast die folgenden Wünsche zur Profundumseinwahl abgegeben.");
        wuenscheString.AppendLine("Falls du eine Änderung vornehmen möchtest, fülle das Formular neu aus.");
        wuenscheString.AppendLine();
        foreach (var slot in slots)
        {
            var slotString = $"{slot.Jahr} {slot.Quartal} {slot.Wochentag switch
            {
                DayOfWeek.Monday => "Montag",
                DayOfWeek.Tuesday => "Dienstag",
                DayOfWeek.Wednesday => "Mittwoch",
                DayOfWeek.Thursday => "Donnerstag",
                DayOfWeek.Friday => "Freitag",
                DayOfWeek.Saturday => "Samstag",
                DayOfWeek.Sunday => "Sonntag",
                _ => ""
            }}";
            wuenscheString.AppendLine($"{slotString}: ");

            foreach (var b in wuenscheArray.Where(b => b.ProfundumInstanz.Slots.Contains(slot)))
                wuenscheString.AppendLine($"    {(int)b.Stufe}. {b.ProfundumInstanz.Profundum.Bezeichnung}");
        }

        await _notificationService.ScheduleNotificationAsync(student,
            "Deine Profunda Einwahl-Wünsche",
            wuenscheString.ToString(),
            TimeSpan.Zero);
    }

    ///
    public async Task<Dictionary<string, DTOProfundumDefinition>> GetEnrollment(Models_Person student,
        ICollection<Guid> slotIds)
    {
        return (await _dbContext.ProfundaSlots.Where(s => slotIds.Contains(s.Id)).ToArrayAsync()).ToDictionary(
            s => s.ToString(),
            s =>
                _dbContext.ProfundaEinschreibungen
                    .Where(p => p.ProfundumInstanz == null)
                    .Include(pe => pe.ProfundumInstanz!)
                    .ThenInclude(pi => pi.Profundum)
                    .ThenInclude(p => p.Kategorie)
                    .Where(pe => pe.BetroffenePerson.Id == student.Id)
                    .Where(p => p.ProfundumInstanz!.Slots.Contains(s))
                    .Select(pe => new DTOProfundumDefinition(pe.ProfundumInstanz!.Profundum))
                    .First());
    }
}

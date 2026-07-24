using System.Text;
using Altafraner.AfraApp.Profundum.Configuration;
using Altafraner.AfraApp.Profundum.Domain.Contracts.Services;
using Altafraner.AfraApp.Profundum.Domain.DTO;
using Altafraner.AfraApp.Profundum.Domain.Models;
using Altafraner.AfraApp.User.Services;
using Altafraner.Backbone.EmailSchedulingModule;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Models_Person = Altafraner.AfraApp.User.Domain.Models.Person;

namespace Altafraner.AfraApp.Profundum.Services;

internal class ProfundumEinwahlWunschException : ArgumentException
{
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

    /// <summary>Whether any of the given Quartale falls in a Halbjahr where this student's grade must take a Profil.</summary>
    public bool IsProfilPflichtig(Models_Person student, IEnumerable<ProfundumQuartal> quartale)
    {
        var klasse = _userService.GetKlassenstufe(student, DateTime.UtcNow);
        var profilQuartale = _profundumConfiguration.Value.ProfilPflichtigkeit.GetValueOrDefault(klasse);
        if (profilQuartale is null) return false;

        var ret = profilQuartale.Intersect(quartale).Any();
        return ret;
    }

    /// <summary>
    ///     Whether a student meets the grade-range and (if applicable) Profil-eligibility criteria to enroll in the
    ///     given Definition. Used to guard team-partner invite redemption up front - see
    ///     <see cref="ProfundumPartnerService" />.
    /// </summary>
    public bool IsEligibleForDefinition(Models_Person student, ProfundumDefinition definition, IEnumerable<ProfundumQuartal> quartale)
    {
        var klasse = _userService.GetKlassenstufe(student, DateTime.UtcNow);
        if (definition.MinKlasse is not null && klasse < definition.MinKlasse) return false;
        if (definition.MaxKlasse is not null && klasse > definition.MaxKlasse) return false;
        if (definition.Kategorie.ProfilProfundum)
        {
            return IsProfilPflichtig(student, quartale);
        }

        return true;
    }

    /// <summary>
    ///     Instanzen the student is eligible for by grade range and Profil status, restricted to the given slots
    ///     (an Instanz only qualifies if all of its own Slots are among them), and further narrowed to Definitionen
    ///     every registered <see cref="Domain.Contracts.Rules.IProfundumIndividualRule.CheckDefinitionEligibility" />
    ///     agrees this student could ever legally pick (e.g. excludes an unmet Dependency or an already-enrolled
    ///     Profundum) - so the catalog never suggests something guaranteed to be rejected at submission.
    /// </summary>
    public IEnumerable<ProfundumInstanz> GetAvailableProfundaInstanzen(Models_Person student,
        IEnumerable<ProfundumSlot> slots, bool profil, IEnumerable<ProfundumEinschreibung> enrollments)
    {
        var klasse = _userService.GetKlassenstufe(student, DateTime.UtcNow);
        var enrollmentsArray = enrollments as ProfundumEinschreibung[] ?? enrollments.ToArray();
        var individualRules = _rulesFactory.GetIndividualRules().ToArray();
        var profundaInstanzen = _dbContext.ProfundaInstanzen
            .AsSplitQuery()
            .Include(p => p.Slots)
            .Include(p => p.Verantwortliche)
            .Include(p => p.Profundum).ThenInclude(p => p.Kategorie)
            .Include(p => p.Profundum).ThenInclude(p => p.Dependencies)
            .Include(p => p.Profundum).ThenInclude(p => p.Fachbereiche)
            .Where(p => (p.Profundum.MinKlasse == null || klasse >= p.Profundum.MinKlasse)
                        && (p.Profundum.MaxKlasse == null || klasse <= p.Profundum.MaxKlasse))
            .Where(p => !p.Profundum.Kategorie.ProfilProfundum || profil)
            .Where(p => p.Slots.All(x => slots.Contains(x)))
            .ToArray()
            .Where(p => individualRules.All(r =>
                r.CheckDefinitionEligibility(student, p.Profundum, slots, enrollmentsArray).IsValid))
            .ToArray();
        return profundaInstanzen;
    }

    private static string SlotLabel(ProfundumSlot slot) => $"{slot.Jahr} {slot.Quartal} {slot.Wochentag switch
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

    ///
    public DTOProfundumKatalog GetKatalog(Models_Person student)
    {
        var cfg = _profundumConfiguration.Value;
        var now = DateTime.UtcNow;
        var einschreibeZeitraum = _dbContext.ProfundumEinwahlZeitraeume.FirstOrDefault(z => z.EinwahlStart <= now && z.EinwahlStop > now);
        if (einschreibeZeitraum is null)
        {
            return new DTOProfundumKatalog
            {
                Optionen = [],
                Fixiert = [],
                OffeneSlotIds = [],
                MinBelegWuensche = cfg.MinBelegWuensche,
                MinWuenschePerSlot = cfg.MinWuenschePerSlot,
                AktuelleWuensche = [],
                IstAbgegeben = false,
            };
        }

        var slots = _dbContext.ProfundaSlots.ToArray().Order(new ProfundumSlotComparer()).ToArray();
        var fixedEnrollments = _dbContext.ProfundaEinschreibungen
            .Where(e => e.IsFixed)
            .Where(e => e.BetroffenePerson == student)
            .Include(e => e.ProfundumInstanz).ThenInclude(p => p!.Profundum)
            .Include(e => e.Slot).ToArray();
        var fixedSlots = fixedEnrollments.Select(e => e.Slot).Distinct().ToArray();
        var openSlots = slots.Where(s => !fixedSlots.Contains(s)).ToArray();

        var profilPflichtig = IsProfilPflichtig(student, slots.Select(s => s.Quartal));
        var angebote = GetAvailableProfundaInstanzen(student, openSlots, profilPflichtig, fixedEnrollments).ToArray();

        var optionen = angebote
            .GroupBy(a => a.Profundum)
            .Select(g => new DTOKatalogEintrag
            {
                DefinitionId = g.Key.Id,
                Bezeichnung = g.Key.Bezeichnung,
                ProfilProfundum = g.Key.Kategorie.ProfilProfundum,
                SlotIds = g.SelectMany(a => a.Slots)
                    .Where(openSlots.Contains)
                    .Select(s => s.ToString())
                    .Distinct()
                    .ToArray(),
                ErlaubtPartnerwahl = g.Key.ErlaubtPartnerwahl,
                Beschreibung = g.Key.Beschreibung,
                Fachbereiche = g.Key.Fachbereiche.Select(f => f.Label).ToArray(),
                Voraussetzungen = g.Key.Dependencies.Select(d => d.Bezeichnung).ToArray(),
                Instanzen = g.Select(a => new DTOKatalogEintragInstanz
                {
                    SlotIds = a.Slots.Where(openSlots.Contains).Select(s => s.ToString()).ToArray(),
                    Ort = a.Ort,
                    Verantwortliche = a.Verantwortliche.Select(v => v.ToString()).ToArray(),
                    MaxEinschreibungen = a.MaxEinschreibungen,
                }).ToArray(),
            })
            .OrderBy(x => !x.ProfilProfundum)
            .ThenBy(x => x.Bezeichnung)
            .ToArray();

        var fixiert = fixedEnrollments
            .Select(e => new DTOFixierterSlot
            {
                SlotId = e.Slot.ToString(),
                SlotLabel = SlotLabel(e.Slot),
                Bezeichnung = e.ProfundumInstanz is null ? "-" : e.ProfundumInstanz.Profundum.Bezeichnung,
            })
            .ToArray();

        var aktuelleWuensche = _dbContext.ProfundaBelegWuensche
            .Where(w => w.BetroffenePerson == student && w.EinwahlZeitraum == einschreibeZeitraum)
            .OrderBy(w => w.Rang)
            .ToArray();

        return new DTOProfundumKatalog
        {
            Optionen = optionen,
            Fixiert = fixiert,
            OffeneSlotIds = openSlots.Select(s => s.ToString()).ToArray(),
            MinBelegWuensche = cfg.MinBelegWuensche,
            MinWuenschePerSlot = cfg.MinWuenschePerSlot,
            AktuelleWuensche = aktuelleWuensche.Select(w => w.ProfundumDefinitionId).ToArray(),
            IstAbgegeben = aktuelleWuensche.Length > 0 && aktuelleWuensche[0].IstAbgegeben,
        };
    }

    /// <summary>
    ///     Register a ranked set of Profundum Belegwuensche.
    ///     When <paramref name="istEntwurf" /> is false (a final submission), validates that at least
    ///     <see cref="ProfundumConfiguration.MinBelegWuensche" /> Profunda are ranked, that each currently open slot
    ///     is covered by at least <see cref="ProfundumConfiguration.MinWuenschePerSlot" /> of them, and runs the full
    ///     rule engine. When <paramref name="istEntwurf" /> is true (an unfinished draft save), only structural/
    ///     eligibility checks run (Einwahl open, no duplicate Profunda, each wish resolves to something offerable) -
    ///     completeness is deliberately not required for a draft, since the student is still working on it.
    /// </summary>
    /// <param name="student">The student wanting to enroll</param>
    /// <param name="wuensche">The ranked ids of ProfundumDefinitionen, in order of preference (index 0 = rank 1)</param>
    /// <param name="istEntwurf">True to save as an unfinished draft rather than a final submission.</param>
    public async Task RegisterBelegWunschAsync(Models_Person student, List<Guid> wuensche, bool istEntwurf = false)
    {
        var cfg = _profundumConfiguration.Value;
        var now = DateTime.UtcNow;
        var einschreibeZeitraum =
            await _dbContext.ProfundumEinwahlZeitraeume.FirstOrDefaultAsync(z =>
                z.EinwahlStart <= now && z.EinwahlStop > now);
        if (einschreibeZeitraum is null)
            throw new ProfundumEinwahlWunschException("Einwahl geschlossen");

        var fixedEnrollments = await _dbContext.ProfundaEinschreibungen
            .Where(e => e.IsFixed)
            .Where(e => e.BetroffenePerson == student)
            .Include(e => e.ProfundumInstanz).ThenInclude(p => p!.Profundum)
            .Include(e => e.Slot)
            .ToArrayAsync();
        var fixedSlots = fixedEnrollments.Select(s => s.Slot).Distinct().ToArray();
        var slots = _dbContext.ProfundaSlots.ToArray();
        var openSlots = await _dbContext.ProfundaSlots
            .Where(s => !fixedSlots.Contains(s))
            .ToArrayAsync();

        var toRemove = _dbContext.ProfundaBelegWuensche
            .Where(w => w.BetroffenePerson == student)
            .Where(w => w.EinwahlZeitraum == einschreibeZeitraum);
        _dbContext.ProfundaBelegWuensche.RemoveRange(toRemove);

        if (!istEntwurf && wuensche.Count < cfg.MinBelegWuensche)
            throw new ProfundumEinwahlWunschException($"Es müssen mindestens {cfg.MinBelegWuensche} Profunda gewählt werden.");
        if (wuensche.Distinct().Count() != wuensche.Count)
            throw new ProfundumEinwahlWunschException("Ein Profundum darf nur einmal gewählt werden.");

        var profilPflichtig = IsProfilPflichtig(student, slots.Select(s => s.Quartal));
        var angebote = GetAvailableProfundaInstanzen(student, openSlots, profilPflichtig, fixedEnrollments)
            .ToLookup(a => a.Profundum.Id);

        var slotCoverage = openSlots.ToDictionary(s => s, _ => 0);
        var belegWuensche = new List<ProfundumBelegWunsch>();

        for (var i = 0; i < wuensche.Count; ++i)
        {
            var definitionId = wuensche[i];
            var instanzenForDefinition = angebote[definitionId].ToArray();
            if (instanzenForDefinition.Length == 0)
                throw new ProfundumEinwahlWunschException($"Profundum nicht gefunden oder nicht wählbar: {definitionId}.");

            foreach (var s in instanzenForDefinition.SelectMany(a => a.Slots).Distinct())
                if (slotCoverage.ContainsKey(s))
                    slotCoverage[s]++;

            belegWuensche.Add(new ProfundumBelegWunsch
            {
                BetroffenePerson = student,
                ProfundumDefinition = instanzenForDefinition[0].Profundum,
                Rang = i + 1,
                EinwahlZeitraum = einschreibeZeitraum,
                IstAbgegeben = !istEntwurf,
            });
        }

        if (!istEntwurf)
        {
            var uncovered = slotCoverage.Where(kv => kv.Value < cfg.MinWuenschePerSlot).Select(kv => kv.Key).ToArray();
            if (uncovered.Length != 0)
                throw new ProfundumEinwahlWunschException(
                    $"Für die folgenden Slots müssen mindestens {cfg.MinWuenschePerSlot} der gewählten Profunda ein Angebot enthalten: "
                    + string.Join(", ", uncovered.Select(s => s.ToString())));

            var errmsgs = _rulesFactory.GetIndividualRules().Select(r => r.CheckForSubmission(student, slots, fixedEnrollments, belegWuensche))
                .Where(x => !x.IsValid).SelectMany(x => x.Messages);
            if (errmsgs.Any())
            {
                throw new ProfundumEinwahlWunschException(errmsgs.Aggregate(new StringBuilder(), (a, b) => a.AppendLine(b)).ToString());
            }
        }

        _dbContext.ProfundaBelegWuensche.AddRange(belegWuensche);
        await _dbContext.SaveChangesAsync();
        if (!istEntwurf)
            await SendWuenscheEMail(student, belegWuensche);
    }

    private async Task SendWuenscheEMail(Models_Person student, IEnumerable<ProfundumBelegWunsch> wuensche)
    {
        var wuenscheString = new StringBuilder();
        wuenscheString.AppendLine("Du hast die folgenden Wünsche zur Profundumseinwahl abgegeben.");
        wuenscheString.AppendLine("Falls du eine Änderung vornehmen möchtest, fülle das Formular neu aus.");
        wuenscheString.AppendLine();
        foreach (var b in wuensche.OrderBy(b => b.Rang))
            wuenscheString.AppendLine($"    {b.Rang}. {b.ProfundumDefinition.Bezeichnung}");

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
                    .AsSplitQuery()
                    .Where(p => p.ProfundumInstanz == null)
                    .Include(pe => pe.ProfundumInstanz!)
                    .ThenInclude(pi => pi.Profundum)
                    .ThenInclude(p => p.Kategorie)
                    .Include(pe => pe.ProfundumInstanz!)
                    .ThenInclude(pi => pi.Profundum)
                    .ThenInclude(p => p.Fachbereiche)
                    .Where(pe => pe.BetroffenePerson.Id == student.Id)
                    .Where(p => p.ProfundumInstanz!.Slots.Contains(s))
                    .Select(pe => new DTOProfundumDefinition(pe.ProfundumInstanz!.Profundum))
                    .First());
    }
}

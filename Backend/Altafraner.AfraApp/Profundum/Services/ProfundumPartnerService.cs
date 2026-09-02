using Altafraner.AfraApp.Profundum.Configuration;
using Altafraner.AfraApp.Profundum.Domain.DTO;
using Altafraner.AfraApp.Profundum.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Models_Person = Altafraner.AfraApp.User.Domain.Models.Person;

namespace Altafraner.AfraApp.Profundum.Services;

internal class ProfundumPartnerException : ArgumentException
{
    public ProfundumPartnerException(string message)
        : base(message)
    {
    }
}

/// <summary>
///     Handles mutual team-partner invites and pairings for Profunda whose organizer has enabled
///     <see cref="ProfundumDefinition.ErlaubtPartnerwahl" />. A pairing only takes effect once BOTH students have
///     opted in - one student creates an invite (a random token), the other redeems it - and only synchronizes
///     placement in the matching solver, it never forces either student into the topic (see
///     <c>PartnerPairingRule</c>).
/// </summary>
internal class ProfundumPartnerService
{
    private const int MaxTokenAttempts = 10;

    private readonly AfraAppContext _dbContext;
    private readonly ProfundumEnrollmentService _enrollmentService;
    private readonly ProfundumPartnerTokenService _tokenService;
    private readonly int _tokenWordCount;

    public ProfundumPartnerService(AfraAppContext dbContext, ProfundumEnrollmentService enrollmentService,
        ProfundumPartnerTokenService tokenService, IOptions<ProfundumConfiguration> config)
    {
        _dbContext = dbContext;
        _enrollmentService = enrollmentService;
        _tokenService = tokenService;
        _tokenWordCount = config.Value.PartnerwahlTokenWordCount;
    }

    private async Task<ProfundumEinwahlZeitraum> GetOffeneEinwahlZeitraumAsync()
    {
        var now = DateTime.UtcNow;
        var zeitraum = await _dbContext.ProfundumEinwahlZeitraeume
            .Include(z => z.Slots)
            .FirstOrDefaultAsync(z => z.EinwahlStart <= now && z.EinwahlStop > now);
        if (zeitraum is null)
            throw new ProfundumPartnerException("Einwahl geschlossen");
        return zeitraum;
    }

    public async Task<DTOProfundumPartnerEinladung> CreateEinladungAsync(Models_Person student, Guid definitionId)
    {
        var zeitraum = await GetOffeneEinwahlZeitraumAsync();

        var definition = await _dbContext.Profunda
            .Include(d => d.Kategorie)
            .FirstOrDefaultAsync(d => d.Id == definitionId);
        if (definition is null)
            throw new ProfundumPartnerException("Profundum nicht gefunden.");
        if (!definition.ErlaubtPartnerwahl)
            throw new ProfundumPartnerException("Für dieses Profundum ist keine Partnerwahl vorgesehen.");

        if (!_enrollmentService.IsEligibleForDefinition(student, definition, zeitraum.Slots.Select(s => s.Quartal)))
            throw new ProfundumPartnerException("Du erfüllst die Voraussetzungen für dieses Profundum nicht.");

        if (await IstBereitsGepaartAsync(zeitraum.Id, definitionId, student.Id))
            throw new ProfundumPartnerException("Du hast für dieses Profundum bereits eine bestätigte Partnerschaft.");

        var token = await GenerateUniqueTokenAsync(zeitraum.Id);

        var einladung = new ProfundumPartnerEinladung
        {
            Id = Guid.NewGuid(),
            Token = token,
            ProfundumDefinition = definition,
            EinwahlZeitraum = zeitraum,
            InitiatorPerson = student,
        };
        _dbContext.ProfundumPartnerEinladungen.Add(einladung);
        await _dbContext.SaveChangesAsync();

        return new DTOProfundumPartnerEinladung(einladung);
    }

    private async Task<string> GenerateUniqueTokenAsync(Guid einwahlZeitraumId)
    {
        var wordCount = _tokenWordCount;
        for (var attempt = 0; attempt < MaxTokenAttempts; attempt++)
        {
            var token = _tokenService.GenerateToken(wordCount);
            var vergeben = await _dbContext.ProfundumPartnerEinladungen
                .AnyAsync(e => e.EinwahlZeitraumId == einwahlZeitraumId && e.Token == token);
            if (!vergeben)
                return token;

            wordCount++;
        }

        throw new ProfundumPartnerException("Konnte keinen eindeutigen Einladungscode erzeugen.");
    }

    /// <summary>
    ///     Whether the given person already has a confirmed <see cref="ProfundumPartnerWunsch" /> for this
    ///     Definition/Einwahlzeitraum, as either PersonA or PersonB. Checked before both creating a new invite
    ///     and redeeming one - a person may only ever have one confirmed partner per Profundum per Einwahlzeitraum.
    /// </summary>
    private Task<bool> IstBereitsGepaartAsync(Guid einwahlZeitraumId, Guid definitionId, Guid personId)
        => _dbContext.ProfundumPartnerWuensche
            .AnyAsync(w => w.EinwahlZeitraumId == einwahlZeitraumId
                    && w.ProfundumDefinitionId == definitionId
                    && (w.PersonAId == personId || w.PersonBId == personId));

    public async Task<DTOProfundumPartnerWunsch> RedeemEinladungAsync(Models_Person student, Guid definitionId, string token)
    {
        var zeitraum = await GetOffeneEinwahlZeitraumAsync();
        var normalizedToken = token.Trim().ToLowerInvariant();

        var einladung = await _dbContext.ProfundumPartnerEinladungen
            .Include(e => e.ProfundumDefinition).ThenInclude(d => d.Kategorie)
            .Include(e => e.InitiatorPerson)
            .FirstOrDefaultAsync(e => e.Token == normalizedToken && e.EinwahlZeitraumId == zeitraum.Id
                    && e.ProfundumDefinitionId == definitionId);
        if (einladung is null)
            throw new ProfundumPartnerException("Einladung ungültig oder abgelaufen.");
        if (einladung.InitiatorPersonId == student.Id)
            throw new ProfundumPartnerException("Du kannst deine eigene Einladung nicht annehmen.");

        if (!_enrollmentService.IsEligibleForDefinition(student, einladung.ProfundumDefinition, zeitraum.Slots.Select(s => s.Quartal)))
            throw new ProfundumPartnerException("Du erfüllst die Voraussetzungen für dieses Profundum nicht.");

        if (await IstBereitsGepaartAsync(zeitraum.Id, definitionId, student.Id))
            throw new ProfundumPartnerException("Du hast für dieses Profundum bereits eine bestätigte Partnerschaft.");
        if (await IstBereitsGepaartAsync(zeitraum.Id, definitionId, einladung.InitiatorPersonId))
            throw new ProfundumPartnerException(
                "Diese Einladung ist nicht mehr gültig, da die einladende Person bereits anderweitig verpartnert ist.");

        var wunsch = new ProfundumPartnerWunsch
        {
            Id = Guid.NewGuid(),
            ProfundumDefinition = einladung.ProfundumDefinition,
            EinwahlZeitraum = zeitraum,
            PersonA = einladung.InitiatorPerson,
            PersonB = student,
        };
        _dbContext.ProfundumPartnerWuensche.Add(wunsch);
        _dbContext.ProfundumPartnerEinladungen.Remove(einladung);
        await _dbContext.SaveChangesAsync();

        return new DTOProfundumPartnerWunsch(wunsch, student.Id);
    }

    public async Task<(DTOProfundumPartnerEinladung[] Einladungen, DTOProfundumPartnerWunsch[] Wuensche)> GetForStudentAsync(Models_Person student)
    {
        var einladungen = await _dbContext.ProfundumPartnerEinladungen
            .Include(e => e.ProfundumDefinition)
            .Where(e => e.InitiatorPersonId == student.Id)
            .Select(e => new DTOProfundumPartnerEinladung(e))
            .ToArrayAsync();

        var wuensche = await _dbContext.ProfundumPartnerWuensche
            .Include(w => w.ProfundumDefinition)
            .Include(w => w.PersonA)
            .Include(w => w.PersonB)
            .Where(w => w.PersonAId == student.Id || w.PersonBId == student.Id)
            .Select(w => new DTOProfundumPartnerWunsch(w, student.Id))
            .ToArrayAsync();

        return (einladungen, wuensche);
    }

    public async Task DeleteEinladungAsync(Models_Person student, string token)
    {
        var normalizedToken = token.Trim().ToLowerInvariant();
        await _dbContext.ProfundumPartnerEinladungen
            .Where(e => e.Token == normalizedToken && e.InitiatorPersonId == student.Id)
            .ExecuteDeleteAsync();
    }

    public async Task DeleteWunschAsync(Models_Person student, Guid id)
    {
        await _dbContext.ProfundumPartnerWuensche
            .Where(w => w.Id == id && (w.PersonAId == student.Id || w.PersonBId == student.Id))
            .ExecuteDeleteAsync();
    }

    public Task<DTOProfundumPartnerWunschStaff[]> GetAllWuenscheAsync()
    {
        return _dbContext.ProfundumPartnerWuensche
            .Include(w => w.ProfundumDefinition)
            .Include(w => w.PersonA)
            .Include(w => w.PersonB)
            .Select(w => new DTOProfundumPartnerWunschStaff(w))
            .ToArrayAsync();
    }

    public async Task DissolveWunschAsync(Guid id)
    {
        await _dbContext.ProfundumPartnerWuensche.Where(w => w.Id == id).ExecuteDeleteAsync();
    }
}

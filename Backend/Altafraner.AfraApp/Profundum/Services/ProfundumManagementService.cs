using System.Text.Json;
using Altafraner.AfraApp.Profundum.Domain.DTO;
using Altafraner.AfraApp.Profundum.Domain.Models;
using Altafraner.AfraApp.User.Domain.DTO;
using Altafraner.Typst;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Altafraner.AfraApp.Profundum.Services;

/// <summary>
///     A service for managing profunda.
/// </summary>
internal class ProfundumManagementService
{
    private readonly AfraAppContext _dbContext;
    private readonly ILogger _logger;
    private readonly IOptions<TypstConfiguration> _typstConfig;
    private readonly Typst.Typst _typst;

    /// <summary>
    ///     Constructs the ManagementService. Usually called by the DI container.
    /// </summary>
    public ProfundumManagementService(AfraAppContext dbContext,
        ILogger<ProfundumManagementService> logger,
        IOptions<TypstConfiguration> typstConfig,
        Typst.Typst typst
        )
    {
        _dbContext = dbContext;
        _logger = logger;
        _typstConfig = typstConfig;
        _typst = typst;
    }

    public async Task<ProfundumEinwahlZeitraum> CreateEinwahlZeitraumAsync(DTOProfundumEinwahlZeitraumCreation zeitraum)
    {
        if (zeitraum.EinwahlStart is null || zeitraum.EinwahlStop is null)
        {
            throw new ArgumentNullException();
        }

        var einwahlZeitraum = new ProfundumEinwahlZeitraum
        {
            EinwahlStart = DateTime.SpecifyKind(DateTime.Parse(zeitraum.EinwahlStart), DateTimeKind.Utc),
            EinwahlStop = DateTime.SpecifyKind(DateTime.Parse(zeitraum.EinwahlStop), DateTimeKind.Utc),
        };
        _dbContext.ProfundumEinwahlZeitraeume.Add(einwahlZeitraum);
        await _dbContext.SaveChangesAsync();
        return einwahlZeitraum;
    }

    public Task<DTOProfundumEinwahlZeitraum[]> GetEinwahlZeiträumeAsync()
    {
        return _dbContext.ProfundumEinwahlZeitraeume
            .Select(e => new DTOProfundumEinwahlZeitraum(e))
            .ToArrayAsync();
    }

    public async Task<bool> UpdateEinwahlZeitraumAsync(Guid id, DTOProfundumEinwahlZeitraumCreation dto)
    {
        var zeitraum = await _dbContext.ProfundumEinwahlZeitraeume.FindAsync(id);
        if (zeitraum is null)
            return false;

        if (dto.EinwahlStart != null)
            zeitraum.EinwahlStart = DateTime.SpecifyKind(DateTime.Parse(dto.EinwahlStart), DateTimeKind.Utc);

        if (dto.EinwahlStop != null)
            zeitraum.EinwahlStop = DateTime.SpecifyKind(DateTime.Parse(dto.EinwahlStop), DateTimeKind.Utc);

        await _dbContext.SaveChangesAsync();
        return true;
    }

    public Task DeleteEinwahlZeitraumAsync(Guid id)
    {
        return _dbContext.ProfundumEinwahlZeitraeume
            .Where(e => e.Id == id)
            .ExecuteDeleteAsync();
    }

    public Task<DTOProfundumSlot[]> GetSlotsAsync()
    {
        return _dbContext.ProfundaSlots
            .Include(s => s.EinwahlZeitraum)
            .Select(s => new DTOProfundumSlot(s))
            .ToArrayAsync();
    }

    public async Task<ProfundumSlot?> CreateSlotAsync(DTOProfundumSlotCreation dtoSlot)
    {
        var zeitraum = await _dbContext.ProfundumEinwahlZeitraeume.FindAsync(dtoSlot.EinwahlZeitraumId);
        if (zeitraum is null)
        {
            return null;
        }

        var slot = new ProfundumSlot
        {
            Jahr = dtoSlot.Jahr,
            Quartal = dtoSlot.Quartal,
            Wochentag = dtoSlot.Wochentag,
            EinwahlZeitraum = zeitraum,
        };
        _dbContext.ProfundaSlots.Add(slot);
        await _dbContext.SaveChangesAsync();
        return slot;
    }

    public async Task<bool> UpdateSlotAsync(Guid id, DTOProfundumSlotCreation dto)
    {
        var slot = await _dbContext.ProfundaSlots
            .Include(s => s.EinwahlZeitraum)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (slot is null)
            return false;

        slot.Jahr = dto.Jahr;
        slot.Quartal = dto.Quartal;
        slot.Wochentag = dto.Wochentag;

        if (dto.EinwahlZeitraumId != Guid.Empty && dto.EinwahlZeitraumId != slot.EinwahlZeitraum.Id)
        {
            var zeitraum = await _dbContext.ProfundumEinwahlZeitraeume.FindAsync(dto.EinwahlZeitraumId);
            if (zeitraum is null)
                return false;
            slot.EinwahlZeitraum = zeitraum;
        }

        await _dbContext.SaveChangesAsync();
        return true;
    }

    public Task DeleteSlotAsync(Guid id)
    {
        return _dbContext.ProfundaSlots
            .Where(s => s.Id == id)
            .ExecuteDeleteAsync();
    }

    public async Task<ProfundumKategorie?> CreateKategorieAsync(DTOProfundumKategorieCreation dtoKategorie)
    {
        var kategorie = new ProfundumKategorie
        {
            Bezeichnung = dtoKategorie.Bezeichnung,
            ProfilProfundum = dtoKategorie.ProfilProfundum,
            MaxProEinwahl = dtoKategorie.MaxProEinwahl,
        };

        _dbContext.ProfundaKategorien.Add(kategorie);
        await _dbContext.SaveChangesAsync();
        return kategorie;
    }

    public async Task<ProfundumKategorie?> UpdateKategorieAsync(Guid kategorieId, DTOProfundumKategorieCreation dtoKategorie)
    {
        var kategorie = await _dbContext.ProfundaKategorien.FindAsync(kategorieId);
        if (kategorie is null)
        {
            throw new ArgumentException();
        }

        if (dtoKategorie.MaxProEinwahl != kategorie.MaxProEinwahl)
            kategorie.MaxProEinwahl = dtoKategorie.MaxProEinwahl;
        if (dtoKategorie.Bezeichnung != kategorie.Bezeichnung)
            kategorie.Bezeichnung = dtoKategorie.Bezeichnung;
        if (dtoKategorie.ProfilProfundum != kategorie.ProfilProfundum)
            kategorie.ProfilProfundum = dtoKategorie.ProfilProfundum;

        await _dbContext.SaveChangesAsync();
        return kategorie;
    }

    public async Task DeleteKategorieAsync(Guid kategorieId)
    {
        await _dbContext.ProfundaKategorien.Where(k => k.Id == kategorieId).ExecuteDeleteAsync();
    }

    public Task<DTOProfundumKategorie[]> GetKategorienAsync()
    {
        return _dbContext.ProfundaKategorien.Select(k => new DTOProfundumKategorie(k)).ToArrayAsync();
    }

    public async Task<ProfundumDefinition?> CreateProfundumAsync(DTOProfundumDefinitionCreation dtoProfundum)
    {
        var kat = await _dbContext.ProfundaKategorien.FindAsync(dtoProfundum.KategorieId);
        if (kat is null)
        {
            return null;
        }

        var verantwortliche = await _dbContext.Personen
            .Where(p => dtoProfundum.VerantwortlicheIds.Contains(p.Id))
            .ToArrayAsync();

        var deps = await _dbContext.Profunda
            .Where(p => dtoProfundum.DependencyIds.Contains(p.Id))
            .ToListAsync();

        var def = new ProfundumDefinition
        {
            Bezeichnung = dtoProfundum.Bezeichnung,
            Beschreibung = dtoProfundum.Beschreibung,
            Kategorie = kat,
            Verantwortliche = verantwortliche,
            MinKlasse = dtoProfundum.MinKlasse,
            MaxKlasse = dtoProfundum.MaxKlasse,
            Dependencies = deps,
        };
        _dbContext.Profunda.Add(def);
        await _dbContext.SaveChangesAsync();
        return def;
    }

    public async Task<ProfundumDefinition?> UpdateProfundumAsync(Guid profundumId, DTOProfundumDefinitionCreation dtoProfundum)
    {
        var profundum = await _dbContext.Profunda
            .AsSplitQuery()
            .Include(p => p.Verantwortliche)
            .Include(p => p.Dependencies)
            .Where(p => p.Id == profundumId)
            .FirstOrDefaultAsync();
        if (profundum is null)
        {
            return null;
        }

        var deps = await _dbContext.Profunda
            .Where(p => dtoProfundum.DependencyIds.Contains(p.Id))
            .ToListAsync();
        profundum.Dependencies = deps;

        var verantwortliche = await _dbContext.Personen
            .Where(p => dtoProfundum.VerantwortlicheIds.Contains(p.Id))
            .ToListAsync();
        profundum.Verantwortliche = verantwortliche;

        if (dtoProfundum.Bezeichnung != profundum.Bezeichnung)
            profundum.Bezeichnung = dtoProfundum.Bezeichnung;
        if (dtoProfundum.Beschreibung != profundum.Beschreibung)
            profundum.Beschreibung = dtoProfundum.Beschreibung;
        profundum.MinKlasse = dtoProfundum.MinKlasse;
        profundum.MaxKlasse = dtoProfundum.MaxKlasse;

        var kat = await _dbContext.ProfundaKategorien.FindAsync(dtoProfundum.KategorieId);
        if (kat is null)
        {
            return null;
        }
        profundum.Kategorie = kat;

        await _dbContext.SaveChangesAsync();
        return profundum;
    }

    public Task DeleteProfundumAsync(Guid profundumId)
    {
        _dbContext.Profunda.Where(p => p.Id == profundumId).ExecuteDelete();
        return _dbContext.SaveChangesAsync();
    }

    public Task<DTOProfundumDefinition[]> GetProfundaAsync()
    {
        return _dbContext.Profunda
            .AsSplitQuery()
            .Include(p => p.Kategorie)
            .Include(p => p.Verantwortliche)
            .Include(p => p.Dependencies)
            .Select(p => new DTOProfundumDefinition(p))
            .ToArrayAsync();
    }

    public Task<DTOProfundumDefinition?> GetProfundumAsync(Guid profundumId)
    {
        return _dbContext.Profunda
            .AsSplitQuery()
            .Include(p => p.Kategorie)
            .Include(p => p.Verantwortliche)
            .Include(p => p.Dependencies)
            .Where(p => p.Id == profundumId)
            .Select(p => new DTOProfundumDefinition(p)).FirstOrDefaultAsync();
    }

    public async Task<ProfundumInstanz?> CreateInstanzAsync(DTOProfundumInstanzCreation dtoInstanz)
    {
        var def = await _dbContext.Profunda.FindAsync(dtoInstanz.ProfundumId);
        if (def is null)
        {
            _logger.LogError("no such profundum def");
            return null;
        }

        var inst = new ProfundumInstanz
        {
            Profundum = def,
            MaxEinschreibungen = dtoInstanz.MaxEinschreibungen,
            Slots = [],
        };
        _dbContext.ProfundaInstanzen.Add(inst);
        foreach (var s in dtoInstanz.Slots)
        {
            var slt = await _dbContext.ProfundaSlots.FindAsync(s);
            if (slt is null)
            {
                _logger.LogError("no such slot");
                return null;
            }

            inst.Slots.Add(slt);
        }

        await _dbContext.SaveChangesAsync();
        return inst;
    }

    public Task<DTOProfundumInstanz[]> GetInstanzenAsync()
    {
        return _dbContext.ProfundaInstanzen
            .AsSingleQuery()
            .Include(i => i.Profundum)
            .Include(i => i.Slots)
            .Select(i => new DTOProfundumInstanz(i))
            .ToArrayAsync();
    }

    public Task<DTOProfundumInstanz?> GetInstanzAsync(Guid instanzId)
    {
        return _dbContext.ProfundaInstanzen
            .AsSingleQuery()
            .Include(i => i.Profundum)
            .Include(i => i.Slots)
            .Where(i => i.Id == instanzId)
            .Select(i => new DTOProfundumInstanz(i))
            .FirstOrDefaultAsync();
    }

    public async Task<ProfundumInstanz?> UpdateInstanzAsync(Guid instanzId, DTOProfundumInstanzCreation patch)
    {
        var instanz = await _dbContext.ProfundaInstanzen
            .Include(i => i.Slots)
            .FirstOrDefaultAsync(i => i.Id == instanzId);

        if (instanz is null) return null;

        instanz.MaxEinschreibungen = patch.MaxEinschreibungen;

        // update slots
        instanz.Slots.Clear();
        foreach (var slotId in patch.Slots)
        {
            var slt = await _dbContext.ProfundaSlots.FindAsync(slotId);
            if (slt != null)
                instanz.Slots.Add(slt);
        }

        await _dbContext.SaveChangesAsync();
        return instanz;
    }

    public Task DeleteInstanzAsync(Guid instanzId)
    {
        _dbContext.ProfundaInstanzen.Where(i => i.Id == instanzId).ExecuteDelete();
        return _dbContext.SaveChangesAsync();
    }


    public Task<Dictionary<Guid, DTOProfundumEnrollment[]>> GetAllEnrollmentsAsync()
    {
        return _dbContext.ProfundaEinschreibungen
            .Include(e => e.BetroffenePerson)
            .GroupBy(e => e.BetroffenePersonId)
            .ToDictionaryAsync(e => e.Key,
                e => e.Select(ei => new DTOProfundumEnrollment(ei)).ToArray());
    }

    public async Task<byte[]?> GetInstanzPdfAsync(Guid instanzId)
    {
        var p = await _dbContext.ProfundaInstanzen
            .AsSplitQuery()
            .Include(i => i.Profundum).ThenInclude(p => p.Verantwortliche)
            .Include(i => i.Slots)
            .Where(i => i.Id == instanzId)
            .FirstOrDefaultAsync();


        if (p is null)
        {
            return null;
        }

        var teilnehmer = _dbContext.ProfundaEinschreibungen
            .Where(e => e.ProfundumInstanz.Id == p.Id)
            .Select(e => e.BetroffenePerson)
            .OrderBy(e => e.LastName)
            .ThenBy(e => e.FirstName);

        const string src = Typst.Templates.Profundum.Instanz;

        var inputs = new
        {
            bezeichnung = p.Profundum.Bezeichnung,
            beschreibung = p.Profundum.Beschreibung,
            slots = p.Slots.OrderBy(p => p.Jahr).ThenBy(p => p.Quartal).ThenBy(p => p.Wochentag),
            verantwortliche = p.Profundum.Verantwortliche.Select(v => new PersonInfoMinimal(v)),
            teilnehmer = teilnehmer.Select(v => new PersonInfoMinimal(v)),
        };

        return _typst.generatePdf(src, null, inputs);
    }
}

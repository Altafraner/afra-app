using Altafraner.AfraApp.Profundum.Domain.DTO;
using Altafraner.AfraApp.Profundum.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Altafraner.AfraApp.Profundum.Services;

/// <summary>
///     A service for managing profunda.
/// </summary>
public class ProfundumManagementService
{
    private readonly AfraAppContext _dbContext;
    private readonly ILogger _logger;

    /// <summary>
    ///     Constructs the ManagementService. Usually called by the DI container.
    /// </summary>
    public ProfundumManagementService(AfraAppContext dbContext,
        ILogger<ProfundumEnrollmentService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    ///
    public async Task<ProfundumEinwahlZeitraum> CreateEinwahlZeitraumAsync(DTOProfundumEinwahlZeitraum zeitraum)
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

    ///
    public Task<DTOProfundumEinwahlZeitraum[]> GetEinwahlZeiträumeAsync()
    {
        return _dbContext.ProfundumEinwahlZeitraeume
            .Select(e => new DTOProfundumEinwahlZeitraum(e))
            .ToArrayAsync();
    }


    ///
    public async Task<ProfundumSlot?> CreateSlotAsync(DTOProfundumSlot dtoSlot)
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

    ///
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

    ///
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

    ///
    public async Task DeleteKategorieAsync(Guid kategorieId)
    {
        _dbContext.ProfundaKategorien.Where(k => k.Id == kategorieId).ExecuteDelete();
        await _dbContext.SaveChangesAsync();
    }

    ///
    public Task<DTOProfundumKategorie[]> GetKategorienAsync()
    {
        return _dbContext.ProfundaKategorien.Select(k => new DTOProfundumKategorie(k)).ToArrayAsync();
    }

    ///
    public async Task<ProfundumDefinition?> CreateProfundumAsync(DTOProfundumDefinitionCreation dtoProfundum)
    {
        var kat = await _dbContext.ProfundaKategorien.FindAsync(dtoProfundum.KategorieId);
        if (kat is null)
        {
            return null;
        }

        var def = new ProfundumDefinition
        {
            Bezeichnung = dtoProfundum.Bezeichnung,
            Kategorie = kat,
            MinKlasse = dtoProfundum.minKlasse,
            MaxKlasse = dtoProfundum.maxKlasse
        };
        _dbContext.Profunda.Add(def);
        await _dbContext.SaveChangesAsync();
        return def;
    }

    ///
    public async Task<ProfundumDefinition?> UpdateProfundumAsync(Guid profundumId, DTOProfundumDefinitionCreation dtoProfundum)
    {
        var profundum = await _dbContext.Profunda.FindAsync(profundumId);
        if (profundum is null)
        {
            return null;
        }

        if (dtoProfundum.Bezeichnung != profundum.Bezeichnung)
            profundum.Bezeichnung = dtoProfundum.Bezeichnung;
        if (dtoProfundum.minKlasse != profundum.MinKlasse)
            profundum.MinKlasse = dtoProfundum.minKlasse;
        if (dtoProfundum.maxKlasse != profundum.MaxKlasse)
            profundum.MaxKlasse = dtoProfundum.maxKlasse;

        var kat = await _dbContext.ProfundaKategorien.FindAsync(dtoProfundum.KategorieId);
        if (kat is null)
        {
            return null;
        }
        profundum.Kategorie = kat;

        await _dbContext.SaveChangesAsync();
        return profundum;
    }

    ///
    public Task DeleteProfundumAsync(Guid profundumId)
    {
        _dbContext.Profunda.Where(p => p.Id == profundumId).ExecuteDelete();
        return _dbContext.SaveChangesAsync();
    }

    ///
    public Task<DTOProfundumDefinition[]> GetProfundaAsync()
    {
        return _dbContext.Profunda
            .Include(p => p.Kategorie)
            .Select(p => new DTOProfundumDefinition(p))
            .ToArrayAsync();
    }

    ///
    public async Task<ProfundumInstanz?> CreateInstanzAsync(DTOProfundumInstanz dtoInstanz)
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
}

using Altafraner.AfraApp.Domain;
using Altafraner.AfraApp.Profundum.Domain.DTO;
using Altafraner.AfraApp.Profundum.Domain.Models;
using Altafraner.AfraApp.User.Domain.DTO;
using Altafraner.AfraApp.User.Domain.Models;
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
    private readonly Altafraner.Typst.Typst _typst;

    /// <summary>
    ///     Constructs the ManagementService. Usually called by the DI container.
    /// </summary>
    public ProfundumManagementService(AfraAppContext dbContext,
        ILogger<ProfundumManagementService> logger,
        IOptions<TypstConfiguration> typstConfig,
        Altafraner.Typst.Typst typst
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

    public async Task UpdateEinwahlZeitraumAsync(Guid id, DTOProfundumEinwahlZeitraumCreation dto)
    {
        var zeitraum = await _dbContext.ProfundumEinwahlZeitraeume.FindAsync(id);
        if (zeitraum is null)
            throw new NotFoundException("referenced einwahlzeitraum not found");

        if (dto.EinwahlStart != null)
            zeitraum.EinwahlStart = DateTime.SpecifyKind(DateTime.Parse(dto.EinwahlStart), DateTimeKind.Utc);

        if (dto.EinwahlStop != null)
            zeitraum.EinwahlStop = DateTime.SpecifyKind(DateTime.Parse(dto.EinwahlStop), DateTimeKind.Utc);

        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteEinwahlZeitraumAsync(Guid id)
    {
        var numDeleted = await _dbContext.ProfundumEinwahlZeitraeume.Where(e => e.Id == id).ExecuteDeleteAsync();
        if (numDeleted == 0) throw new NotFoundException("no such einwahlzeitraum");
    }

    public async Task<DTOProfundumSlot[]> GetSlotsAsync()
    {
        return (await _dbContext.ProfundaSlots
            .Include(s => s.EinwahlZeitraum)
            .ToArrayAsync())
            .Order(new ProfundumSlotComparer())
            .Select(s => new DTOProfundumSlot(s))
            .ToArray();
    }

    public async Task<ProfundumSlot> CreateSlotAsync(DTOProfundumSlotCreation dtoSlot)
    {
        var zeitraum = await _dbContext.ProfundumEinwahlZeitraeume.FindAsync(dtoSlot.EinwahlZeitraumId);
        if (zeitraum is null)
        {
            throw new NotFoundException("referenced zeitraum not found");
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

    public async Task UpdateSlotAsync(Guid id, DTOProfundumSlotCreation dto)
    {
        var slot = await _dbContext.ProfundaSlots
            .Include(s => s.EinwahlZeitraum)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (slot is null)
        {
            throw new NotFoundException("slot to update not found");
        }

        slot.Jahr = dto.Jahr;
        slot.Quartal = dto.Quartal;
        slot.Wochentag = dto.Wochentag;

        if (dto.EinwahlZeitraumId != Guid.Empty && dto.EinwahlZeitraumId != slot.EinwahlZeitraum.Id)
        {
            var zeitraum = await _dbContext.ProfundumEinwahlZeitraeume.FindAsync(dto.EinwahlZeitraumId);
            if (zeitraum is null)
            {
                throw new NotFoundException("referenced zeitraum not found");
            }
            slot.EinwahlZeitraum = zeitraum;
        }

        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteSlotAsync(Guid id)
    {
        var numDeleted = await _dbContext.ProfundaSlots.Where(s => s.Id == id).ExecuteDeleteAsync();
        if (numDeleted == 0) throw new NotFoundException("no such slot");
    }

    public async Task<ProfundumKategorie> CreateKategorieAsync(DTOProfundumKategorieCreation dtoKategorie)
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
        var numDeleted = await _dbContext.ProfundaKategorien.Where(k => k.Id == kategorieId).ExecuteDeleteAsync();
        if (numDeleted == 0) throw new NotFoundException("no such kateorie");
    }

    public Task<DTOProfundumKategorie[]> GetKategorienAsync()
    {
        return _dbContext.ProfundaKategorien.Select(k => new DTOProfundumKategorie(k)).ToArrayAsync();
    }

    public async Task<ProfundumDefinition> CreateProfundumAsync(DTOProfundumDefinitionCreation dtoProfundum)
    {
        var kat = await _dbContext.ProfundaKategorien.FindAsync(dtoProfundum.KategorieId);
        if (kat is null)
            throw new NotFoundException("referenced kategorie not found");

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

    public async Task<ProfundumDefinition> UpdateProfundumAsync(Guid profundumId, DTOProfundumDefinitionCreation dtoProfundum)
    {
        var profundum = await _dbContext.Profunda
            .AsSplitQuery()
            .Include(p => p.Verantwortliche)
            .Include(p => p.Dependencies)
            .Where(p => p.Id == profundumId)
            .FirstOrDefaultAsync();
        if (profundum is null)
            throw new NotFoundException("profundum to update not found");

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
            throw new NotFoundException("referenced kategorie not found");
        profundum.Kategorie = kat;

        await _dbContext.SaveChangesAsync();
        return profundum;
    }

    public async Task DeleteProfundumAsync(Guid profundumId)
    {
        var numDeleted = await _dbContext.Profunda.Where(p => p.Id == profundumId).ExecuteDeleteAsync();
        if (numDeleted == 0) throw new NotFoundException("no such profundum");
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

    public async Task<ProfundumInstanz> CreateInstanzAsync(DTOProfundumInstanzCreation dtoInstanz)
    {
        var def = await _dbContext.Profunda.FindAsync(dtoInstanz.ProfundumId);
        if (def is null)
        {
            throw new NotFoundException("referenced profundum not found");
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
                throw new NotFoundException("referenced slot not found");
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
            .Include(i => i.Profundum).ThenInclude(p => p.Verantwortliche)
            .Include(i => i.Profundum).ThenInclude(p => p.Dependencies)
            .Include(i => i.Profundum).ThenInclude(p => p.Kategorie)
            .Include(i => i.Slots)
            .Include(i => i.Einschreibungen).ThenInclude(e => e.BetroffenePerson)
            .Select(i => new DTOProfundumInstanz(i))
            .ToArrayAsync();
    }

    public Task<DTOProfundumInstanz?> GetInstanzAsync(Guid instanzId)
    {
        return _dbContext.ProfundaInstanzen
            .AsSingleQuery()
            .Include(i => i.Profundum).ThenInclude(p => p.Verantwortliche)
            .Include(i => i.Profundum).ThenInclude(p => p.Dependencies)
            .Include(i => i.Profundum).ThenInclude(p => p.Kategorie)
            .Include(i => i.Slots)
            .Where(i => i.Id == instanzId)
            .Select(i => new DTOProfundumInstanz(i))
            .FirstOrDefaultAsync();
    }

    public async Task<ProfundumInstanz> UpdateInstanzAsync(Guid instanzId, DTOProfundumInstanzCreation patch)
    {
        var instanz = await _dbContext.ProfundaInstanzen
            .Include(i => i.Slots)
            .FirstOrDefaultAsync(i => i.Id == instanzId);

        if (instanz is null) throw new NotFoundException("instanz to update not found");

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

    public async Task DeleteInstanzAsync(Guid instanzId)
    {
        var numDeleted = await _dbContext.ProfundaInstanzen.Where(i => i.Id == instanzId).ExecuteDeleteAsync();
        if (numDeleted == 0) throw new NotFoundException("no such instanz");
    }



    public async Task UpdateEnrollmentsAsync(Guid personId, List<DTOProfundumEnrollment> enrollments)
    {
        var existing = _dbContext.ProfundaEinschreibungen
            .Where(e => e.BetroffenePersonId == personId);

        _dbContext.ProfundaEinschreibungen.RemoveRange(existing);

        var person = _dbContext.Personen.Find(personId);
        if (person is null)
        {
            throw new ArgumentException();
        }


        foreach (var e in enrollments)
        {
            var instanz = _dbContext.ProfundaInstanzen.Find(e.ProfundumInstanzId);
            if (instanz is null)
            {
                throw new ArgumentException();
            }
            var slot = _dbContext.ProfundaSlots.Find(e.ProfundumSlotId);
            if (slot is null)
            {
                throw new ArgumentException();
            }
            _dbContext.ProfundaEinschreibungen.Add(new ProfundumEinschreibung
            {
                BetroffenePerson = person,
                ProfundumInstanz = instanz,
                Slot = slot,
                IsFixed = e.IsFixed
            });
        }

        await _dbContext.SaveChangesAsync();
    }

    public async Task<byte[]> GetInstanzPdfAsync(Guid instanzId)
    {
        var p = await _dbContext.ProfundaInstanzen
            .AsSplitQuery()
            .Include(i => i.Profundum).ThenInclude(p => p.Verantwortliche)
            .Include(i => i.Profundum).ThenInclude(p => p.Dependencies)
            .Include(i => i.Slots)
            .Where(i => i.Id == instanzId)
            .FirstOrDefaultAsync();


        if (p is null)
        {
            throw new NotFoundException("instanz not found");
        }

        var teilnehmer = _dbContext.ProfundaEinschreibungen
            .Where(e => e.ProfundumInstanz.Id == p.Id)
            .Select(e => e.BetroffenePerson)
            .Distinct()
            .OrderBy(p => p.LastName)
            .ThenBy(p => p.FirstName);

        const string src = Altafraner.Typst.Templates.Profundum.Instanz;

        var inputs = new
        {
            bezeichnung = p.Profundum.Bezeichnung,
            beschreibung = p.Profundum.Beschreibung,
            voraussetzungen = p.Profundum.Dependencies.Select(d => d.Bezeichnung),
            slots = p.Slots.OrderBy(p => p.Jahr).ThenBy(p => p.Quartal).ThenBy(p => p.Wochentag),
            verantwortliche = p.Profundum.Verantwortliche.Select(v => new PersonInfoMinimal(v)),
            teilnehmer = teilnehmer.Select(v => new PersonInfoMinimal(v)),
        };

        return _typst.generatePdf(src, inputs);
    }
}

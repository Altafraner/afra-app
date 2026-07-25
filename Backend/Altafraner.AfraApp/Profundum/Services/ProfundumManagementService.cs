using System.IO.Compression;
using System.Text;
using Altafraner.AfraApp.Domain;
using Altafraner.AfraApp.Profundum.Domain.DTO;
using Altafraner.AfraApp.Profundum.Domain.Models;
using Altafraner.AfraApp.User.Domain.DTO;
using Altafraner.AfraApp.User.Domain.Models;
using Altafraner.AfraApp.User.Services;
using Altafraner.Backbone.Utils;
using Microsoft.EntityFrameworkCore;

namespace Altafraner.AfraApp.Profundum.Services;

/// <summary>
///     A service for managing profunda.
/// </summary>
internal class ProfundumManagementService
{
    private readonly AfraAppContext _dbContext;
    private readonly Altafraner.Typst.Typst _typst;
    private readonly UserService _userService;

    /// <summary>
    ///     Constructs the ManagementService. Usually called by the DI container.
    /// </summary>
    public ProfundumManagementService(AfraAppContext dbContext,
        Altafraner.Typst.Typst typst,
        UserService userService
        )
    {
        _dbContext = dbContext;
        _typst = typst;
        _userService = userService;
    }

    /// <summary>Creates a new EinwahlZeitraum (enrollment submission window).</summary>
    public async Task<ProfundumEinwahlZeitraum> CreateEinwahlZeitraumAsync(DTOProfundumEinwahlZeitraumCreation zeitraum)
    {
        if (zeitraum.EinwahlStart is null || zeitraum.EinwahlStop is null)
        {
            throw new ArgumentNullException();
        }

        var einwahlZeitraum = new ProfundumEinwahlZeitraum
        {
            EinwahlStart = DateTimeOffset.Parse(zeitraum.EinwahlStart).UtcDateTime,
            EinwahlStop = DateTimeOffset.Parse(zeitraum.EinwahlStop).UtcDateTime,
        };
        _dbContext.ProfundumEinwahlZeitraeume.Add(einwahlZeitraum);
        await _dbContext.SaveChangesAsync();
        return einwahlZeitraum;
    }

    /// <summary>Returns every EinwahlZeitraum.</summary>
    public Task<DTOProfundumEinwahlZeitraum[]> GetEinwahlZeiträumeAsync()
    {
        return _dbContext.ProfundumEinwahlZeitraeume
            .Select(e => new DTOProfundumEinwahlZeitraum(e))
            .ToArrayAsync();
    }

    /// <summary>Updates the given EinwahlZeitraum's start/stop times.</summary>
    public async Task UpdateEinwahlZeitraumAsync(Guid id, DTOProfundumEinwahlZeitraumCreation dto)
    {
        var zeitraum = await _dbContext.ProfundumEinwahlZeitraeume.FindAsync(id);
        if (zeitraum is null)
            throw new NotFoundException("referenced einwahlzeitraum not found");

        if (dto.EinwahlStart != null)
            zeitraum.EinwahlStart = DateTimeOffset.Parse(dto.EinwahlStart).UtcDateTime;

        if (dto.EinwahlStop != null)
            zeitraum.EinwahlStop = DateTimeOffset.Parse(dto.EinwahlStop).UtcDateTime;

        await _dbContext.SaveChangesAsync();
    }

    /// <summary>Deletes the given EinwahlZeitraum.</summary>
    public async Task DeleteEinwahlZeitraumAsync(Guid id)
    {
        var numDeleted = await _dbContext.ProfundumEinwahlZeitraeume.Where(e => e.Id == id).ExecuteDeleteAsync();
        if (numDeleted == 0) throw new NotFoundException("no such einwahlzeitraum");
    }

    /// <summary>Returns every Slot, ordered by <see cref="ProfundumSlotComparer" />.</summary>
    public async Task<DTOProfundumSlot[]> GetSlotsAsync()
    {
        return (await _dbContext.ProfundaSlots
            .Include(s => s.EinwahlZeitraum)
            .ToArrayAsync())
            .Order(new ProfundumSlotComparer())
            .Select(s => new DTOProfundumSlot(s))
            .ToArray();
    }

    /// <summary>Creates a new Slot within the given EinwahlZeitraum.</summary>
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

    /// <summary>Updates the given Slot, optionally moving it to a different EinwahlZeitraum.</summary>
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

    /// <summary>Deletes the given Slot.</summary>
    public async Task DeleteSlotAsync(Guid id)
    {
        var numDeleted = await _dbContext.ProfundaSlots.Where(s => s.Id == id).ExecuteDeleteAsync();
        if (numDeleted == 0) throw new NotFoundException("no such slot");
    }

    /// <summary>Returns every Termin of the given Slot, ordered by day.</summary>
    public async Task<DTOProfundumTermin[]> GetTermineAsync(Guid slotId)
    {
        return await _dbContext.ProfundaTermine
            .Where(t => t.SlotId == slotId)
            .OrderBy(t => t.Day)
            .Select(t => new DTOProfundumTermin(t))
            .ToArrayAsync();
    }

    /// <summary>Creates a new Termin for the given Slot.</summary>
    public async Task CreateTerminAsync(Guid slotId, DTOProfundumTerminCreation dto)
    {
        var slot = await _dbContext.ProfundaSlots.FindAsync(slotId);
        if (slot is null)
            throw new NotFoundException("referenced slot not found");

        _dbContext.ProfundaTermine.Add(new ProfundumTermin
        {
            Slot = slot,
            Day = dto.Day,
            StartTime = dto.StartTime,
            EndTime = dto.EndTime,
        });
        await _dbContext.SaveChangesAsync();
    }

    /// <summary>Updates the given Termin (identified by its Slot and original day), including moving it to a new day.</summary>
    public async Task UpdateTerminAsync(Guid slotId, DateOnly originalDay, DTOProfundumTerminCreation dto)
    {
        var termin = await _dbContext.ProfundaTermine
            .FirstOrDefaultAsync(t => t.SlotId == slotId && t.Day == originalDay);
        if (termin is null)
            throw new NotFoundException("termin to update not found");

        if (dto.Day != originalDay)
        {
            var slot = await _dbContext.ProfundaSlots.FindAsync(slotId);
            if (slot is null)
                throw new NotFoundException("referenced slot not found");

            _dbContext.ProfundaTermine.Remove(termin);
            _dbContext.ProfundaTermine.Add(new ProfundumTermin
            {
                Slot = slot,
                Day = dto.Day,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
            });
        }
        else
        {
            termin.StartTime = dto.StartTime;
            termin.EndTime = dto.EndTime;
        }

        await _dbContext.SaveChangesAsync();
    }

    /// <summary>Deletes the given Termin.</summary>
    public async Task DeleteTerminAsync(Guid slotId, DateOnly day)
    {
        var numDeleted = await _dbContext.ProfundaTermine
            .Where(t => t.SlotId == slotId && t.Day == day)
            .ExecuteDeleteAsync();
        if (numDeleted == 0) throw new NotFoundException("no such termin");
    }

    /// <summary>Creates a new Kategorie.</summary>
    public async Task<ProfundumKategorie> CreateKategorieAsync(DTOProfundumKategorieCreation dtoKategorie)
    {
        var kategorie = new ProfundumKategorie
        {
            Bezeichnung = dtoKategorie.Bezeichnung,
            ProfilProfundum = dtoKategorie.ProfilProfundum,
        };

        _dbContext.ProfundaKategorien.Add(kategorie);
        await _dbContext.SaveChangesAsync();
        return kategorie;
    }

    /// <summary>Updates the given Kategorie's Bezeichnung/ProfilProfundum flag.</summary>
    public async Task<ProfundumKategorie?> UpdateKategorieAsync(Guid kategorieId, DTOProfundumKategorieCreation dtoKategorie)
    {
        var kategorie = await _dbContext.ProfundaKategorien.FindAsync(kategorieId);
        if (kategorie is null)
        {
            throw new ArgumentException();
        }

        if (dtoKategorie.Bezeichnung != kategorie.Bezeichnung)
            kategorie.Bezeichnung = dtoKategorie.Bezeichnung;
        if (dtoKategorie.ProfilProfundum != kategorie.ProfilProfundum)
            kategorie.ProfilProfundum = dtoKategorie.ProfilProfundum;

        await _dbContext.SaveChangesAsync();
        return kategorie;
    }

    /// <summary>Deletes the given Kategorie.</summary>
    public async Task DeleteKategorieAsync(Guid kategorieId)
    {
        var numDeleted = await _dbContext.ProfundaKategorien.Where(k => k.Id == kategorieId).ExecuteDeleteAsync();
        if (numDeleted == 0) throw new NotFoundException("no such kategorie");
    }

    /// <summary>Returns every Kategorie.</summary>
    public Task<DTOProfundumKategorie[]> GetKategorienAsync()
    {
        return _dbContext.ProfundaKategorien.Select(k => new DTOProfundumKategorie(k)).ToArrayAsync();
    }

    /// <summary>Creates a new Profundum-Definition, resolving its Kategorie/Dependencies/Fachbereiche by id.</summary>
    public async Task<ProfundumDefinition> CreateProfundumAsync(DTOProfundumDefinitionCreation dtoProfundum)
    {
        var kat = await _dbContext.ProfundaKategorien.FindAsync(dtoProfundum.KategorieId);
        if (kat is null)
            throw new NotFoundException("referenced kategorie not found");

        var deps = await _dbContext.Profunda
            .Where(p => dtoProfundum.DependencyIds.Contains(p.Id))
            .ToListAsync();

        var fachbereiche = await _dbContext.ProfundaFachbereiche.Where(e => dtoProfundum.FachbereichIds.Contains(e.Id))
            .ToListAsync();
        if (fachbereiche.Count != dtoProfundum.FachbereichIds.Count)
            throw new KeyNotFoundException("At least one fachbereich does not exist");

        var def = new ProfundumDefinition
        {
            Bezeichnung = dtoProfundum.Bezeichnung,
            Beschreibung = dtoProfundum.Beschreibung,
            Kategorie = kat,
            MinKlasse = dtoProfundum.MinKlasse,
            MaxKlasse = dtoProfundum.MaxKlasse,
            Dependencies = deps,
            Fachbereiche = fachbereiche,
            ErlaubtPartnerwahl = dtoProfundum.ErlaubtPartnerwahl,
        };
        _dbContext.Profunda.Add(def);
        await _dbContext.SaveChangesAsync();
        return def;
    }

    /// <summary>Updates a Profundum-Definition's fields, including its Kategorie/Dependencies/Fachbereiche.</summary>
    public async Task<ProfundumDefinition> UpdateProfundumAsync(Guid profundumId, DTOProfundumDefinitionCreation dtoProfundum)
    {
        var profundum = await _dbContext.Profunda
            .AsSplitQuery()
            .Include(p => p.Dependencies)
            .Include(p => p.Fachbereiche)
            .Where(p => p.Id == profundumId)
            .FirstOrDefaultAsync();
        if (profundum is null)
            throw new NotFoundException("profundum to update not found");

        var deps = await _dbContext.Profunda
            .Where(p => dtoProfundum.DependencyIds.Contains(p.Id))
            .ToListAsync();

        var fachbereiche = await _dbContext.ProfundaFachbereiche.Where(e => dtoProfundum.FachbereichIds.Contains(e.Id))
            .ToListAsync();
        if (fachbereiche.Count != dtoProfundum.FachbereichIds.Count)
            throw new KeyNotFoundException("At least one fachbereich does not exist");

        profundum.Fachbereiche = fachbereiche;
        profundum.Dependencies = deps;

        if (dtoProfundum.Bezeichnung != profundum.Bezeichnung)
            profundum.Bezeichnung = dtoProfundum.Bezeichnung;
        if (dtoProfundum.Beschreibung != profundum.Beschreibung)
            profundum.Beschreibung = dtoProfundum.Beschreibung;
        profundum.MinKlasse = dtoProfundum.MinKlasse;
        profundum.MaxKlasse = dtoProfundum.MaxKlasse;
        profundum.ErlaubtPartnerwahl = dtoProfundum.ErlaubtPartnerwahl;

        var kat = await _dbContext.ProfundaKategorien.FindAsync(dtoProfundum.KategorieId);
        if (kat is null)
            throw new NotFoundException("referenced kategorie not found");
        profundum.Kategorie = kat;

        await _dbContext.SaveChangesAsync();
        return profundum;
    }

    /// <summary>Deletes the given Profundum-Definition.</summary>
    public async Task DeleteProfundumAsync(Guid profundumId)
    {
        var numDeleted = await _dbContext.Profunda.Where(p => p.Id == profundumId).ExecuteDeleteAsync();
        if (numDeleted == 0) throw new NotFoundException("no such profundum");
    }

    /// <summary>Returns every Profundum-Definition, ordered by Bezeichnung.</summary>
    public Task<DTOProfundumDefinition[]> GetProfundaAsync()
    {
        return _dbContext.Profunda
            .AsSplitQuery()
            .Include(p => p.Kategorie)
            .Include(p => p.Dependencies)
            .Include(e => e.Fachbereiche)
            .OrderBy(p => p.Bezeichnung.ToLower())
            .Select(p => new DTOProfundumDefinition(p))
            .ToArrayAsync();
    }

    /// <summary>Returns a single Profundum-Definition by id, or null if it doesn't exist.</summary>
    public Task<DTOProfundumDefinition?> GetProfundumAsync(Guid profundumId)
    {
        return _dbContext.Profunda
            .AsSplitQuery()
            .Include(p => p.Kategorie)
            .Include(p => p.Dependencies)
            .Include(e => e.Fachbereiche)
            .Where(p => p.Id == profundumId)
            .Select(p => new DTOProfundumDefinition(p)).FirstOrDefaultAsync();
    }

    /// <summary>Creates a new Instanz (offering) of a Profundum-Definition, with its Slots and Verantwortliche.</summary>
    public async Task<ProfundumInstanz> CreateInstanzAsync(DTOProfundumInstanzCreation request)
    {
        var def = await _dbContext.Profunda.FindAsync(request.ProfundumId);
        if (def is null)
            throw new NotFoundException("referenced profundum not found");

        var verantwortliche =
            await _dbContext.Personen.Where(p => request.VerantwortlicheIds.Contains(p.Id)).ToListAsync();
        if (verantwortliche.Count != request.VerantwortlicheIds.Count)
            throw new NotFoundException("At least one of the tutors does not exist");

        if (request.Slots.Count == 0)
            throw new ArgumentOutOfRangeException(nameof(request.Slots), "At least one slot is required");

        var slots = await _dbContext.ProfundaSlots.Where(slot => request.Slots.Contains(slot.Id)).ToListAsync();
        if (slots.Count != request.Slots.Count)
        {
            throw new NotFoundException("At least one of the slots does not exist");
        }

        var inst = new ProfundumInstanz
        {
            Profundum = def,
            MaxEinschreibungen = request.MaxEinschreibungen,
            WantedEinschreibungen = request.WantedEinschreibungen,
            Slots = slots,
            Ort = request.Ort,
            Verantwortliche = verantwortliche
        };
        await _dbContext.ProfundaInstanzen.AddAsync(inst);
        await _dbContext.SaveChangesAsync();
        return inst;
    }

    /// <summary>Returns every Instanz, ordered by the owning Profundum's Bezeichnung.</summary>
    public Task<DTOProfundumInstanz[]> GetInstanzenAsync()
    {
        return _dbContext.ProfundaInstanzen
            .AsSplitQuery()
            .Include(p => p.Verantwortliche)
            .Include(i => i.Profundum).ThenInclude(p => p.Dependencies)
            .Include(i => i.Profundum).ThenInclude(p => p.Kategorie)
            .Include(i => i.Profundum)
            .ThenInclude(p => p.Fachbereiche)
            .Include(i => i.Slots)
            .Include(i => i.Einschreibungen)
            .OrderBy(i => i.Profundum.Bezeichnung.ToLower())
            .Select(i => new DTOProfundumInstanz(i))
            .ToArrayAsync();
    }

    /// <summary>Returns a single Instanz by id, or null if it doesn't exist.</summary>
    public Task<DTOProfundumInstanz?> GetInstanzAsync(Guid instanzId)
    {
        return _dbContext.ProfundaInstanzen
            .AsSplitQuery()
            .Include(p => p.Verantwortliche)
            .Include(i => i.Profundum).ThenInclude(p => p.Dependencies)
            .Include(i => i.Profundum).ThenInclude(p => p.Kategorie)
            .Include(i => i.Profundum)
            .ThenInclude(p => p.Fachbereiche)
            .Include(i => i.Slots)
            .Include(i => i.Einschreibungen)
            .Where(i => i.Id == instanzId)
            .Select(i => new DTOProfundumInstanz(i))
            .FirstOrDefaultAsync();
    }

    /// <summary>Updates an Instanz's Slots, Verantwortliche, capacity, and Ort.</summary>
    public async Task<ProfundumInstanz> UpdateInstanzAsync(Guid instanzId, DTOProfundumInstanzCreation patch)
    {
        var instanz = await _dbContext.ProfundaInstanzen
            .AsSplitQuery()
            .Include(i => i.Slots)
            .Include(i => i.Verantwortliche)
            .FirstOrDefaultAsync(i => i.Id == instanzId);

        if (instanz is null) throw new NotFoundException("instanz to update not found");

        var verantwortliche =
            await _dbContext.Personen.Where(p => patch.VerantwortlicheIds.Contains(p.Id)).ToArrayAsync();
        if (verantwortliche.Length != patch.VerantwortlicheIds.Count)
            throw new NotFoundException("At least one of the tutors does not exist");

        if (patch.Slots.Count == 0)
            throw new ArgumentOutOfRangeException(nameof(patch.Slots), "At least one slot is required");

        var slots = await _dbContext.ProfundaSlots.Where(slot => patch.Slots.Contains(slot.Id)).ToArrayAsync();
        if (slots.Length != patch.Slots.Count) throw new NotFoundException("At least one of the slots does not exist");

        instanz.Slots = slots.ToList();

        var verantwortlicheIds = verantwortliche.Select(e => e.Id).ToArray();
        instanz.Verantwortliche.RemoveAll(v => !verantwortlicheIds.Contains(v.Id));
        var instanzVerantwortlicheIds = instanz.Verantwortliche.Select(v => v.Id).ToArray();
        instanz.Verantwortliche.AddRange(verantwortliche.Where(v => !instanzVerantwortlicheIds.Contains(v.Id)));

        instanz.MaxEinschreibungen = patch.MaxEinschreibungen;
        instanz.WantedEinschreibungen = patch.WantedEinschreibungen;
        instanz.Ort = patch.Ort;

        await _dbContext.SaveChangesAsync();
        return instanz;
    }

    /// <summary>Deletes the given Instanz.</summary>
    public async Task DeleteInstanzAsync(Guid instanzId)
    {
        var numDeleted = await _dbContext.ProfundaInstanzen.Where(i => i.Id == instanzId).ExecuteDeleteAsync();
        if (numDeleted == 0) throw new NotFoundException("no such instanz");
    }

    /// <summary>
    ///     Manual staff override: replaces a single student's entire set of <see cref="ProfundumEinschreibung" />
    ///     rows directly, bypassing the matching solver and its rules (including team-partner pairing - see
    ///     <c>rules-engine.md</c> on how a resulting desync is surfaced instead of blocked here).
    /// </summary>
    public async Task UpdateEnrollmentsAsync(Guid personId, List<DTOProfundumEnrollment> enrollments)
    {
        var existing = _dbContext.ProfundaEinschreibungen
            .Where(e => e.BetroffenePersonId == personId);

        _dbContext.ProfundaEinschreibungen.RemoveRange(existing);

        var person = await _dbContext.Personen.FindAsync(personId);
        if (person is null)
        {
            throw new ArgumentException();
        }


        foreach (var e in enrollments)
        {
            ProfundumInstanz? instanz;
            if (e.ProfundumInstanzId is not null)
            {
                instanz = await _dbContext.ProfundaInstanzen.FindAsync(e.ProfundumInstanzId);
                if (instanz is null) throw new ArgumentException();
            }
            else
            {
                instanz = null;
            }

            var slot = await _dbContext.ProfundaSlots.FindAsync(e.ProfundumSlotId);
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

    /// <summary>Renders a single Instanz's roster/course sheet as a PDF via <c>Typst/Profundum/Instanz.typ</c>.</summary>
    public async Task<byte[]> GetInstanzPdfAsync(Guid instanzId)
    {
        var p = await _dbContext.ProfundaInstanzen
            .AsSplitQuery()
            .Include(p => p.Verantwortliche)
            .Include(i => i.Profundum).ThenInclude(p => p.Dependencies)
            .Include(i => i.Slots)
            .Where(i => i.Id == instanzId)
            .FirstOrDefaultAsync();


        if (p is null)
        {
            throw new NotFoundException("instanz not found");
        }

        var einschreibungenForInstanz = await _dbContext.ProfundaEinschreibungen
            .Where(e => e.ProfundumInstanz != null && e.ProfundumInstanz.Id == p.Id)
            .Include(e => e.BetroffenePerson)
            .ToArrayAsync();

        var teilnehmer = einschreibungenForInstanz
            .GroupBy(e => e.BetroffenePerson)
            .Select(g => new PersonInfoMinimal(g.Key)
            {
                Gruppe = _userService.GetGruppe(g.Key, g.Min(e => e.CreatedAt)),
            })
            .OrderBy(x => int.Parse((x.Gruppe ?? "0").TakeWhile(char.IsDigit).ToArray()))
            .ThenBy(x =>
                (x.Gruppe ?? "").SkipWhile(c => !char.IsDigit(c))
                .Aggregate(new StringBuilder(), (a, b) => a.Append(b))
                .ToString())
            .ThenBy(e => e.Nachname)
            .ThenBy(e => e.Vorname);

        const string src = Altafraner.Typst.Templates.Profundum.Instanz;

        var inputs = new
        {
            bezeichnung = p.Profundum.Bezeichnung,
            beschreibung = "",
            voraussetzungen = p.Profundum.Dependencies.Select(d => d.Bezeichnung),
            ort = p.Ort,
            slots = p.Slots.OrderBy(e => e.Jahr).ThenBy(e => e.Quartal).ThenBy(e => e.Wochentag),
            verantwortliche = p.Verantwortliche.Select(v => new PersonInfoMinimal(v)),
            teilnehmer,
        };

        return _typst.GeneratePdf(src, inputs);
    }
    /// <summary>Renders every Instanz roster/course sheet for the given Slot as PDFs, bundled into one zip.</summary>
    public async Task<(byte[], string)> GetSlotPdfsZipAsync(Guid slotId)
    {
        var slot = await _dbContext.ProfundaSlots.FindAsync(slotId);
        if (slot is null)
        {
            throw new NotFoundException("no such slot");
        }

        var instanzen = await _dbContext.ProfundaInstanzen
            .AsSplitQuery()
            .Include(p => p.Verantwortliche)
            .Include(i => i.Profundum).ThenInclude(p => p.Dependencies)
            .Include(i => i.Slots)
            .Where(i => i.Slots.Any(s => s.Id == slotId))
            .ToListAsync();

        var einschreibungen = await _dbContext.ProfundaEinschreibungen
            .Include(e => e.BetroffenePerson)
            .Where(e => e.ProfundumInstanz != null && instanzen.Select(i => i.Id).Contains(e.ProfundumInstanz.Id))
            .ToListAsync();

        const string src = Altafraner.Typst.Templates.Profundum.Instanz;

        var jobs = instanzen.Select(inst =>
        {
            var teilnehmer = einschreibungen
                .Where(e => e.ProfundumInstanz!.Id == inst.Id)
                .GroupBy(e => e.BetroffenePerson)
                .Select(g => new PersonInfoMinimal(g.Key)
                {
                    Gruppe = _userService.GetGruppe(g.Key, g.Min(e => e.CreatedAt)),
                })
                .OrderBy(x => int.Parse((x.Gruppe ?? "0").TakeWhile(char.IsDigit).ToArray()))
                .ThenBy(x =>
                    (x.Gruppe ?? "").SkipWhile(c => !char.IsDigit(c))
                    .Aggregate(new StringBuilder(), (a, b) => a.Append(b)).ToString())
                .ThenBy(e => e.Nachname)
                .ThenBy(e => e.Vorname)
                .ToArray();

            var inputs = new
            {
                bezeichnung = inst.Profundum.Bezeichnung,
                beschreibung = "",
                voraussetzungen = inst.Profundum.Dependencies.Select(d => d.Bezeichnung),
                ort = inst.Ort,
                slots = inst.Slots.OrderBy(e => e.Jahr).ThenBy(e => e.Quartal).ThenBy(e => e.Wochentag),
                verantwortliche = inst.Verantwortliche.Select(v => new PersonInfoMinimal(v)),
                teilnehmer
            };

            var sanitizedName = FilenameSanitizer.Sanitize(inst.Profundum.Bezeichnung);
            var fname = $"{sanitizedName}.pdf";

            return (fname, inputs);
        }).ToList();

        var pdfTasks = jobs.Select(job => Task.Run(() => (job.fname,
                        pdf: _typst.GeneratePdf(src, job.inputs))));

        var results = await Task.WhenAll(pdfTasks);

        using var ms = new MemoryStream();

        await using (var archive = new ZipArchive(ms, ZipArchiveMode.Create, leaveOpen: true))
        {
            foreach (var (fname, pdf) in results)
            {
                var entry = archive.CreateEntry(fname);
                await using var entryStream = await entry.OpenAsync();
                await entryStream.WriteAsync(pdf);
            }
        }
        return (ms.ToArray(), slot.ToString());
    }

    /// <summary>Tab-separated export of every Mittelstufe student's fixed enrollment per Slot, one row per student.</summary>
    public async Task<string> GetStudentMatchingCsv()
    {
        var personen = _dbContext.Personen
            .AsSplitQuery()
            .Include(s => s.ProfundaEinschreibungen)
            .ThenInclude(e => e.ProfundumInstanz)
            .ThenInclude(e => e!.Profundum)
            .Include(person => person.ProfundaEinschreibungen).ThenInclude(profundumEinschreibung => profundumEinschreibung.ProfundumInstanz)
            .Include(person => person.ProfundaEinschreibungen).ThenInclude(profundumEinschreibung => profundumEinschreibung.ProfundumInstanz)
            .Where(p => p.Rolle == Rolle.Mittelstufe)
            .ToAsyncEnumerable()
            .OrderBy(x => int.Parse((x.Gruppe ?? "0").TakeWhile(c => char.IsDigit(c)).ToArray()))
            .ThenBy(x => (x.Gruppe ?? "").SkipWhile(c => !char.IsDigit(c)).Aggregate(new StringBuilder(), (a, b) => a.Append(b)).ToString())
            ;

        var slots = (await _dbContext.ProfundaSlots
            .ToArrayAsync())
            .Order(new ProfundumSlotComparer())
            .ToArray();

        const char sep = '\t';

        var sb = new StringBuilder();
        sb.AppendLine($"Klasse{sep} Name{sep} Vorname{slots.Select(s => s.ToString()).Aggregate("", (r, c) => $"{r}{sep} {c}")}");

        await foreach (var student in personen)
        {
            sb.AppendLine($"{student.Gruppe}{sep} {student.LastName}{sep} {student.FirstName}{slots.Select(s =>
                student.ProfundaEinschreibungen
                    .Where(e => e.IsFixed)
                    .Where(e => e.Slot == s)
                    .Select(e => e.ProfundumInstanz == null ? "" : e.ProfundumInstanz.Profundum.Bezeichnung)
                    .FirstOrDefault(defaultValue: "")
            ).Aggregate("", (r, c) => $"{r}{sep} {c}")}");
        }

        return sb.ToString();
    }
}

using System.IO.Compression;
using System.Text;
using Altafraner.AfraApp.Domain;
using Altafraner.AfraApp.Profundum.Domain.DTO;
using Altafraner.AfraApp.Profundum.Domain.Models;
using Altafraner.AfraApp.User.Domain.DTO;
using Altafraner.AfraApp.User.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Altafraner.AfraApp.Profundum.Services;

/// <summary>
///     A service for managing profunda.
/// </summary>
internal class ProfundumManagementService
{
    private readonly AfraAppContext _dbContext;
    private readonly Altafraner.Typst.Typst _typst;

    /// <summary>
    ///     Constructs the ManagementService. Usually called by the DI container.
    /// </summary>
    public ProfundumManagementService(AfraAppContext dbContext,
        Altafraner.Typst.Typst typst
        )
    {
        _dbContext = dbContext;
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
            EinwahlStart = DateTimeOffset.Parse(zeitraum.EinwahlStart).UtcDateTime,
            EinwahlStop = DateTimeOffset.Parse(zeitraum.EinwahlStop).UtcDateTime,
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
            zeitraum.EinwahlStart = DateTimeOffset.Parse(dto.EinwahlStart).UtcDateTime;

        if (dto.EinwahlStop != null)
            zeitraum.EinwahlStop = DateTimeOffset.Parse(dto.EinwahlStop).UtcDateTime;

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
            Fachbereiche = fachbereiche
        };
        _dbContext.Profunda.Add(def);
        await _dbContext.SaveChangesAsync();
        return def;
    }

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
            .Include(p => p.Dependencies)
            .Include(e => e.Fachbereiche)
            .OrderBy(p => p.Bezeichnung.ToLower())
            .Select(p => new DTOProfundumDefinition(p))
            .ToArrayAsync();
    }

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
            Slots = slots,
            Ort = request.Ort,
            Verantwortliche = verantwortliche
        };
        await _dbContext.ProfundaInstanzen.AddAsync(inst);
        await _dbContext.SaveChangesAsync();
        return inst;
    }

    public Task<DTOProfundumInstanz[]> GetInstanzenAsync()
    {
        return _dbContext.ProfundaInstanzen
            .AsSingleQuery()
            .Include(p => p.Verantwortliche)
            .Include(i => i.Profundum).ThenInclude(p => p.Dependencies)
            .Include(i => i.Profundum).ThenInclude(p => p.Kategorie)
            .Include(i => i.Profundum)
            .ThenInclude(p => p.Fachbereiche)
            .Include(i => i.Slots)
            .Include(i => i.Einschreibungen).ThenInclude(e => e.BetroffenePerson)
            .OrderBy(i => i.Profundum.Bezeichnung.ToLower())
            .Select(i => new DTOProfundumInstanz(i))
            .ToArrayAsync();
    }

    public Task<DTOProfundumInstanz?> GetInstanzAsync(Guid instanzId)
    {
        return _dbContext.ProfundaInstanzen
            .AsSingleQuery()
            .Include(p => p.Verantwortliche)
            .Include(i => i.Profundum).ThenInclude(p => p.Dependencies)
            .Include(i => i.Profundum).ThenInclude(p => p.Kategorie)
            .Include(i => i.Profundum)
            .ThenInclude(p => p.Fachbereiche)
            .Include(i => i.Slots)
            .Where(i => i.Id == instanzId)
            .Select(i => new DTOProfundumInstanz(i))
            .FirstOrDefaultAsync();
    }

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
        var slotIds = slots.Select(s => s.Id).ToArray();

        instanz.Slots = slots.ToList();

        var verantwortlicheIds = verantwortliche.Select(e => e.Id).ToArray();
        instanz.Verantwortliche.RemoveAll(v => !verantwortlicheIds.Contains(v.Id));
        var instanzVerantwortlicheIds = instanz.Verantwortliche.Select(v => v.Id).ToArray();
        instanz.Verantwortliche.AddRange(verantwortliche.Where(v => !instanzVerantwortlicheIds.Contains(v.Id)));

        instanz.MaxEinschreibungen = patch.MaxEinschreibungen;
        instanz.Ort = patch.Ort;

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

        var teilnehmer = _dbContext.ProfundaEinschreibungen
            .Where(e => e.ProfundumInstanz != null && e.ProfundumInstanz.Id == p.Id)
            .Select(e => e.BetroffenePerson)
            .Distinct()
            .OrderBy(e => e.LastName)
            .ThenBy(e => e.FirstName);

        const string src = Altafraner.Typst.Templates.Profundum.Instanz;

        var inputs = new
        {
            bezeichnung = p.Profundum.Bezeichnung,
            beschreibung = "",
            voraussetzungen = p.Profundum.Dependencies.Select(d => d.Bezeichnung),
            ort = p.Ort,
            slots = p.Slots.OrderBy(e => e.Jahr).ThenBy(e => e.Quartal).ThenBy(e => e.Wochentag),
            verantwortliche = p.Verantwortliche.Select(v => new PersonInfoMinimal(v)),
            teilnehmer = teilnehmer.Select(v => new PersonInfoMinimal(v)),
        };

        return _typst.generatePdf(src, inputs);
    }
    public async Task<(byte[], string)> GetSlotPdfsZipAsync(Guid slotId)
    {
        var slot = await _dbContext.ProfundaSlots.FindAsync(slotId);
        if (slot is null)
        {
            throw new NotFoundException("no such slot");
        }

        var instanzen = _dbContext.ProfundaInstanzen
            .Include(i => i.Slots)
            .Include(i => i.Profundum)
            .ToArray()
            .Where(i => i.Slots.Contains(slot));

        using var ms = new MemoryStream();

        await using (var archive = new ZipArchive(ms, ZipArchiveMode.Create, leaveOpen: true))
        {
            foreach (var i in instanzen)
            {
                var sanitizedName =
                    new string(i.Profundum.Bezeichnung.Select(c => char.IsLetterOrDigit(c) || c == ' ' ? c : '_')
                        .ToArray()).Trim(' ', '_');
                var fname = $"{sanitizedName}.pdf";
                var entry = archive.CreateEntry(fname);
                await using var entryStream = await entry.OpenAsync();
                var pdf = await GetInstanzPdfAsync(i.Id);
                await entryStream.WriteAsync(pdf, 0, pdf.Length);
            }
        }
        return (ms.ToArray(), slot.ToString());
    }

    ///
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

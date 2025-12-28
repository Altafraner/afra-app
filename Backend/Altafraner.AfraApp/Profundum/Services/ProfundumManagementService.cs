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

    /// <summary>
    ///     Constructs the ManagementService. Usually called by the DI container.
    /// </summary>
    public ProfundumManagementService(AfraAppContext dbContext,
        ILogger<ProfundumManagementService> logger,
        IOptions<TypstConfiguration> typstConfig
        )
    {
        _dbContext = dbContext;
        _logger = logger;
        _typstConfig = typstConfig;
    }

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

    public Task<DTOProfundumEinwahlZeitraum[]> GetEinwahlZeiträumeAsync()
    {
        return _dbContext.ProfundumEinwahlZeitraeume
            .Select(e => new DTOProfundumEinwahlZeitraum(e))
            .ToArrayAsync();
    }

    public Task<DTOProfundumSlot[]> GetSlotsAsync()
    {
        return _dbContext.ProfundaSlots
            .Include(s => s.EinwahlZeitraum)
            .Select(s => new DTOProfundumSlot(s))
            .ToArrayAsync();
    }

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

        var deps = await _dbContext.Profunda.Where(p => dtoProfundum.DependencyIds.Contains(p.Id)).ToArrayAsync();

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
            .ToArrayAsync();
        profundum.Dependencies = deps;
        foreach (var d in dtoProfundum.DependencyIds)
        {
            Console.WriteLine(d);
        }

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
            .Select(e => e.BetroffenePerson);

        var src = $$"""
            #import "@preview/cmarker:0.1.8"
            #import "@preview/mitex:0.2.6": mitex

            #let bezeichnung = sys.inputs.bezeichnung
            #let beschreibung = sys.inputs.beschreibung
            #let slots = json(bytes(sys.inputs.slots))
            #let verantwortliche = json(bytes(sys.inputs.verantwortliche))
            #let teilnehmer = json(bytes(sys.inputs.teilnehmer))

            #align(right, image("logo.png", width: 50mm))

            #let accent-color = rgb("#0069B4")
            #let accent-font = "TheSerif"

            #show heading.where(level: 1): set text(size: 22pt, font: accent-font, weight: 500, fill: accent-color)
            #show heading.where(level: 2): set text(size: 17pt, font: accent-font, weight: 500, fill: accent-color)
            #show heading.where(level: 3): set text(size: 14pt, font: accent-font, weight: 500, fill: accent-color)
            #show heading.where(level: 4): set text(size: 12pt, font: accent-font, weight: 500, fill: accent-color)

            = Profundum: #bezeichnung

            #let weekdays = (
              "Montag",
              "Dienstag",
              "Mittwoch",
              "Donnerstag",
              "Freitag",
              "Samstag",
              "Sonntag",
            )

            #table(columns: 3,stroke:none,
                ..for s in slots {
                    ([#s.Jahr/#{calc.rem-euclid(s.Jahr + 1, 100)}], [Q#s.Quartal], weekdays.at(s.Wochentag+1))
                }
            )

            == Inhalt


            #cmarker.render(math:mitex,h1-level:3,
                beschreibung
            )

            == Verantwortliche

            #table(columns: 3,stroke:none,
                ..for v in verantwortliche {
                    (v.Nachname, v.Vorname, v.Email)
                }
            )

            == Teilnehmer

            #table(columns: 3,
                ..for v in teilnehmer {
                    (v.Gruppe, v.Nachname, v.Vorname,)
                }
            )
            """;


        var typst = new TypstCompilerWrapper(src, null, _typstConfig.Value.TypstResourcePath);

        typst.SetSysInputs(new Dictionary<string, string> {
                { "bezeichnung", p.Profundum.Bezeichnung },
                { "beschreibung", p.Profundum.Beschreibung },
                { "slots", JsonSerializer.Serialize(p.Slots) },
                { "verantwortliche", JsonSerializer.Serialize(p.Profundum.Verantwortliche.Select(v=>new PersonInfoMinimal(v))) },
                { "teilnehmer", JsonSerializer.Serialize(teilnehmer.Select(v=>new PersonInfoMinimal(v))) },
        });

        var res = typst.CompilePdf();
        return res;
    }

}

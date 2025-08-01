using Afra_App.Profundum.Domain.DTO;
using Afra_App.Profundum.Domain.Models;
using Afra_App.User.Services;
using Microsoft.EntityFrameworkCore;
using Person = Afra_App.User.Domain.Models.Person;

namespace Afra_App.Profundum.Services;

/// <summary>
///     A service for handling enrollments.
/// </summary>
public class EnrollmentService
{
    private readonly AfraAppContext _dbContext;
    private readonly ILogger _logger;
    private readonly UserService _userService;

    /// <summary>
    ///     Constructs the EnrollmentService. Usually called by the DI container.
    /// </summary>
    public EnrollmentService(AfraAppContext dbContext,
        ILogger<EnrollmentService> logger, UserService userService)
    {
        _dbContext = dbContext;
        _logger = logger;
        _userService = userService;
    }

    ///
    public ICollection<BlockKatalog> GetKatalog()
    {
        var bk = new List<BlockKatalog>() { };
        var slots = _dbContext.ProfundaSlots.Where(s => s.EinwahlMöglich).OrderBy(s => (s.Jahr * 4 + (int)s.Quartal) * 7 + s.Wochentag).ToArray();

        foreach (var s in slots)
        {
            var profundumInstanzen = _dbContext.ProfundaInstanzen
                .Where(p => s.Id == p.Slots
                        .OrderBy(s => (s.Jahr * 4 + (int)s.Quartal) * 7 + s.Wochentag)
                        .First().Id
                );
            bk.Add(new BlockKatalog
            {
                label = s.ToString(),
                id = s.ToString(),
                options = profundumInstanzen.Select(p => new BlockOption
                {
                    label = p.Profundum.Bezeichnung,
                    value = p.Id,
                    alsoIncludes = p.Slots.Where(x => x.Id != s.Id).Select(s => s.ToString()).ToArray(),
                }).ToArray(),
            });
        }

        return bk;
    }

    ///
    public async Task<IResult> RegisterBelegWunschAsync(Person student, Dictionary<String, Guid[]> wuensche)
    {
        var slotsMöglich = _dbContext.ProfundaSlots.Where(s => s.EinwahlMöglich).OrderBy(s => (s.Jahr * 4 + (int)s.Quartal) * 7 + s.Wochentag).ToArray().AsReadOnly();

        var konflikte = _dbContext.ProfundaBelegWuensche
            .Include(bw => bw.ProfundumInstanz)
            .Where(p => p.ProfundumInstanz.Slots.Any(s => s.EinwahlMöglich))
            .Where(p => p.BetroffenePerson.Id == student.Id);
        if (konflikte.Any())
        {
            return Results.Conflict("Bereits abgegeben");
        }

        var ProfilProfundumEnthalten = false;

        foreach (var (str, l) in wuensche)
        {
            var s = slotsMöglich.Where(sm => sm.ToString() == str).FirstOrDefault();
            if (s is null)
            {
                return Results.BadRequest("Kein solcher Slot");
            }
            for (int i = 0; i < l.Length; ++i)
            {
                if (!BelegWunschStufe.IsDefined(typeof(BelegWunschStufe), i + 1))
                {
                    return Results.BadRequest("Belegwunschstufe nicht definiert");
                }

                var stufe = (BelegWunschStufe)(i + 1);
                var profundumInstanz = _dbContext.ProfundaInstanzen.Include(p => p.Profundum).Include(p => p.Slots).Where(p => p.Id == l[i]).First();

                if (profundumInstanz is null)
                {
                    return Results.BadRequest("ProfundumInstanz nicht gefunden");
                }

                if (profundumInstanz.Slots.Except(slotsMöglich).Any())
                {
                    return Results.BadRequest("ProfundumInstanz hat nicht einwählbare slots");

                }

                if (profundumInstanz.Slots.OrderBy(s => (s.Jahr * 4 + (int)s.Quartal) * 7 + s.Wochentag).First().Id == s.Id)
                {
                    _logger.LogInformation("Added {} {}", profundumInstanz.Profundum.Bezeichnung, stufe.ToString());
                    var belegWunsch = new BelegWunsch() { ProfundumInstanz = profundumInstanz, Stufe = stufe, BetroffenePerson = student };
                    _dbContext.ProfundaBelegWuensche.Add(belegWunsch);

                    if (profundumInstanz.Profundum.ProfilProfundum)
                    {
                        ProfilProfundumEnthalten = true;
                    }
                }
                else if (!profundumInstanz.Slots.Where(sl => sl.Id == s.Id).Any())
                {
                    return Results.BadRequest("Profunduminstanz enthält nicht diesen Slot");
                }
            }
        }

        ProfundumQuartal[] QuartaleWintersemester = [ProfundumQuartal.Q1, ProfundumQuartal.Q2];
        if (QuartaleWintersemester.Contains(slotsMöglich.First().Quartal)
            ? _userService.GetKlassenstufe(student) == 10
            : _userService.GetKlassenstufe(student) == 9)
        {
            if (!ProfilProfundumEnthalten)
            {
                return Results.BadRequest("Kein Profilprofundum in Einwahl enthalten");
            }
        }

        await _dbContext.SaveChangesAsync();

        return Results.Ok("Einwahl gespeichert");
    }
}

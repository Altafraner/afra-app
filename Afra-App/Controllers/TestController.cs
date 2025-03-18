using Afra_App.Data;
using Afra_App.Data.Otium;
using Bogus;
using Afra_App.Authentication;
using Afra_App.Data.People;
using Afra_App.Data.Schuljahr;
using Afra_App.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Person = Afra_App.Data.People.Person;
using Microsoft.EntityFrameworkCore;
using Termin = Afra_App.Data.Otium.Termin;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
// Sorry, this is a test controller, not worth the effort.

namespace Afra_App.Controllers;

[ApiController]
[Route("/api/[controller]")]
public class TestController(AfraAppContext dbContext, UserService userService) : ControllerBase
{
    [HttpGet("reset")]
    public ActionResult ResetDb()
    {
        dbContext.Database.EnsureDeleted();
        dbContext.Database.Migrate();

        return Ok("Die Datenbank wurde erfolgreich zurückgesetzt.");
    }

    [HttpGet("seed")]
    public async Task<ActionResult> SeedDb()
    {
        var akademisches = new Kategorie
        {
            Bezeichnung = "Akademisches", Icon = "pi pi-graduation-cap", CssColor = "var(--p-blue-500)", Required = true
        };
        var otiumsKategorien = new List<Kategorie>
        {
            akademisches,
            new() { Bezeichnung = "Bewegung", Icon = "pi pi-heart", CssColor = "var(--p-teal-500)" },
            new() { Bezeichnung = "Muße", Icon = "pi pi-headphones", CssColor = "var(--p-orange-500)" },
            new() { Bezeichnung = "Besinnung", Icon = "pi pi-hourglass", CssColor = "var(--p-yellow-500)" },
            new() { Bezeichnung = "Beratung", Icon = "pi pi-user", CssColor = "var(--p-purple-500)" },
            new() { Bezeichnung = "Teamräume", Icon = "pi pi-home", CssColor = "var(--p-red-500)" },
            new() { Parent = akademisches, Bezeichnung = "Studienzeit" },
            new() { Parent = akademisches, Bezeichnung = "Schüler:innen unterrichten Schüler:innen" },
            new() { Parent = akademisches, Bezeichnung = "Wettbewerbe" },
            new() { Parent = akademisches, Bezeichnung = "Sonstige" }
        };
        List<bool[]> possibleOtiaBlocks = [[true, true], [true, true], [true, false], [false, true]];
        List<string> rooms =
        [
            "102", "103", "104", "105", "106", "108", "109", "110", "202", "203", "204", "205", "206",
            "207", "208", "209", "211", "212", "213", "214", "215", "216", "217", "301", "307", "308"
        ];

        List<(string, Kategorie)> possibleOtia =
        [
            ("Schreibwerkstatt", otiumsKategorien[6]),
            ("Studienzeit Mathematik", otiumsKategorien[6]),
            ("Studienzeit Physik", otiumsKategorien[6]),
            ("Studienzeit Griechisch", otiumsKategorien[6]),
            ("Studienzeit Französisch", otiumsKategorien[6]),
            ("Schülerlabor", otiumsKategorien[9]),
            ("Bibliothek", otiumsKategorien[9]),
            ("Informatik / Pyhsik / Mathe mit Richard", otiumsKategorien[7]),
            ("Buchclub", otiumsKategorien[9]),
            ("F1inSchools", otiumsKategorien[8]),
            ("Kraftraum", otiumsKategorien[1]),
            ("Yoga", otiumsKategorien[1]),
            ("Übungsraum Musik", otiumsKategorien[2]),
            ("Offenes Atelier", otiumsKategorien[2]),
            ("Ruheraum", otiumsKategorien[3]),
            ("Handarbeit", otiumsKategorien[3]),
            ("Lernen Lernen", otiumsKategorien[5])
        ];


        var personFaker = new Faker<Person>("de")
            .RuleFor(p => p.Nachname, f => f.Person.LastName)
            .RuleFor(p => p.Vorname, f => f.Person.FirstName)
            .RuleFor(p => p.Email, (_, p)
                => $"{p.Vorname.ToLower()}.{p.Nachname.ToLower()}@example.org"
            )
            .RuleFor(p => p.Rolle, Rolle.Tutor);

        var mentoren = personFaker.Generate(60);
        dbContext.Personen.AddRange(mentoren);

        var studentsFaker = personFaker
            .RuleFor(p => p.Mentor, f => f.PickRandom(mentoren))
            .RuleFor(p => p.Rolle, Rolle.Student);

        var students = studentsFaker.Generate(250);
        dbContext.Personen.AddRange(students);

        var today = DateTime.Today;
        var nextMonday = today.AddDays((int)DayOfWeek.Monday - (int)today.DayOfWeek);
        var nextFriday = today.AddDays((int)DayOfWeek.Friday - (int)today.DayOfWeek);


        var schultagGenerator = new Faker<Schultag>()
            .RuleFor(s => s.OtiumsBlock, f => f.PickRandom(possibleOtiaBlocks))
            .RuleFor(s => s.Datum,
                f => DateOnly.FromDateTime(f.PickRandomParam(nextMonday, nextFriday)).AddDays(f.IndexFaker * 7))
            .RuleFor(s => s.Wochentyp, f => f.PickRandom<Wochentyp>());
        var schultage = schultagGenerator.Generate(20);
        dbContext.Schultage.AddRange(schultage);


        dbContext.OtiaKategorien.AddRange(otiumsKategorien);
        await dbContext.SaveChangesAsync();

        var otiumGenerator = new Faker<Otium>("de")
            .RuleFor(o => o.Bezeichnung, f => possibleOtia[f.IndexFaker].Item1)
            .RuleFor(o => o.Beschreibung, f => f.Commerce.ProductDescription())
            .RuleFor(o => o.Verantwortliche, f => f.Random.Bool(0.7f) ? [f.PickRandom(mentoren)] : [])
            .RuleFor(o => o.Kategorie, f => possibleOtia[f.IndexFaker].Item2);

        var otia = otiumGenerator.Generate(possibleOtia.Count);
        dbContext.AddRange(otia);

        await dbContext.SaveChangesAsync();

        var otiumTerminGenerator = new Faker<Termin>("de")
            .RuleFor(t => t.Otium, f => f.PickRandom(otia))
            .RuleFor(t => t.Tutor, (_, t) => t.Otium.Verantwortliche.FirstOrDefault())
            .RuleFor(t => t.IstAbgesagt, f => f.Random.Bool(0.1f))
            .RuleFor(t => t.Ort, f => f.PickRandom(rooms))
            .RuleFor(t => t.Block, f => f.Random.Byte(0, 1))
            .RuleFor(t => t.Schultag, f => f.PickRandom(schultage))
            .RuleFor(t => t.MaxEinschreibungen, f => f.Random.Bool() ? null : f.Random.Int(2, 20));

        dbContext.OtiaTermine.AddRange(
            otiumTerminGenerator.Generate(300).ToList());

        await dbContext.SaveChangesAsync();

        return Ok("Die Datenbank wurde erfolgreich befüllt.");
    }

    [Route("authenticate/{id:guid}")]
    public async Task<ActionResult> AuthenticateAs(Guid id)
    {
        try
        {
            await userService.SignInAsync(id, HttpContext);
        }
        catch (InvalidOperationException e)
        {
            return BadRequest(e.Message);
        }

        return Ok("Logged in");
    }

    [Route("authenticate/logout")]
    public async Task<ActionResult> AuthenticateAs()
    {
        await HttpContext.SignOutAsync();
        return Ok("Logged out");
    }

    [Route("authenticate")]
    [Authorize]
    public ActionResult<Data.DTO.Person> PrintAuthentication()
    {
        return new Data.DTO.Person(HttpContext.GetPerson(dbContext));
    }
}
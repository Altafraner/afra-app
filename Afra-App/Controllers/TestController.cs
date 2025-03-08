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
        var personFaker = new Faker<Person>(locale: "de")
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
        var nextMonday = today.AddDays((int) DayOfWeek.Monday - (int) today.DayOfWeek);
        var nextFriday = today.AddDays((int) DayOfWeek.Friday - (int) today.DayOfWeek);

        List<bool[]> possibleOtiaBlocks = [[true, true], [true, false], [false, true]];
        
        var schultagGenerator = new Faker<Schultag>()
            .RuleFor(s => s.OtiumsBlock, f => f.PickRandom(possibleOtiaBlocks))
            .RuleFor(s => s.Datum,
                f => DateOnly.FromDateTime(f.PickRandomParam(nextMonday, nextFriday)).AddDays(f.IndexFaker * 7))
            .RuleFor(s => s.Wochentyp, f => f.PickRandom<Wochentyp>());
        var schultage = schultagGenerator.Generate(20);
        dbContext.Schultage.AddRange(schultage);

        var akademisches = new Kategorie
            { Bezeichnung = "Akademisches", Icon = "pi pi-graduation-cap", CssColor = "var(--p-blue-500)", Required = true};
        var otiumsKategorien = new List<Kategorie>
        {
            akademisches,
            new() { Bezeichnung = "Bewegung", Icon = "pi pi-heart", CssColor = "var(--p-teal-500)"},
            new() { Bezeichnung = "Musik", Icon = "pi pi-headphones", CssColor = "var(--p-orange-500)"},
            new() { Bezeichnung = "Besinnung", Icon = "pi pi-hourglass", CssColor = "var(--p-yellow-500)"},
            new() { Bezeichnung = "Beratung", Icon = "pi pi-user", CssColor = "var(--p-purple-500)"},
            new() { Bezeichnung = "Teamräume"},
            new() { Parent = akademisches, Bezeichnung = "Studienzeit"},
            new() { Parent = akademisches, Bezeichnung = "Schüler:innen unterrichten Schüler:innen"},
            new() { Parent = akademisches, Bezeichnung = "Wettbewerbe"},
            new() { Parent = akademisches, Bezeichnung = "Sonstige"}
        };
        dbContext.OtiaKategorien.AddRange(otiumsKategorien);
        await dbContext.SaveChangesAsync();

        var otiumGenerator = new Faker<Otium>(locale: "de")
            .RuleFor(o => o.Bezeichnung, f => f.Hacker.Noun())
            .RuleFor(o => o.Beschreibung, f => f.Rant.Review())
            .RuleFor(o => o.Verantwortliche, f => [f.PickRandom(mentoren)])
            .RuleFor(o => o.Kategorie, f => f.PickRandom(otiumsKategorien));

        var otia = otiumGenerator.Generate(30);
        dbContext.AddRange(otia);

        await dbContext.SaveChangesAsync();

        var otiumTerminGenerator = new Faker<Termin>(locale: "de")
            .RuleFor(t => t.Otium, f => f.PickRandom(otia))
            .RuleFor(t => t.Tutor, (_, t) => t.Otium.Verantwortliche.FirstOrDefault())
            .RuleFor(t => t.IstAbgesagt, f => f.Random.Bool(0.1f))
            .RuleFor(t => t.Ort, f => f.Hacker.Abbreviation())
            .RuleFor(t => t.Block, f => f.Random.Byte(0,1))
            .RuleFor(t => t.Schultag, f => f.PickRandom(schultage));

        dbContext.OtiaTermine.AddRange(
            otiumTerminGenerator.Generate(200).ToList());

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
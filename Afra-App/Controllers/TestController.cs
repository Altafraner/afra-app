using Afra_App.Data;
using Afra_App.Data.Otium;
using Afra_App.Data.People;
using Bogus;
using Microsoft.AspNetCore.Mvc;
using Person = Afra_App.Data.People.Person;

namespace Afra_App.Controllers;

[ApiController]
[Route("/api/[controller]")]
public class TestController(AfraAppContext dbContext) : ControllerBase
{
    // GET
    [HttpGet("reset")]
    public ActionResult ResetDb()
    {
        dbContext.Database.EnsureDeleted();
        dbContext.Database.EnsureCreated();

        return Ok("Die Datenbank wurde erfolgreich zurückgesetzt.");
    }

    [HttpGet("seed")]
    public ActionResult SeedDb()
    {
        var personFaker = new Faker<Person>(locale: "de")
            .RuleFor(p => p.Nachname, f => f.Person.LastName)
            .RuleFor(p => p.Vorname, f => f.Person.FirstName)
            .RuleFor(p => p.Email, (_, p)
                => $"{p.Vorname.ToLower()}.{p.Nachname.ToLower()}@example.org"
            );

        var mentoren = personFaker.Generate(60);
        dbContext.Personen.AddRange(mentoren);

        var studentsFaker = personFaker
            .RuleFor(p => p.Mentor, f => f.PickRandom(mentoren));

        var students = studentsFaker.Generate(250);
        dbContext.Personen.AddRange(students);

        var otiumKategorieGenerator = new Faker<Kategorie>(locale: "de")
            .RuleFor(k => k.Bezeichnung, f => f.Hacker.Adjective() + f.UniqueIndex);
        var otiaKategorien = otiumKategorieGenerator.Generate(10);
        dbContext.OtiaKategorien.AddRange(otiaKategorien);
        dbContext.SaveChanges();

        var otiumGenerator = new Faker<Otium>(locale: "de")
            .RuleFor(o => o.Bezeichnung, f => f.Hacker.Noun())
            .RuleFor(o => o.Beschreibung, f => f.Rant.Review())
            .RuleFor(o => o.Verantwortliche, f => [f.PickRandom(mentoren)])
            .RuleFor(o => o.Kategorie, f => f.PickRandom(otiaKategorien));

        var otia = otiumGenerator.Generate(30);
        dbContext.AddRange(otia);

        dbContext.SaveChanges();

        DayOfWeek[] allowedWeekdays = [DayOfWeek.Monday, DayOfWeek.Friday];
        var otiumTerminGenerator = new Faker<Termin>(locale: "de")
            .RuleFor(t => t.Otium, f => f.PickRandom(otia))
            .RuleFor(t => t.Tutor, (_, t) => t.Otium.Verantwortliche.FirstOrDefault())
            .RuleFor(t => t.IstAbgesagt, f => f.Random.Bool(0.1f))
            .RuleFor(t => t.Ort, f => f.Hacker.Abbreviation())
            .RuleFor(t => t.Block, f => f.Random.Byte(0,1))
            .RuleFor(t => t.Datum, f =>
            {
                var date = f.Date.SoonDateOnly(7);
                var dayOfWeek = f.PickRandom(allowedWeekdays);
                return date.AddDays(dayOfWeek - date.DayOfWeek);
            });

        dbContext.OtiaTermine.AddRange(
            otiumTerminGenerator.Generate(50));

        dbContext.SaveChanges();
        
        return Ok("Die Datenbank wurde erfolgreich befüllt.");
    }
}
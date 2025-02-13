using Afra_App.Data;
using Afra_App.Data.People;
using Microsoft.AspNetCore.Mvc;

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
        var schueler = dbContext.Personen;
        var schueler1 = new Person
        {
            Vorname = "Schueler1",
            Nachname = "ZTest",
            Email = "test1@test.te"
        };
        var lehrer1 = new Person
        {
            Vorname = "Lehrer2",
            Nachname = "CCTest",
            Email = "test2@test.te"
        };
        var lehrer2 = new Person
        {
            Vorname = "Lehrer1",
            Nachname = "CTest",
            Email = "test3@test.te"
        };
        var schueler2 = new Person
        {
            Vorname = "Schueler2",
            Nachname = "BTest",
            Email = "test4@test.te"
        };
        var schueler3 = new Person
        {
            Vorname = "Schueler3",
            Nachname = "ATest",
            Email = "test5@test.te"
        };
        schueler.AddRange(
            schueler1,
            lehrer1,
            lehrer2,
            schueler2,
            schueler3
        );
        lehrer1.Mentees.Add(schueler1);
        lehrer2.Mentees.Add(schueler2);
        lehrer2.Mentees.Add(schueler3);
        
        dbContext.SaveChanges();

        return Ok("Die Datenbank wurde erfolgreich befüllt.");
    }
}
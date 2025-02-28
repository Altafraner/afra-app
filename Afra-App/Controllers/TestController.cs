using Afra_App.Authentication;
using Afra_App.Models;
using Afra_App.Models.Json;
using Afra_App.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
    public ActionResult SeedDb()
    {
        var klassen = dbContext.Classes;

        var klasse1 = new Class
            {
                Level = 7,
                Appendix = "a"
            };
        klassen.AddRange(
            klasse1, new Class
            {
                Level = 8,
                Appendix = "a"
            }, new Class
            {
                Level = 9,
                Appendix = "a"
            }, new Class
            {
                Level = 9,
                Appendix = "b"
            }, new Class
            {
                Level = 9,
                Appendix = "c"
            }, new Class
            {
                Level = 10,
                Appendix = "a"
            }, new Class
            {
                Level = 10,
                Appendix = "b"
            }, new Class
            {
                Level = 10,
                Appendix = "c"
            });

        var schueler = dbContext.People;
        var schueler1 = new Person
        {
            FirstName = "Schueler1",
            LastName = "ZTest",
            Email = "test1@test.te"
        };
        var lehrer1 = new Person
        {
            FirstName = "Lehrer2",
            LastName = "CCTest",
            Email = "test2@test.te"
        };
        var lehrer2 = new Person
        {
            FirstName = "Lehrer1",
            LastName = "CTest",
            Email = "test3@test.te"
        };
        var schueler2 = new Person
        {
            FirstName = "Schueler2",
            LastName = "BTest",
            Email = "test4@test.te"
        };
        var schueler3 = new Person
        {
            FirstName = "Schueler3",
            LastName = "ATest",
            Email = "test5@test.te"
        };
        schueler.AddRange(
            schueler1,
            lehrer1,
            lehrer2,
            schueler2,
            schueler3
        );
        
        klasse1.Tutor = lehrer1;
        klasse1.Students.Add(schueler1);
        klasse1.Students.Add(schueler2);
        klasse1.Students.Add(schueler3);
        lehrer1.Mentees.Add(schueler1);
        lehrer2.Mentees.Add(schueler2);
        lehrer2.Mentees.Add(schueler3);
        
        dbContext.SaveChanges();

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
    public ActionResult<PersonJsonInfo> PrintAuthentication()
    {
        return new PersonJsonInfo(HttpContext.GetPerson(dbContext));
    }
}
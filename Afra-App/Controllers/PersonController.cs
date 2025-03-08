using System.Text.Json.Serialization;
using Afra_App.Data;
using Afra_App.Data.DTO;
using Afra_App.Data.People;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Person = Afra_App.Data.DTO.Person;

namespace Afra_App.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PersonController : ControllerBase
{
    private AfraAppContext _dbContext;

    public PersonController(AfraAppContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public IEnumerable<Data.People.Person> GetPeople()
    {
        return _dbContext.Personen;
    }

    public record struct PeopleRequestFilter
    {
        [JsonPropertyName("vorname")] public string? Vorname { get; init; }
        [JsonPropertyName("nachname")] public string? Nachname { get; init; }
    }

    [HttpPost("page")]
    public ActionResult<object> GetPeoplePaginated(
        [FromQuery] int page,
        [FromQuery] int pageSize,
        [FromBody] PeopleRequestFilter filter,
        [FromQuery] string sort = "nachname",
        [FromQuery] string sortDirection = "asc")
    {
        IQueryable<Data.People.Person> query = _dbContext.Personen;

        if (!string.IsNullOrWhiteSpace(filter.Vorname))
            query = query.Where(p => EF.Functions.Like(p.Vorname, $"{filter.Vorname.ToLower()}%"));
        
        if (!string.IsNullOrWhiteSpace(filter.Nachname))
            query = query.Where(p => EF.Functions.Like(p.Nachname, $"{filter.Nachname.ToLower()}%"));

        query = (sort, sortDirection) switch
        {
            ("nachname", "asc") => query.OrderBy(e => e.Nachname),
            ("nachname", "desc") => query.OrderByDescending(e => e.Nachname),
            ("vorname", "asc") => query.OrderBy(e => e.Vorname),
            ("vorname", "desc") => query.OrderByDescending(e => e.Vorname),
            _ => query
        };

        return new Pagination<Person>(
            query.Count(),
            query.Skip((page - 1) * pageSize).Take(pageSize).Select(person => new Person(person))
        );
    }

    [HttpGet("{id:guid}")]
    public ActionResult<Person> GetPerson(Guid id)
    {
        var person = _dbContext.Personen.Find(id);
        if (person == null)
            return NotFound();

        return new Person(person);
    }

    [HttpGet("{id:guid}/mentor")]
    public ActionResult<Data.People.Person> GetMentor(Guid id)
    {
        var person = _dbContext.Personen
            .Find(id);

        if (person == null)
            return NotFound();

        _dbContext.Personen.Entry(person)
            .Reference(p => p.Mentor)
            .Load();

        var mentor = person.Mentor;
        if (mentor == null)
            return NotFound();
        return mentor;
    }

    [HttpGet("{id:guid}/mentees")]
    public ActionResult<IEnumerable<Data.People.Person>> GetMentees(Guid id)
    {
        var mentor = _dbContext.Personen.Find(id);
        if (mentor == null)
            return NotFound();

        _dbContext.Personen.Entry(mentor)
            .Collection(p => p.Mentees)
            .Load();
        return mentor.Mentees.ToList();
    }
}
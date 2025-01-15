using System.Text.Json.Serialization;
using Afra_App.Models;
using Afra_App.Models.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
    public IEnumerable<Person> GetPeople()
    {
        return _dbContext.People.Include(p => p.Class);
    }

    public record struct PeopleRequestFilter
    {
        [JsonPropertyName("firstName")] public string? FirstName { get; init; }
        [JsonPropertyName("lastName")] public string? LastName { get; init; }
        [JsonPropertyName("classes")] public IEnumerable<Guid?> Classes { get; init; }
    }

    [HttpPost("page")]
    public ActionResult<object> GetPeoplePaginated(
        [FromQuery] int page,
        [FromQuery] int pageSize,
        [FromBody] PeopleRequestFilter filter,
        [FromQuery] string sort = "last_name",
        [FromQuery] string sortDirection = "asc")
    {
        IQueryable<Person> query = _dbContext.People
            .Include(e => e.Class);

        if (!string.IsNullOrWhiteSpace(filter.FirstName))
            query = query.Where(p => EF.Functions.Like(p.FirstName, $"{filter.FirstName.ToLower()}%"));
        
        if (!string.IsNullOrWhiteSpace(filter.LastName))
            query = query.Where(p => EF.Functions.Like(p.LastName, $"{filter.LastName.ToLower()}%"));
        
        if (filter.Classes.Any())
            query = !filter.Classes.Contains(null)
                ? query.Where(p => p.Class != null && filter.Classes.Contains(p.Class.Id))
                : query.Where(p => p.Class == null || filter.Classes.Contains(p.Class.Id));

        query = (sort, sortDirection) switch
        {
            ("lastName", "asc") => query.OrderBy(e => e.LastName),
            ("lastName", "desc") => query.OrderByDescending(e => e.LastName),
            ("firstName", "asc") => query.OrderBy(e => e.FirstName),
            ("firstName", "desc") => query.OrderByDescending(e => e.FirstName),
            ("class.name", "asc") => query
                .OrderBy(e => e.Class == null ? 0 : e.Class.Level)
                .ThenBy(e => e.Class == null ? "" : e.Class.Appendix),
            ("class.name", "desc") => query
                .OrderByDescending(e => e.Class == null ? 0 : e.Class.Level)
                .ThenByDescending(e => e.Class == null ? "" : e.Class.Appendix),
            _ => query
        };

        return new Pagination<PersonJsonInfo>(
            query.Count(),
            query.Skip((page - 1) * pageSize).Take(pageSize).Select(person => new PersonJsonInfo(person))
        );
    }

    [HttpGet("{id:guid}")]
    public ActionResult<PersonJsonInfo> GetPerson(Guid id)
    {
        var person = _dbContext.People.Find(id);
        if (person == null)
        {
            return NotFound();
        }

        return new PersonJsonInfo(person);
    }

    public record struct PersonCreationData()
    {
        [JsonPropertyName("firstName")] public string FirstName { get; set; }

        [JsonPropertyName("lastName")] public string LastName { get; set; }

        [JsonPropertyName("email")] public string EMail { get; set; }

        [JsonPropertyName("classId")] public Guid? ClassId { get; set; }
    }

    [HttpPost]
    public ActionResult<Person> PostPerson(PersonCreationData personData)
    {
        var person = new Person
        {
            Email = personData.EMail,
            FirstName = personData.FirstName,
            LastName = personData.LastName,
        };

        if (personData.ClassId != null)
        {
            var classElement = _dbContext.Classes.Find(personData.ClassId);
            if (classElement is null)
                return NotFound("Die angeforderte Klasse konnte nicht gefunden werden!");
            person.Class = classElement;
        }

        _dbContext.People.Add(person);
        _dbContext.SaveChanges();
        return CreatedAtAction(nameof(GetPerson), new { id = person.Id }, person);
    }

    [HttpDelete("{id:guid}")]
    public ActionResult<Person> DeletePerson(Guid id)
    {
        var person = _dbContext.People.Find(id);
        if (person == null)
        {
            return NotFound();
        }

        _dbContext.People.Remove(person);
        _dbContext.SaveChanges();
        return person;
    }

    [HttpGet("{id:guid}/mentor")]
    public ActionResult<Person> GetMentor(Guid id)
    {
        var person = _dbContext.People
            .Find(id);

        if (person == null)
            return NotFound();

        _dbContext.People.Entry(person)
            .Reference(p => p.Mentor)
            .Load();

        var mentor = person.Mentor;
        if (mentor == null)
            return NotFound();
        return mentor;
    }

    [HttpGet("{id:guid}/mentees")]
    public ActionResult<IEnumerable<Person>> GetMentees(Guid id)
    {
        var mentor = _dbContext.People.Find(id);
        if (mentor == null)
        {
            return NotFound();
        }

        _dbContext.People.Entry(mentor)
            .Collection(p => p.Mentees)
            .Load();
        return mentor.Mentees.ToList();
    }

    [HttpPut("{mentorId:guid}/mentees/{menteeId:guid}")]
    public ActionResult<Person> PutMentor(Guid menteeId, Guid mentorId)
    {
        var person = _dbContext.People.Find(menteeId);
        if (person == null)
        {
            return NotFound();
        }

        var mentor = _dbContext.People.Find(mentorId);
        if (mentor == null)
        {
            return NotFound();
        }

        person.Mentor = mentor;
        _dbContext.SaveChanges();
        return person;
    }
}

using System.Text.Json.Serialization;
using Afra_App.Data;
using Afra_App.Data.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Afra_App.Controllers;

/// <summary>
/// A controller for managing people. Currently, not really in use.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class PersonController : ControllerBase
{
    private readonly AfraAppContext _dbContext;

    /// <inheritdoc />
    public PersonController(AfraAppContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Gets all people in the database.
    /// </summary>
    /// <returns>A list of all people.</returns>
    [HttpGet]
    public IEnumerable<PersonInfoMinimal> GetPeople()
    {
        return _dbContext.Personen.Select(p => new PersonInfoMinimal(p));
    }

    /// <summary>
    /// A filter for the <see cref="GetPeoplePaginated"/> method.
    /// </summary>
    public record struct PeopleRequestFilter
    {
        /// <summary>
        /// The first name of the person
        /// </summary>
        [JsonPropertyName("vorname")] public string? Vorname { get; init; }
        /// <summary>
        /// The last name of the person
        /// </summary>
        [JsonPropertyName("nachname")] public string? Nachname { get; init; }
    }

    /// <summary>
    /// Gets a paginated list of people.
    /// </summary>
    /// <param name="page">The number of the current page</param>
    /// <param name="pageSize">The number of people per page</param>
    /// <param name="filter">A filter to apply</param>
    /// <param name="sort">The colum to sort by</param>
    /// <param name="sortDirection">asc, if sorting ascending; Otherwise desc.</param>
    /// <returns></returns>
    [HttpPost("page")]
    public ActionResult<Pagination<PersonInfoMinimal>> GetPeoplePaginated(
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

        return new Pagination<PersonInfoMinimal>(
            query.Count(),
            query.Skip((page - 1) * pageSize).Take(pageSize).Select(person => new PersonInfoMinimal(person))
        );
    }

    /// <summary>
    /// Finds the person with the specified id.
    /// </summary>
    [HttpGet("{id:guid}")]
    public ActionResult<PersonInfoMinimal> GetPerson(Guid id)
    {
        var person = _dbContext.Personen.Find(id);
        if (person == null)
            return NotFound();

        return new PersonInfoMinimal(person);
    }

    /// <summary>
    /// Finds the mentor of the person with the specified id.
    /// </summary>
    [HttpGet("{id:guid}/mentor")]
    public async Task<ActionResult<PersonInfoMinimal>> GetMentor(Guid id)
    {
        var person = await _dbContext.Personen
            .Include(p => p.Mentor)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (person == null)
            return NotFound();

        var mentor = person.Mentor;
        if (mentor == null)
            return NotFound();
        return new PersonInfoMinimal(mentor);
    }

    /// <summary>
    /// Get a list of all mentees of the person with the specified id.
    /// </summary>
    [HttpGet("{id:guid}/mentees")]
    public async Task<ActionResult<IEnumerable<PersonInfoMinimal>>> GetMentees(Guid id)
    {
        var mentor = await _dbContext.Personen
            .Include(p => p.Mentees)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (mentor == null)
            return NotFound();

        return Ok(mentor.Mentees
            .Select(p => new PersonInfoMinimal(p)));
    }
}
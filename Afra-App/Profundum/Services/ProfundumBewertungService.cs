
using Afra_App;
using Afra_App.User.Domain.DTO;
using Microsoft.EntityFrameworkCore;
using DomainModels = Afra_App.User.Domain.Models.Person;
using DomainDTOs = Afra_App.User.Domain.DTO.Person;


/// <summary>
/// An exception that is thrown when there is an error with Profundum Bewertungen.
/// </summary>
public class ProfundumsBewertungException : Exception
{
    /// <summary>
    /// An exception that is thrown when there is an error with Profundum Bewertungen.
    /// </summary>
    public ProfundumsBewertungException(string message) : base(message)
    {
    }
}

/// <summary>
/// A service for managing Profundum Bewertungen.
/// </summary>
public class ProfundumsBewertungService
{
    private readonly AfraAppContext _dbContext;

    /// <summary>
    /// Gets all persons in the database.
    /// </summary>
    public async Task<List<DomainModels>> GetAllPersonsAsync()
    {
        return await _dbContext.Personen.ToListAsync();
    }
    private readonly ILogger<ProfundumsBewertungService> _logger;

    /// <summary>
    /// Creates a new instance of the ProfundumBewertungService.
    /// </summary>
    public ProfundumsBewertungService(AfraAppContext dbContext, ILogger<ProfundumsBewertungService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }
    /// <summary>
    /// Gets all Bewertungen for a user.
    /// </summary>
    public async Task<List<ProfundumBewertung>?> GetBewertungenAsync(DomainModels user)
    {
        var person = await _dbContext.Personen.FindAsync(user.Id);
        if (person == null)
        {
            _logger.LogWarning("User {UserId} has no associated person", user.Id);
            return null;
        }

        var bewertungen = await _dbContext.ProfundumBewertungen
            .Where(b => b.BetroffenePerson.Id == person.Id)
            .ToListAsync();

        return bewertungen;
    }

    /// <summary>
    /// Adds multiple Bewertungen for a user.
    /// </summary>
    /// <exception cref="ProfundumsBewertungException"></exception>
    public async Task AddBewertungenAsync(DomainModels user, List<ProfundumBewertung> bewertungen)

    {
        var person = await _dbContext.Personen.FindAsync(user.Id);
        if (person == null)
        {
            throw new ProfundumsBewertungException("User has no associated person");
        }

        foreach (var bewertung in bewertungen)
        {
            if (bewertung.Grad < 1 || bewertung.Grad > 5)
            {
                throw new ProfundumsBewertungException($"Invalid rating value: {bewertung.Grad}. Must be between 1 and 5.");
            }

            bewertung.BetroffenePerson = person;
            _dbContext.ProfundumBewertungen.Add(bewertung);
        }

        await _dbContext.SaveChangesAsync();
    }
}

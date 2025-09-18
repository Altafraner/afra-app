
using Afra_App;
using Afra_App.User.Domain.DTO;
using Microsoft.EntityFrameworkCore;
using DomainModels = Afra_App.User.Domain.Models.Person;
using DomainDTOs = Afra_App.User.Domain.DTO.Person;


/// <summary>
/// An exception that is thrown when there is an error with Profundum Bewertungen.
/// </summary>
public class ProfundumBewertungException : Exception
{
    /// <summary>
    /// An exception that is thrown when there is an error with Profundum Bewertungen.
    /// </summary>
    public ProfundumBewertungException(string message) : base(message)
    {
    }
}

/// <summary>
/// A service for managing Profundum Bewertungen.
/// </summary>
public class ProfundumBewertungService
{
    private readonly AfraAppContext _dbContext;
    
    /// <summary>
    /// A service for managing Profundum Bewertungen.
    /// </summary>
    /// <param name="context"></param>
    public ProfundumBewertungService(AfraAppContext context)
    {
        _dbContext = context;
    }

    /// <summary>
    /// Gets all persons in the database.
    /// </summary>
    /// <returns></returns>
    public async Task<List<DomainModels>> GetAllPersonsAsync()
    {
        return await _dbContext.Personen.ToListAsync();
    }
    private readonly ILogger<ProfundumBewertungService> _logger;

    /// <summary>
    /// Creates a new instance of the ProfundumBewertungService.
    /// </summary>
    public ProfundumBewertungService(AfraAppContext dbContext, ILogger<ProfundumBewertungService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }
    /// <summary>
    /// Gets all Bewertungen for a user.
    /// </summary>
    public async Task<List<ProfundumBewertung>?> GetBewertungenAsync(Afra_App.User.Domain.Models.Person user)
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
    /// <param name="user"></param>
    /// <param name="bewertungen"></param>
    /// <returns></returns>
    /// <exception cref="ProfundumBewertungException"></exception>
    public async Task AddBewertungenAsync(DomainModels user, List<ProfundumBewertung> bewertungen)

    {
        var person = await _dbContext.Personen.FindAsync(user.Id);
        if (person == null)
        {
            throw new ProfundumBewertungException("User has no associated person");
        }

        foreach (var bewertung in bewertungen)
        {
            if (bewertung.Grad < 1 || bewertung.Grad > 5)
            {
                throw new ProfundumBewertungException($"Invalid rating value: {bewertung.Grad}. Must be between 1 and 5.");
            }
            
            bewertung.BetroffenePerson = person;
            _dbContext.ProfundumBewertungen.Add(bewertung);
        }

        await _dbContext.SaveChangesAsync();
    }
}
using Afra_App;
using Microsoft.EntityFrameworkCore;
using PersonModels = Afra_App.User.Domain.Models.Person;


/// <summary>
///     An exception that is thrown when there is an error with Profundum Bewertungen.
/// </summary>
public class ProfundumsBewertungException : Exception
{
    /// <summary>
    ///     An exception that is thrown when there is an error with Profundum Bewertungen.
    /// </summary>
    public ProfundumsBewertungException(string message) : base(message)
    {
    }
}

/// <summary>
///     A service for managing Profundum Bewertungen.
/// </summary>
public class ProfundumsBewertungService
{
    private readonly AfraAppContext _dbContext;

    /// <summary>
    ///     Gets all persons in the database.
    /// </summary>
    private readonly ILogger<ProfundumsBewertungService> _logger;

    /// <summary>
    ///     Creates a new instance of the ProfundumBewertungService.
    /// </summary>
    public ProfundumsBewertungService(AfraAppContext dbContext, ILogger<ProfundumsBewertungService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <summary>
    ///     Gets all Bewertungen for a user.
    /// </summary>
    public async Task<List<ProfundumBewertung>?> GetBewertungenAsync(PersonModels user)
    {
        if (user == null)
        {
            _logger.LogWarning("User is null, cannot load Bewertungen");
            return null;
        }


        var bewertungen = await _dbContext.ProfundumBewertungen
            .Where(b => b.BetroffenePerson.Id == user.Id)
            .ToListAsync();

        return bewertungen;
    }

    /// <summary>
    ///     Adds multiple Bewertungen for a user.
    /// </summary>
    /// <exception cref="ProfundumsBewertungException"></exception>
    public async Task AddBewertungenAsync(PersonModels user, List<ProfundumBewertung> bewertungen)
    {
        if (user == null)
        {
            throw new ProfundumsBewertungException("User has no associated person");
        }

        foreach (var bewertung in bewertungen)
        {
            if (bewertung.Grad < 1 || bewertung.Grad > 5)
            {
                throw new ArgumentOutOfRangeException($"Invalid rating value: {bewertung.Grad}. Must be between 1 and 5.");
            }

            bewertung.BetroffenePerson = user;
            _dbContext.ProfundumBewertungen.Add(bewertung);
        }

        await _dbContext.SaveChangesAsync();
    }
}

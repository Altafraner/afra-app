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
    public async Task AddBewertungenAsync(PersonModels user, List<ProfundumBewertung> bewertungen, List<ProfundumAnker> anker)
    {
        if (user == null)
        {
            throw new ProfundumsBewertungException("User has no associated person");
        }

        if (anker == null)
        {
            throw new ProfundumsBewertungException("Anker konnte nicht gefunden werden");
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

    /// <summary>
    ///     Adds a single Bewertung that rates only an Anker for a given person and profundum instanz.
    /// </summary>
    /// <exception cref="ProfundumsBewertungException"></exception>
    public async Task<ProfundumAnkerBewertung?> AddBewertungAnkerAsync(
        Guid personId, Guid instanzId, Guid ankerId, Guid kriteriumId, int grad, bool wirdbewertet)
    {
        if (wirdbewertet)
        {
            if (grad < 1 || grad > 5)
                throw new ProfundumsBewertungException("Ungültiger Grad; muss zwischen 1 und 5 liegen.");
        }

        var person = await _dbContext.Personen.FindAsync(personId);
        if (person == null)
            throw new ProfundumsBewertungException("Person nicht gefunden");

        var instanz = await _dbContext.ProfundaInstanzen.FindAsync(instanzId);
        if (instanz == null)
            throw new ProfundumsBewertungException("Profundum-Instanz nicht gefunden");

        var anker = await _dbContext.ProfundumAnker.FindAsync(ankerId);
        if (anker == null)
            throw new ProfundumsBewertungException("Anker nicht gefunden");

        var kriterium = await _dbContext.ProfundumsBewertungKriterien.FindAsync(kriteriumId);
        if (kriterium == null)
            throw new ProfundumsBewertungException("Kriterium nicht gefunden");

        var existingBewertung = await _dbContext.ProfundumAnkerBewertungen
            .FirstOrDefaultAsync(b =>
                b.BewertetePersonId == personId &&
                b.Profundum.Id == instanzId &&
                b.Anker.Id == ankerId &&
                b.Kriterium.Id == kriteriumId);

        if (!wirdbewertet)
        {
            if (existingBewertung != null)
            {
                _dbContext.ProfundumAnkerBewertungen.Remove(existingBewertung);
                await _dbContext.SaveChangesAsync();
                return null;
            }

            return null;
        }

        if (existingBewertung != null)
        {
            existingBewertung.Bewertung = grad;
            await _dbContext.SaveChangesAsync();
            return existingBewertung;
        }

        var bewertung = new ProfundumAnkerBewertung
        {
            Id = Guid.NewGuid(),
            BewertetePersonId = person.Id,
            Kriterium = kriterium,
            BewertetePerson = person,
            Profundum = instanz,
            Anker = anker,
            Bewertung = grad
        };

        _dbContext.ProfundumAnkerBewertungen.Add(bewertung);
        await _dbContext.SaveChangesAsync();

        return bewertung;
    }

}

using Afra_App.Otium.Domain.Models;
using Afra_App.Otium.Domain.Models.Schuljahr;
using Afra_App.User.Domain.Models;
using Microsoft.EntityFrameworkCore;
using DB_Otium = Afra_App.Otium.Domain.Models.OtiumDefinition;

namespace Afra_App.Otium.Services;

/// <summary>
/// A service class for managing Otium-related operations.
/// </summary>
public class ManagementService
{
    private readonly AfraAppContext _dbContext;

    /// <summary>
    /// Called by DI
    /// </summary>
    public ManagementService(AfraAppContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Gets an Otium instance by its ID.
    /// </summary>
    /// <exception cref="KeyNotFoundException">There is no Otium with the specified id</exception>
    public async Task<DB_Otium> GetOtiumByIdAsync(Guid otiumId)
    {
        var otium = await _dbContext.Otia.FindAsync(otiumId);
        if (otium is null)
            throw new KeyNotFoundException("Otium instance not found.");

        return otium;
    }

    /// <summary>
    /// Fetches a Termin by its ID.
    /// </summary>
    /// <exception cref="KeyNotFoundException">There is no such termin</exception>
    public async Task<OtiumTermin> GetTerminByIdAsync(Guid terminId)
    {
        var termin = await _dbContext.OtiaTermine
            .Include(t => t.Otium)
            .FirstOrDefaultAsync(t => t.Id == terminId);
        if (termin is null)
            throw new KeyNotFoundException("Termin not found.");

        return termin;
    }

    /// <summary>
    /// Fetches the Otium instance associated with a given Termin.
    /// </summary>
    public async Task<DB_Otium> GetOtiumOfTerminAsync(OtiumTermin termin)
    {
        await _dbContext.Entry(termin).Reference(t => t.Otium).LoadAsync();
        return termin.Otium;
    }

    /// <summary>
    /// Gets the Block associated with a given Termin.
    /// </summary>
    public async Task<Block> GetBlockOfTerminAsync(OtiumTermin termin)
    {
        await _dbContext.Entry(termin).Reference(t => t.Block).LoadAsync();
        return termin.Block;
    }

    /// <summary>
    /// Fetches a Wiederholung by its ID.
    /// </summary>
    /// <exception cref="KeyNotFoundException">There is no such termin</exception>
    public async Task<OtiumWiederholung> GetWiederholungByIdAsync(Guid terminId)
    {
        var wiederholung = await _dbContext.OtiaWiederholungen.FindAsync(terminId);
        if (wiederholung is null)
            throw new KeyNotFoundException("Termin not found.");

        return wiederholung;
    }

    /// <summary>
    /// Fetches the Otium instance associated with a given Wiederholung.
    /// </summary>
    public async Task<DB_Otium> GetOtiumOfWiederholungAsync(OtiumWiederholung wiederholung)
    {
        await _dbContext.Entry(wiederholung).Reference(t => t.Otium).LoadAsync();
        return wiederholung.Otium;
    }

    /// <summary>
    /// Gets the list of responsible persons for a given Otium instance.
    /// </summary>
    public async Task<List<Person>> GetVerantwortlicheAsync(DB_Otium otium)
    {
        await _dbContext.Entry(otium).Collection(o => o.Verantwortliche).LoadAsync();
        return otium.Verantwortliche.ToList();
    }

    /// <summary>
    /// Continues a previously cancelled Termin.
    /// </summary>
    /// <exception cref="InvalidOperationException">The termin is not canceled.</exception>
    public async Task ContinueTerminAsync(OtiumTermin termin)
    {
        if (!termin.IstAbgesagt)
            throw new InvalidOperationException("Termin ist nicht abgesagt.");

        termin.IstAbgesagt = false;
        _dbContext.OtiaTermine.Update(termin);
        await _dbContext.SaveChangesAsync();
    }
}

using System.ComponentModel.DataAnnotations;
using Afra_App.Data;
using Afra_App.Data.Otium;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Afra_App.Controllers;

[Route("api/[controller]")]
[Authorize]
[ApiController]
public class OtiumController(AfraAppContext context)
{
    /// <summary>
    /// Retrieves the list of categories.
    /// </summary>
    /// <returns>A tree of categories.</returns>
    [Route("kategorie")]
    public IAsyncEnumerable<Kategorie> GetKategorien()
    {
        return context.OtiaKategorien.AsNoTracking()
            .Include(k => k.Children)
            .Where(k => k.Parent == null)
            .AsAsyncEnumerable();
    }

    /// <summary>
    /// Retrieves the Otium data for a given date and block.
    /// </summary>
    /// <param name="date">The date for which to retrieve the Otium data.</param>
    /// <param name="block">The block number for which to retrieve the Otium data.</param>
    /// <returns>A List of all Otia happening at that time.</returns>
    [HttpGet("{date}/{block:int}")]
    public async IAsyncEnumerable<Data.Json.Termin> GetOtium(DateOnly date, byte block)
    {
        // Okay, I recognize that this is not readable at all
        // Get the schultag for the given date and block
        var schultag = await context.Schultage
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Datum == date && s.OtiumsBlock[block]);
        
        if (schultag == null) yield break;
        
        // Get all termine for the given schultag and block
        var termine = context.OtiaTermine
            .AsNoTracking()
            .Where(t => t.Schultag == schultag && t.Block == block)
            .Include(t => t.Otium)
            .AsAsyncEnumerable();
        
        // Calculate the load for each termin and cast it to a json object
        await foreach (var termin in termine)
        {
            yield return new Data.Json.Termin(termin, await CalculateLoad(termin));
        }

    }

    /// <summary>
    /// Calculates the load for a given termin.
    /// </summary>
    /// <param name="termin">The termin for which to calculate the load.</param>
    /// <returns>The calculated load as a double, or null if MaxEinschreibungen is null.</returns>
    private async Task<double?> CalculateLoad(Termin termin)
    {
        if (termin.MaxEinschreibungen is null)
            return null;

        var minutesSum = await context.OtiaEinschreibungen.AsNoTracking()
            .Where(e => e.Termin == termin)
            .SumAsync(e => (e.Ende - e.Start).TotalMinutes);

        return minutesSum / (75 * termin.MaxEinschreibungen);
    }
}
using Afra_App.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Afra_App.Controllers;

/// <summary>
/// A controller for managing the school year.
/// </summary>
[Authorize]
[Route("api/[controller]")]
public class Schuljahr(AfraAppContext dbContext) : ControllerBase
{
    private AfraAppContext _dbContext = dbContext;

    /// <summary>
    /// Retrieves all school days in the database.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetSchuljahr()
    {
        var schultage = await _dbContext.Schultage.OrderBy(s => s.Datum).ToListAsync();
        var next = schultage.FirstOrDefault(s => s.Datum >= DateOnly.FromDateTime(DateTime.Now)) ?? schultage.Last();

        return Ok(new Data.DTO.Schuljahr(next, schultage));
    }
}
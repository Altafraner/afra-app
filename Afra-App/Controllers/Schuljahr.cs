using Afra_App.Data;
using Afra_App.Data.Schuljahr;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Afra_App.Controllers;

[Authorize]
[Route("api/[controller]")]
public class Schuljahr(AfraAppContext dbContext) : ControllerBase
{
    private AfraAppContext _dbContext = dbContext;
    
    private IAsyncEnumerable<Schultag> GetSchultage()
    {
        return _dbContext.Schultage.AsAsyncEnumerable();
    }

    [HttpGet]
    public async Task<IActionResult> GetSchuljahr()
    {
        var schultage = await _dbContext.Schultage.OrderBy(s => s.Datum).ToListAsync();
        var next = schultage.FirstOrDefault(s => s.Datum >= DateOnly.FromDateTime(DateTime.Now)) ?? schultage.Last();

        return Ok(new Data.DTO.Schuljahr(next, schultage));
    }
}
using Afra_App.Data;
using Afra_App.Data.Schuljahr;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Afra_App.Controllers;

[Authorize]
[Route("api/[controller]")]
public class Schuljahr(AfraAppContext dbContext) : ControllerBase
{
    private AfraAppContext _dbContext = dbContext;
    
    [HttpGet]
    public IEnumerable<Schultag> GetSchultage()
    {
        return _dbContext.Schultage;
    }
}
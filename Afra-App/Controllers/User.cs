using Afra_App.Authentication;
using Afra_App.Data;
using Afra_App.Data.DTO;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace Afra_App.Controllers;

/// <summary>
/// Controller for getting information about the current user.
/// </summary>
[Route("api/[controller]")]
public class User : ControllerBase
{
    /// <summary>
    /// Checks if the current user is authorized.
    /// </summary>
    /// <param name="dbContext">The database context to use for retrieving the Person.</param>
    /// <returns>An ActionResult indicating whether the user is authorized.</returns>
    [HttpGet]
    public async Task<ActionResult> IsAuthorized([FromServices] AfraAppContext dbContext)
    {
        // Check if the user is authenticated
        if (!(HttpContext.User.Identity?.IsAuthenticated ?? false)) return Unauthorized();
        
        try
        {
            // Retrieve the Person associated with the current user and return it
            var person = HttpContext.GetPerson(dbContext);
            return Ok(new PersonInfoMinimal(person));
        }
        catch (Exception e) when (e is InvalidOperationException or KeyNotFoundException)
        {
            // Sign out the user if they are (for some bizarre reason) authenticated, but the Person could not be found
            await HttpContext.SignOutAsync();
            return Unauthorized();
        }
    }
}
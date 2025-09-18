using Afra_App;
using Afra_App.Backbone.Authentication;
using Afra_App.User.Services;

/// <summary>
///    Contains endpoints for managing Profunda Bewertungen.
/// </summary>
public static class Bewertung
{
    /// <summary>
    ///     Maps the Profundum Bewertung endpoints to the given <see cref="IEndpointRouteBuilder" />.
    /// </summary>
    public static void MapBewertungEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/bewertung")
            .RequireAuthorization(AuthorizationPolicies.MittelStufeStudentOnly);
        group.MapPost("/", AddBewertungAsync);
        group.MapGet("/", GetBewertungenAsync);
    }

    ///
    private static async Task<IResult> GetBewertungenAsync(ProfundumBewertungService bewertungService,
        UserAccessor userAccessor, AfraAppContext dbContext, ILogger<ProfundumBewertungService> logger)
    {
        var user = await userAccessor.GetUserAsync();
    
        var bewertungen = await bewertungService.GetBewertungenAsync(user);
        return Results.Ok(bewertungen);
    }

    ///
    private static async Task<IResult> AddBewertungAsync(ProfundumBewertungService bewertungService,
        UserAccessor userAccessor, AfraAppContext dbContext, ILogger<ProfundumBewertungService> logger,
        List<ProfundumBewertung> bewertungen)
    {
        var user = await userAccessor.GetUserAsync();
        try
        {
            await bewertungService.AddBewertungenAsync(user, bewertungen);
        }
        catch (ProfundumBewertungException e)
        {
            return Results.BadRequest(e.Message);
        }

        return Results.Ok("Feedback gespeichert");
    }
}
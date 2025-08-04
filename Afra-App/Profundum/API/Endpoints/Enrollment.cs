using Afra_App.Profundum.Services;
using Afra_App.User.Services;

namespace Afra_App.Profundum.API.Endpoints;

/// <summary>
///     Contains endpoints for managing kategories.
/// </summary>
public static class Enrollment
{
    /// <summary>
    ///     Maps the kategorie endpoints to the given <see cref="IEndpointRouteBuilder" />.
    /// </summary>
    public static void MapEnrollmentEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/", AddBelegWunschAsync);
        app.MapGet("/", GetOptionsAsync);
        app.MapGet("/matching", MatchingAsync);
    }

    ///
    private static async Task<IResult> GetOptionsAsync(EnrollmentService enrollmentService,
        UserAccessor userAccessor, AfraAppContext dbContext, ILogger<EnrollmentService> logger)
    {
        var user = await userAccessor.GetUserAsync();
        if (user is not { Rolle: User.Domain.Models.Rolle.Mittelstufe })
        {
            logger.LogWarning("not mittelstufe");
            return Results.Forbid();
        }

        var katalog = enrollmentService.GetKatalog();
        return Results.Ok(katalog!);
    }

    ///
    private static async Task<IResult> AddBelegWunschAsync(EnrollmentService enrollmentService,
        UserAccessor userAccessor, AfraAppContext dbContext, ILogger<EnrollmentService> logger,
        Dictionary<String, Guid[]> wuensche)
    {
        var user = await userAccessor.GetUserAsync();
        if (user is not { Rolle: User.Domain.Models.Rolle.Mittelstufe })
        {
            logger.LogWarning("not mittelstufe");
            return Results.Forbid();
        }

        return await enrollmentService.RegisterBelegWunschAsync(user, wuensche);
    }

    ///
    private static async Task<IResult> MatchingAsync(EnrollmentService enrollmentService,
           UserAccessor userAccessor, AfraAppContext dbContext, ILogger<EnrollmentService> logger, Guid[] slots)
    {
        var user = await userAccessor.GetUserAsync();
        if (user is not { Rolle: User.Domain.Models.Rolle.Tutor })
        {
            logger.LogWarning("not tutor");
            return Results.Unauthorized();
        }
        var slotsMöglich = dbContext.ProfundaSlots.Where(s => s.EinwahlMöglich).Select(s => s.Id).ToArray();
        return await enrollmentService.PerformMatching(slotsMöglich);
    }

}

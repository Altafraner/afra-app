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
        app.MapPost("/", AddBelegWuenscheAsync);
        app.MapGet("/", GetOptionsAsync);
        app.MapGet("/matching", MatchingAsync);
        app.MapGet("/matching.csv", MatchingAsyncCSV);
        app.MapGet("/missing", GetUnenrolledAsync);
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

        var katalog = enrollmentService.GetKatalog(user);
        return Results.Ok(katalog!);
    }

    ///
    private static async Task<IResult> AddBelegWuenscheAsync(EnrollmentService enrollmentService,
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
           UserAccessor userAccessor, AfraAppContext dbContext, ILogger<EnrollmentService> logger)
    {
        var user = await userAccessor.GetUserAsync();
        if (user is not { Rolle: User.Domain.Models.Rolle.Tutor })
        {
            logger.LogWarning("not tutor");
            return Results.Unauthorized();
        }
        var slotsMöglich = dbContext.ProfundaSlots.Where(s => s.EinwahlMöglich).Select(s => s.Id).ToArray();
        try
        {
            var result = await enrollmentService.PerformMatching(slotsMöglich);
            return Results.Ok(result);
        }
        catch (ArgumentException e)
        {
            return Results.BadRequest(e.Message);
        }
    }

    ///
    private static async Task<IResult> MatchingAsyncCSV(EnrollmentService enrollmentService,
           UserAccessor userAccessor, AfraAppContext dbContext, ILogger<EnrollmentService> logger)
    {
        var user = await userAccessor.GetUserAsync();
        if (user is not { Rolle: User.Domain.Models.Rolle.Tutor })
        {
            logger.LogWarning("not tutor");
            return Results.Unauthorized();
        }
        var slotsMöglich = dbContext.ProfundaSlots.Where(s => s.EinwahlMöglich).Select(s => s.Id).ToArray();
        try
        {
            await enrollmentService.PerformMatching(slotsMöglich);
            var csv = await enrollmentService.GetStudentMatchingCSV(slotsMöglich);
            return Results.File(System.Text.Encoding.UTF8.GetBytes(csv), "text/csv");
        }
        catch (ArgumentException e)
        {
            return Results.BadRequest(e.Message);
        }
    }

    ///
    private static async Task<IResult> GetUnenrolledAsync(EnrollmentService enrollmentService,
           UserAccessor userAccessor, AfraAppContext dbContext, ILogger<EnrollmentService> logger, Guid[] slots)
    {
        var user = await userAccessor.GetUserAsync();
        if (user is not { Rolle: User.Domain.Models.Rolle.Tutor })
        {
            logger.LogWarning("not tutor");
            return Results.Unauthorized();
        }
        var slotsMöglich = dbContext.ProfundaSlots.Where(s => s.EinwahlMöglich).Select(s => s.Id).ToArray();
        try
        {
            var result = await enrollmentService.GetMissingStudents(slotsMöglich);
            return Results.Ok(result);
        }
        catch (ArgumentException e)
        {
            return Results.BadRequest(e.Message);
        }
    }
}

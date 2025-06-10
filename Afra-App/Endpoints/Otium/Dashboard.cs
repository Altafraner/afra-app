using Afra_App.Authentication;
using Afra_App.Data;
using Afra_App.Services.Otium;
using Afra_App.Services.User;
using Microsoft.EntityFrameworkCore;

namespace Afra_App.Endpoints.Otium;

/// <summary>
///     A class containing the endpoints for the dashboard.
/// </summary>
public static class Dashboard
{
    /// <summary>
    ///     Maps the dashboard endpoints to the given <see cref="IEndpointRouteBuilder" />.
    /// </summary>
    public static void MapDashboardEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/student", GetStudentDashboard)
            .RequireAuthorization(AuthorizationPolicies.StudentOnly);
        app.MapGet("/student/all",
                (OtiumEndpointService service, UserAccessor userAccessor) =>
                    GetStudentDashboard(service, userAccessor, true))
            .RequireAuthorization(AuthorizationPolicies.StudentOnly);

        app.MapGet("/student/{studentId:guid}", GetStudentDashboardForTeacher)
            .RequireAuthorization(AuthorizationPolicies.TutorOnly);
        app.MapGet("/student/{studentId:guid}/all",
                (OtiumEndpointService service,
                        UserAccessor userAccessor, AfraAppContext dbContext, Guid studentId) =>
                    GetStudentDashboardForTeacher(service, userAccessor, dbContext, studentId, true))
            .RequireAuthorization(AuthorizationPolicies.TutorOnly);

        app.MapGet("/teacher", GetTeacherDashboard)
            .RequireAuthorization(AuthorizationPolicies.TutorOnly);
    }

    private static async Task<IResult> GetStudentDashboard(OtiumEndpointService service,
        UserAccessor userAccessor,
        bool all = false)
    {
        var user = await userAccessor.GetUserAsync();
        return Results.Ok(service.GetStudentDashboardAsyncEnumerable(user, all));
    }

    private static async Task<IResult> GetStudentDashboardForTeacher(OtiumEndpointService service,
        UserAccessor userAccessor,
        AfraAppContext dbContext,
        Guid studentId,
        bool all = false)
    {
        var user = await userAccessor.GetUserAsync();
        var student = dbContext.Personen
            .Include(p => p.Mentor)
            .FirstOrDefault(p => p.Id == studentId);

        if (student is null) return Results.NotFound();
        if (student.Mentor is null || student.Mentor.Id != user.Id) return Results.Unauthorized();

        return Results.Ok(service.GetStudentDashboardForTeacher(student, all));
    }

    private static async Task<IResult> GetTeacherDashboard(OtiumEndpointService service,
        UserAccessor userAccessor)
    {
        var user = await userAccessor.GetUserAsync();
        return Results.Ok(await service.GetTeacherDashboardAsync(user));
    }
}

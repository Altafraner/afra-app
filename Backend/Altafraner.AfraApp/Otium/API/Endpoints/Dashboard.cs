using Altafraner.AfraApp.Backbone.Authorization;
using Altafraner.AfraApp.Otium.Services;
using Altafraner.AfraApp.User.Domain.Models;
using Altafraner.AfraApp.User.Services;

namespace Altafraner.AfraApp.Otium.API.Endpoints;

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
                        UserService userService, UserAuthorizationHelper authHelper, Guid studentId) =>
                    GetStudentDashboardForTeacher(service, userService, authHelper, studentId, true))
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
        UserService userService,
        UserAuthorizationHelper authHelper,
        Guid studentId,
        bool all = false)
    {
        Person student;
        try
        {
            student = await userService.GetUserByIdAsync(studentId);
        }
        catch (KeyNotFoundException)
        {
            return Results.NotFound();
        }

        var isMentor = await authHelper.CurrentUserIsMentorOf(student);
        var hasBypass = await authHelper.CurrentUserHasGlobalPermission(GlobalPermission.Otiumsverantwortlich) ||
                        await authHelper.CurrentUserHasGlobalPermission(GlobalPermission.Admin);

        if (!isMentor && !hasBypass) return Results.Unauthorized();

        return Results.Ok(service.GetStudentDashboardForTeacher(student, all));
    }

    private static async Task<IResult> GetTeacherDashboard(OtiumEndpointService service,
        UserAccessor userAccessor)
    {
        var user = await userAccessor.GetUserAsync();
        return Results.Ok(await service.GetTeacherDashboardAsync(user));
    }
}

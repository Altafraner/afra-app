using Altafraner.AfraApp.Backbone.Authorization;
using Altafraner.AfraApp.Dashboard.Contracts.DTO;
using Altafraner.AfraApp.Dashboard.Services;
using Altafraner.AfraApp.User.Services;
using Altafraner.Backbone.Utils;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Altafraner.AfraApp.Dashboard.API;

internal static class Dashboard
{
    public static void MapDashboardEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/dashboard");
        group.MapGet("/tutor",
                async (UserAccessor userAccessor, DashboardService dashboardService, UserService userService) =>
                {
                    var user = await userAccessor.GetUserAsync();
                    var mentees = await userService.GetMenteesAsync(user);
                    return new TutorDashboard
                    {
                        Events = await dashboardService.GetTutorDashboard(user),
                        Mentees = await dashboardService.GetMenteeStatuses(mentees.ToArray())
                    };
                })
            .RequireAuthorization(AuthorizationPolicies.TutorOnly);

        group.MapGet("/student",
                async (UserAccessor userAccessor,
                    DashboardService dashboardService,
                    DateOnly start,
                    int numWeeks) =>
                {
                    var user = await userAccessor.GetUserAsync();
                    var monday = start.GetStartOfWeek();
                    if (numWeeks <= 0) return (Results<BadRequest, Ok<StudentDashboard>>)TypedResults.BadRequest();

                    var weeks = await dashboardService.GetStudentWeeks(user, monday, numWeeks);
                    return TypedResults.Ok(new StudentDashboard { Weeks = weeks });
                })
            .RequireAuthorization(AuthorizationPolicies.StudentOnly);
    }
}

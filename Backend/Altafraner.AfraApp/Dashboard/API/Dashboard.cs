using Altafraner.AfraApp.Backbone.Authorization;
using Altafraner.AfraApp.Dashboard.Contracts.DTO;
using Altafraner.AfraApp.Dashboard.Services;
using Altafraner.AfraApp.User.Services;

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
    }
}

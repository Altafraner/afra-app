using Afra_App.Backbone.Calendar.Services;
using Afra_App.User.Services;

namespace Afra_App.Backbone.Calendar.API.Endpoints;

/// <summary>
///     A class containing the endpoints for the management and usage of calendar subscriptions
/// </summary>
public static class Calendar
{
    /// <summary>
    ///     Maps the calendar endpoints to the given <see cref="IEndpointRouteBuilder" />.
    /// </summary>
    /// <param name="app"></param>
    public static void MapCalendarEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/calendar/subscribe", SubscribeCalendarAsync).RequireAuthorization();
        app.MapGet("/api/calendar/{subId:guid}", GetCalendarAsync);
    }

    private static async Task<IResult> SubscribeCalendarAsync(UserAccessor userAccessor, CalendarService calendarService)
    {
        var user = await userAccessor.GetUserAsync();
        var subId = await calendarService.addCalendarSubscriptonAsync(user)!;
        return Results.Ok(subId);
    }

    private static async Task<IResult> GetCalendarAsync(UserAccessor userAccessor, CalendarService calendarService, Guid subId)
    {
        var cal = await calendarService.getCalendarAsync(subId);
        if (cal is null)
        {
            return Results.NotFound("No subscription found");
        }
        return Results.File(System.Text.Encoding.UTF8.GetBytes(cal), "text/calendar");
    }
}

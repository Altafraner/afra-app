using Altafraner.AfraApp.Calendar.Services;
using Altafraner.AfraApp.User.Services;

namespace Altafraner.AfraApp.Calendar.API.Endpoints;

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
        app.MapGet("/api/calendar", SubscribeCalendarAsync).RequireAuthorization();
        app.MapGet("/api/calendar/count", GetNumberOfSubscriptionsAsync).RequireAuthorization();
        app.MapDelete("/api/calendar", DeleteAllSubscriptionsAsync).RequireAuthorization();
        app.MapGet("/api/calendar/{subId:guid}.ics", GetCalendarAsync);
    }

    private static async Task<IResult> SubscribeCalendarAsync(UserAccessor userAccessor, CalendarService calendarService)
    {
        var user = await userAccessor.GetUserAsync();
        var subId = await calendarService.AddCalendarSubscriptionAsync(user)!;
        return Results.Ok(subId);
    }

    private static async Task<IResult> DeleteAllSubscriptionsAsync(UserAccessor userAccessor, CalendarService calendarService)
    {
        var user = await userAccessor.GetUserAsync();
        await calendarService.DeleteAllCalendarSubscriptionAsync(user)!;
        return Results.Ok();
    }

    private static async Task<IResult> GetNumberOfSubscriptionsAsync(UserAccessor userAccessor, CalendarService calendarService)
    {
        var user = await userAccessor.GetUserAsync();
        var subId = await calendarService.GetNumCalendarSubscriptionAsync(user)!;
        return Results.Ok(subId);
    }

    private static async Task<IResult> GetCalendarAsync(UserAccessor userAccessor, CalendarService calendarService, Guid subId)
    {
        var cal = await calendarService.GetCalendarAsync(subId);
        if (cal is null)
        {
            return Results.NotFound("No subscription found");
        }
        return Results.File(System.Text.Encoding.UTF8.GetBytes(cal), "text/calendar");
    }
}

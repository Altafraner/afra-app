using System.Security.Claims;
using Altafraner.AfraApp.Attendance.Domain.Contracts;
using Altafraner.AfraApp.Attendance.Domain.Dto;
using Altafraner.AfraApp.Backbone.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Altafraner.AfraApp.Attendance.API.Endpoints;

internal static class Supervision
{
    internal static void MapSupervisionEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/active", GetActiveSlots).RequireAuthorization(AuthorizationPolicies.TutorOnly);
    }

    private static async Task<Ok<List<AttendanceSlot>>> GetActiveSlots(IServiceProvider serviceProvider,
        ClaimsPrincipal user)
    {
        var providers = serviceProvider.GetKeyedServices<IAttendanceInformationProvider>(KeyedService.AnyKey);
        List<AttendanceSlot> slots = [];
        foreach (var provider in providers) slots.AddRange(await provider.GetAvailableSlots(user));

        return TypedResults.Ok(slots);
    }
}

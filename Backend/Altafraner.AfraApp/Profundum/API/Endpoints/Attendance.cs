using System.Security.Claims;
using Altafraner.AfraApp.Attendance.Domain.Contracts;
using Altafraner.AfraApp.Attendance.Domain.Dto;
using Altafraner.AfraApp.Attendance.Domain.HubClients;
using Altafraner.AfraApp.Backbone.Authorization;
using Altafraner.AfraApp.Profundum.Domain.DTO;
using Altafraner.AfraApp.Profundum.Services;
using Altafraner.AfraApp.User.Domain.DTO;
using Altafraner.AfraApp.User.Domain.Models;
using Altafraner.AfraApp.User.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Altafraner.AfraApp.Profundum.API.Endpoints;

internal static class Attendance
{
    public static void MapAttendanceEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/attendance/{instanceId:guid}/{terminId:guid}",
            async (ClaimsPrincipal userClaims,
                Guid instanceId,
                Guid terminId,
                AfraAppContext dbContext,
                IServiceProvider serviceProvider,
                UserAuthorizationHelper authHelper,
                IAttendanceService attendanceService) =>
            {
                var termin = await dbContext.ProfundaTermine
                    .Include(e => e.Slot)
                    .FirstOrDefaultAsync(e => e.Id == terminId);
                if (termin == null)
                    return (Results<NotFound, UnauthorizedHttpResult, Ok<ProfundumTerminInstanceInfo>>)TypedResults
                        .NotFound();
                var instance = await dbContext.ProfundaInstanzen
                    .AsSplitQuery()
                    .Include(e => e.Verantwortliche)
                    .Include(e => e.Profundum)
                    .Include(e =>
                        e.Einschreibungen.Where(e2 => e2.SlotId == termin.Slot.Id && e2.IsFixed)
                            .OrderBy(e3 => e3.BetroffenePerson.FirstName)
                            .ThenBy(e2 => e2.BetroffenePerson.LastName))
                    .ThenInclude(profundumEinschreibung => profundumEinschreibung.BetroffenePerson)
                    .FirstOrDefaultAsync(e =>
                        e.Id == instanceId && e.Slots.Any(e2 => e2.Id == termin.Slot.Id));
                if (instance is null) return TypedResults.NotFound();

                var user = await authHelper.GetUserAsync();
                if (!instance.Verantwortliche.Contains(user) &&
                    !await authHelper.CurrentUserHasGlobalPermission(GlobalPermission.Profundumsverantwortlich))
                    return TypedResults.Unauthorized();

                var provider =
                    serviceProvider.GetRequiredKeyedService<IAttendanceInformationProvider>(
                        ProfundumAttendanceInformationProvider.ScopeValue);
                var mayEditAttendance = await provider.Authorize(termin.Id, userClaims);
                var now = DateTime.Now;
                var showAttendance = termin.Day.ToDateTime(termin.StartTime) <= now;

                var attendanceEntryIds = instance.Einschreibungen.Select(e => new AttendanceEntryId
                {
                    Scope = ProfundumAttendanceInformationProvider.ScopeValue,
                    SlotId = terminId,
                    StudentId = e.BetroffenePersonId
                });

                var attendances = await attendanceService.GetAttendances(attendanceEntryIds);
                var attendanceById = attendances.ToDictionary(e => e.Key.StudentId, e => e.Value);

                return TypedResults.Ok(new ProfundumTerminInstanceInfo
                {
                    Slot = new DTOProfundumSlot(termin.Slot),
                    Label = instance.Profundum.Bezeichnung,
                    Enrollments = instance.Einschreibungen.Select(e =>
                    {
                        var attendance = attendanceById[e.BetroffenePersonId];
                        return new IAttendanceHubClient.StudentStatus(new PersonInfoMinimal(e.BetroffenePerson),
                            attendance.State,
                            attendance.Type,
                            [
                            ]);
                    }),
                    IsDoneOrStarted = showAttendance,
                    IsAttendanceEditable = mayEditAttendance,
                    Start = termin.Day.ToDateTime(termin.StartTime)
                });
            })
            .RequireAuthorization(AuthorizationPolicies.TutorOnly);
    }
}

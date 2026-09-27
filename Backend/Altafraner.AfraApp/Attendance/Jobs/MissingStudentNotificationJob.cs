using Altafraner.AfraApp.Attendance.Domain.Contracts;
using Altafraner.AfraApp.Attendance.Domain.HubClients;
using Altafraner.AfraApp.Attendance.Domain.Models;
using Altafraner.AfraApp.User.Domain.Models;
using Altafraner.AfraApp.User.Services;
using Altafraner.Backbone.EmailOutbox;
using Quartz;

namespace Altafraner.AfraApp.Attendance.Jobs;

/// <summary>
///     Sends a notification to a list of recipients about missing students in a block.
/// </summary>
internal sealed class MissingStudentNotificationJob : IJob
{
    internal const string ScopeItem = "scope";
    internal const string SlotIdItem = "slot_id";

    private readonly IServiceProvider _serviceProvider;
    private readonly IAttendanceService _attendanceService;
    private readonly ILogger<MissingStudentNotificationJob> _logger;
    private readonly IEmailOutbox _emailOutbox;
    private readonly IAttendanceNotificationService _attendanceNotificationService;
    private readonly UserService _userService;

    public MissingStudentNotificationJob(ILogger<MissingStudentNotificationJob> logger,
        IServiceProvider serviceProvider,
        IAttendanceService attendanceService,
        IEmailOutbox emailOutbox,
        IAttendanceNotificationService attendanceNotificationService,
        UserService userService)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _attendanceService = attendanceService;
        _emailOutbox = emailOutbox;
        _attendanceNotificationService = attendanceNotificationService;
        _userService = userService;
    }

    public async ValueTask Execute(IJobExecutionContext context,
        CancellationToken cancellationToken)
    {
        var scope = context.MergedJobDataMap.Get<AttendanceScope>(ScopeItem);
        var slotId = context.MergedJobDataMap.Get<Guid>(SlotIdItem);

        var informationProvider = _serviceProvider.GetRequiredKeyedService<IAttendanceInformationProvider>(scope);
        var metadata = await informationProvider.GetMetadataForSlot(slotId);

        var attendances = await _attendanceService.GetAttendanceForSlotAsync(scope, slotId);
        var allPersons = await _userService.GetUsersWithRoleAsync(Rolle.Mittelstufe);

        var allMissing = allPersons
            .Where(p => !attendances.TryGetValue(p, out var value) || value.State == AttendanceState.Fehlend)
            .ToList();
        if (allMissing.Count == 0) return;

        _logger.LogWarning("Sending report for missing students in slot {SlotId}", slotId);
        const string subject = "Fehlende Personen zum Otium";
        var len = (int)Math.Ceiling(Math.Log10(allMissing.Count));
        var body = $"""
                    Hallo,

                    es fehlen folgende Personen im aktuellen Otiums-Block:
                    {string.Join("\r\n", allMissing.Select((p, i) => $"{(i + 1).ToString().PadLeft(len)}. {p.FirstName} {p.LastName}"))}
                    """;
        foreach (var recipient in metadata.MissingStudentsNotificationRecipients)
            await _emailOutbox.SendReportAsync(recipient, subject, body);

        var successNotification = new IAttendanceHubClient.Notification(
            "Benachrichtigungen gesendet",
            $"Es wurden Benachrichtigungen über die Abwesenheit von {allMissing.Count} Schüler:innen versandt.",
            IAttendanceHubClient.NotificationSeverity.Info);
        await _attendanceNotificationService.SendNotificationToSlot(scope, slotId, successNotification);
        _logger.LogInformation("Successfully sent missing students report for slot {SlotId}", slotId);
    }
}

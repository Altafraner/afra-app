using Altafraner.AfraApp.Attendance.Domain.Contracts;
using Altafraner.AfraApp.Attendance.Domain.HubClients;
using Altafraner.AfraApp.Attendance.Domain.Models;
using Quartz;

namespace Altafraner.AfraApp.Attendance.Jobs;

internal sealed class MissingStudentsNotificationWarningJob : IJob
{
    internal const string ScopeItem = "scope";
    internal const string SlotIdItem = "slot_id";

    private readonly IAttendanceNotificationService _notificationService;

    public MissingStudentsNotificationWarningJob(IAttendanceNotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var scope = (AttendanceScope)context.MergedJobDataMap[ScopeItem];
        var slotId = Guid.Parse((string)context.MergedJobDataMap[SlotIdItem]);

        var notification = new IAttendanceHubClient.Notification(
            "Benachrichtigungen werden bald gesendet",
            "In fünf Minuten wird eine Benachrichtigung über alle Personen gesendet, die im aktuellen Slot fehlen.",
            IAttendanceHubClient.NotificationSeverity.Warning);
        await _notificationService.SendNotificationToSlot(scope, slotId, notification);
    }
}

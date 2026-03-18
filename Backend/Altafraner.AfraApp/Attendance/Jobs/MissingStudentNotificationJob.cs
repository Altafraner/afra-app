using Altafraner.AfraApp.Attendance.Domain.Contracts;
using Altafraner.AfraApp.Attendance.Domain.HubClients;
using Altafraner.AfraApp.Attendance.Domain.Models;
using Altafraner.AfraApp.User.Domain.Models;
using Altafraner.Backbone.EmailOutbox;
using Altafraner.Backbone.Scheduling;
using Quartz;

namespace Altafraner.AfraApp.Attendance.Jobs;

/// <summary>
///     Sends a notification to a list of recipients about missing students in a block.
/// </summary>
internal sealed class MissingStudentNotificationJob : RetryJob
{
    internal const string ScopeItem = "scope";
    internal const string SlotIdItem = "slot_id";

    private readonly IServiceProvider _serviceProvider;
    private readonly IAttendanceService _attendanceService;
    private readonly ILogger<MissingStudentNotificationJob> _logger;
    private readonly IEmailOutbox _emailOutbox;
    private readonly IAttendanceNotificationService _attendanceNotificationService;

    public MissingStudentNotificationJob(ILogger<MissingStudentNotificationJob> logger,
        IServiceProvider serviceProvider,
        IAttendanceService attendanceService,
        IEmailOutbox emailOutbox,
        IAttendanceNotificationService attendanceNotificationService) : base(logger)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _attendanceService = attendanceService;
        _emailOutbox = emailOutbox;
        _attendanceNotificationService = attendanceNotificationService;
    }

    protected override int MaxRetryCount => 5;
    protected override TimeSpan GetRetryDelay(int retryCount) => TimeSpan.FromMinutes(1);

    protected override async Task ExecuteAsync(IJobExecutionContext context, int _)
    {
        var scope = (AttendanceScope)context.MergedJobDataMap[ScopeItem];
        var slotId = (Guid)context.MergedJobDataMap[SlotIdItem];

        var informationProvider = _serviceProvider.GetRequiredKeyedService<IAttendanceInformationProvider>(scope);
        var metadata = await informationProvider.GetMetadataForSlot(slotId);

        var attendances = await _attendanceService.GetAttendanceForSlotAsync(scope, slotId);
        var enrollments = await informationProvider.GetEnrollmentsForSlot(slotId);
        var enrolledPersons = enrollments.SelectMany(e => e.Enrollments).Distinct().ToHashSet();

        var allMissing = attendances
            .Where(e => enrolledPersons.Contains(e.Key) && e.Key.Rolle == Rolle.Mittelstufe)
            .Select(e => e.Key)
            .Distinct()
            .OrderBy(p => p.LastName)
            .ThenBy(p => p.FirstName)
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

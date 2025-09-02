using Afra_App.Backbone.Email.Services.Contracts;
using Afra_App.Backbone.Scheduler.Templates;
using Afra_App.Otium.API.Hubs;
using Afra_App.Otium.Configuration;
using Afra_App.Otium.Domain.Contracts.Services;
using Afra_App.Otium.Domain.HubClients;
using Afra_App.Otium.Domain.Models;
using Afra_App.Otium.Services;
using Afra_App.User.Domain.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using Quartz;

namespace Afra_App.Otium.Jobs;

/// <summary>
///     Sends a notification to a list of recipients about missing students in a block.
/// </summary>
internal sealed class MissingStudentNotificationJob : RetryJob
{
    private readonly IHubContext<AttendanceHub, IAttendanceHubClient> _attendanceHub;
    private readonly IAttendanceService _attendanceService;
    private readonly BlockHelper _blockHelper;
    private readonly IEmailOutbox _emailOutbox;
    private readonly ILogger<MissingStudentNotificationJob> _logger;
    private readonly IOptions<OtiumConfiguration> _otiumConfiguration;

    public MissingStudentNotificationJob(
        IAttendanceService attendanceService,
        IEmailOutbox emailOutbox,
        IHubContext<AttendanceHub, IAttendanceHubClient> attendanceHub,
        IOptions<OtiumConfiguration> otiumConfiguration,
        ILogger<MissingStudentNotificationJob> logger,
        BlockHelper blockHelper) : base(logger)
    {
        _attendanceService = attendanceService;
        _emailOutbox = emailOutbox;
        _attendanceHub = attendanceHub;
        _otiumConfiguration = otiumConfiguration;
        _logger = logger;
        _blockHelper = blockHelper;
    }

    protected override int MaxRetryCount => 5;
    protected override TimeSpan GetRetryDelay(int retryCount) => TimeSpan.FromMinutes(1);

    protected override async Task ExecuteAsync(IJobExecutionContext context, int _)
    {
        if (!_otiumConfiguration.Value.MissingStudentsReport.Enabled)
        {
            _logger.LogInformation("Missing students report is disabled, skipping job execution.");
            return;
        }

        var blockId = context.MergedJobDataMap.GetGuidValueFromString("block");
        var schemaId = context.MergedJobDataMap.GetChar("block_schema");
        var schema = _blockHelper.Get(schemaId)!;

        if (!_otiumConfiguration.Value.MissingStudentsReport.Enabled)
            return;

        var (terminAttendance, missingPersons, missingPersonsChecked) =
            await _attendanceService.GetAttendanceForBlockAsync(blockId);

        var missingEnrolled = terminAttendance.Values
            .SelectMany(d => d.AsEnumerable())
            .Where(e => e.Value == OtiumAnwesenheitsStatus.Fehlend);
        var othersAnwesenheitsStatus = schema.Verpflichtend
            ? missingPersons
                .Where(e => e.Value == OtiumAnwesenheitsStatus.Fehlend)
            : [];

        var allMissing = missingEnrolled
            .Concat(othersAnwesenheitsStatus)
            .Where(e => e.Key.Rolle == Rolle.Mittelstufe)
            .Select(e => e.Key)
            .DistinctBy(p => p.Id)
            .OrderBy(p => p.Nachname)
            .ThenBy(p => p.Vorname)
            .ToList();

        if (allMissing.Count == 0) return;

        if (!context.MergedJobDataMap.TryGetBoolean("warning_send", out var warningSend) || !warningSend)
        {
            _logger.LogInformation("Sending warning notification for supervisors of block {BlockId}", blockId);
            var fireAfter = TimeOnly.Parse(context.MergedJobDataMap.GetString("fire_after")!);
            var now = TimeOnly.FromDateTime(DateTime.Now);

            if (now < fireAfter && !missingPersonsChecked ||
                terminAttendance.Keys.Any(k => !k.SindAnwesenheitenKontrolliert)) return;

            var notification = new IAttendanceHubClient.Notification(
                "Benachrichtigungen werden bald gesendet",
                "In fünf Minuten wird eine Benachrichtigung über alle Personen gesendet, die im aktuellen Otium-Block fehlen.",
                IAttendanceHubClient.NotificationSeverity.Warning);
            await _attendanceHub.Clients.Group(AttendanceHub.BlockGroupName(blockId)).Notify(notification);
            foreach (var termin in terminAttendance.Keys)
                await _attendanceHub.Clients.Group(AttendanceHub.TerminGroupName(termin.Id)).Notify(notification);

            var trigger = TriggerBuilder.Create()
                .ForJob(context.JobDetail.Key)
                .UsingJobData("warning_send", true)
                .StartAt(DateTimeOffset.Now.AddMinutes(5))
                .Build();
            await context.Scheduler.RescheduleJob(context.Trigger.Key, trigger);
            return;
        }

        _logger.LogWarning("Sending report for missing students in block {BlockId}", blockId);
        const string subject = "Fehlende Personen zum Otium";
        var len = (int)Math.Ceiling(Math.Log10(allMissing.Count));
        var body = $"""
                    Hallo,

                    es fehlen folgende Personen im aktuellen Otiums-Block:
                    {string.Join("\r\n", allMissing.Select((p, i) => $"{(i + 1).ToString().PadLeft(len)}. {p.Vorname} {p.Nachname}"))}
                    """;
        foreach (var recipient in _otiumConfiguration.Value.MissingStudentsReport.Recipients)
            await _emailOutbox.SendReportAsync(recipient, subject, body);

        var successNotification = new IAttendanceHubClient.Notification(
            "Benachrichtigungen gesendet",
            $"Es wurden Benachrichtigungen über die Abwesenheit von {allMissing.Count} Schüler:innen versandt.",
            IAttendanceHubClient.NotificationSeverity.Info);
        await _attendanceHub.Clients.Group(AttendanceHub.BlockGroupName(blockId)).Notify(successNotification);
        foreach (var termin in terminAttendance.Keys)
            await _attendanceHub.Clients.Group(AttendanceHub.TerminGroupName(termin.Id))
                .Notify(successNotification);
        _logger.LogInformation("Successfully sent missing students report for block {BlockId}", blockId);
    }
}
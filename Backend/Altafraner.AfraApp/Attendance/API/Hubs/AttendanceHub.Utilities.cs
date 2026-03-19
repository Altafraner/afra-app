using Altafraner.AfraApp.Attendance.Domain.Contracts;
using Altafraner.AfraApp.Attendance.Domain.Dto.Enrollments;
using Altafraner.AfraApp.Attendance.Domain.Models;
using Altafraner.AfraApp.Attendance.Jobs;
using Microsoft.AspNetCore.SignalR;
using Quartz;

namespace Altafraner.AfraApp.Attendance.API.Hubs;

internal partial class AttendanceHub
{
    private const string ScopeItem = "scope";
    private const string TypeItem = "type";
    private const string SlotIdItem = "slot_id";
    private const string EventIdItem = "event_id";

    private AttendanceScope? Scope
    {
        get => (AttendanceScope?)Context.Items[ScopeItem];
        set => Context.Items[ScopeItem] = value;
    }

    private AttendanceType Type
    {
        get => (AttendanceType)Context.Items[TypeItem]!;
        set => Context.Items[TypeItem] = value;
    }

    private Guid SlotId
    {
        get => (Guid)Context.Items[SlotIdItem]!;
        set => Context.Items[SlotIdItem] = value;
    }

    private Guid? EventId
    {
        get => (Guid?)Context.Items[EventIdItem];
        set => Context.Items[EventIdItem] = value;
    }

    internal static string EventGroupName(AttendanceScope scope, Guid slotId, Guid eventId)
    {
        return $"event-{scope}-{slotId}-{eventId}";
    }

    internal static string SlotGroupName(AttendanceScope scope, Guid terminId)
    {
        return $"block-{scope}-{terminId}";
    }

    private IAttendanceInformationProvider GetInformationProvider()
    {
        var scope = (AttendanceScope)Context.Items[ScopeItem]!;
        return _serviceProvider.GetRequiredKeyedService<IAttendanceInformationProvider>(scope);
    }

    private async Task Authorize(IAttendanceInformationProvider informationProvider)
    {
        var isAuthenticated = await informationProvider.Authorize(SlotId, Context.User!);
        if (!isAuthenticated) throw new HubException("You are not authorized to access this slot");
    }

    private void EnsureSubscribed()
    {
        if (!Context.Items.ContainsKey(ScopeItem))
            throw new HubException("You must subscribe to a slot or event first");
    }

    private async Task ScheduleMissingStudentNotifications(AttendanceSlotMetadata metadata)
    {
        if (!metadata.MissingStudentsNotificationTime.HasValue) return;

        var warningTime = metadata.MissingStudentsNotificationTime.Value.AddMinutes(-5);
        if (DateTime.Now >= warningTime) return;

        var schedulerFactory = _serviceProvider.GetRequiredService<ISchedulerFactory>();
        var scheduler = await schedulerFactory.GetScheduler();
        var warningJobKey = new JobKey($"missing_student_notification_warning-{Scope!.Value}-{SlotId}");
        var notificationJobKey = new JobKey($"missing_student_notification-{Scope!.Value}-{SlotId}");

        if (!await scheduler.CheckExists(warningJobKey))
        {
            var job = JobBuilder.Create<MissingStudentsNotificationWarningJob>()
                .WithIdentity(warningJobKey)
                .UsingJobData(MissingStudentsNotificationWarningJob.ScopeItem, (int)Scope!.Value)
                .UsingJobData(MissingStudentsNotificationWarningJob.SlotIdItem, SlotId)
                .DisallowConcurrentExecution()
                .Build();
            var trigger = TriggerBuilder.Create()
                .WithIdentity($"missing_student_notification_warning-trigger-{Scope!.Value}-{SlotId}")
                .StartAt(warningTime)
                .Build();
            await scheduler.ScheduleJob(job, trigger);
        }

        if (!await scheduler.CheckExists(notificationJobKey))
        {
            var job = JobBuilder.Create<MissingStudentNotificationJob>()
                .WithIdentity(notificationJobKey)
                .UsingJobData(MissingStudentNotificationJob.ScopeItem, (int)Scope!.Value)
                .UsingJobData(MissingStudentNotificationJob.SlotIdItem, SlotId)
                .DisallowConcurrentExecution()
                .Build();
            var trigger = TriggerBuilder.Create()
                .WithIdentity($"missing_student_notification-trigger-{Scope!.Value}-{SlotId}")
                .StartAt(metadata.MissingStudentsNotificationTime.Value)
                .Build();
            await scheduler.ScheduleJob(job, trigger);
        }
    }

    private enum AttendanceType
    {
        Event,
        Slot
    }
}

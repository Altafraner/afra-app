using Afra_App.Otium.Configuration;
using Afra_App.Otium.Jobs;
using Microsoft.Extensions.Options;
using Quartz;

namespace Afra_App.Otium.Services;

/// <summary>
///     A background service that schedules a job to remind users about their missing enrollments in Otium events.
/// </summary>
public class EnrollmentReminderScheduler : BackgroundService
{
    private const string GroupName = "otium-enrollment-reminder";
    private const string JobName = "enrollment-reminder-job";
    private readonly ILogger<EnrollmentReminderScheduler> _logger;
    private readonly IOptions<OtiumConfiguration> _otiumConfiguration;

    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    ///     Constructor for the EnrollmentReminderService.
    /// </summary>
    public EnrollmentReminderScheduler(IServiceProvider serviceProvider,
        IOptions<OtiumConfiguration> otiumConfiguration,
        ILogger<EnrollmentReminderScheduler> logger)
    {
        _serviceProvider = serviceProvider;
        _otiumConfiguration = otiumConfiguration;
        _logger = logger;
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_otiumConfiguration.Value.EnrollmentReminder.Enabled)
        {
            _logger.LogInformation("Reminders are disabled. Skipping enrollment reminder job scheduling.");
            return;
        }

        var defaultReminderTime = _otiumConfiguration.Value.EnrollmentReminder.Time;

        using var scope = _serviceProvider.CreateScope();
        var schedulerFactory = scope.ServiceProvider.GetRequiredService<ISchedulerFactory>();
        var scheduler = await schedulerFactory.GetScheduler(stoppingToken);
        var key = new JobKey(JobName, GroupName);

        var triggerNow = TriggerBuilder.Create()
            .ForJob(key)
            .StartNow()
            .Build();

        var triggerCron = TriggerBuilder.Create()
            .ForJob(key)
            .WithSchedule(CronScheduleBuilder.DailyAtHourAndMinute(defaultReminderTime.Hour, defaultReminderTime.Minute)
                .WithMisfireHandlingInstructionFireAndProceed())
            .Build();

        var exists = await scheduler.CheckExists(key, stoppingToken);
        if (exists)
        {
            var triggers = await scheduler.GetTriggersOfJob(key, stoppingToken);
            await scheduler.UnscheduleJobs(triggers.Select(t => t.Key).ToList(), stoppingToken);

            await scheduler.ScheduleJob(triggerCron, stoppingToken);
            await scheduler.ScheduleJob(triggerNow, stoppingToken);
            return;
        }

        var job = JobBuilder.Create<EnrollmentReminderJob>()
            .PersistJobDataAfterExecution()
            .DisallowConcurrentExecution()
            .StoreDurably()
            .WithIdentity(key)
            .Build();

        await scheduler.AddJob(job, false, stoppingToken);
        await scheduler.ScheduleJob(triggerCron, stoppingToken);
        await scheduler.ScheduleJob(triggerNow, stoppingToken);
    }
}

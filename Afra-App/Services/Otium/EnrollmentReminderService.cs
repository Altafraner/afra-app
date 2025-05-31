using Quartz;

namespace Afra_App.Services.Otium;

/// <summary>
/// A background service that schedules a job to remind users about their missing enrollments in Otium events.
/// </summary>
public class EnrollmentReminderService : BackgroundService
{
    private const string GroupName = "otium-enrollment-reminder";
    private const string JobName = "enrollment-reminder-job";
    public static readonly TimeOnly DefaultReminderTime = new(17, 0);
    private readonly ILogger<EnrollmentReminderService> _logger;

    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Constructor for the EnrollmentReminderService.
    /// </summary>
    public EnrollmentReminderService(IServiceProvider serviceProvider, ILogger<EnrollmentReminderService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var schedulerFactory = scope.ServiceProvider.GetRequiredService<ISchedulerFactory>();
        var scheduler = await schedulerFactory.GetScheduler(stoppingToken);
        var key = new JobKey(JobName, GroupName);

        var triggerNow = TriggerBuilder.Create()
            .ForJob(key)
            .StartNow()
            .Build();

        var exists = await scheduler.CheckExists(key, stoppingToken);
        if (exists)
        {
            var triggers = await scheduler.GetTriggersOfJob(key, stoppingToken);
            await scheduler.UnscheduleJobs(triggers.Select(t => t.Key).ToList(), stoppingToken);

            await scheduler.ScheduleJob(CreateTrigger(), stoppingToken);
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
        await scheduler.ScheduleJob(CreateTrigger(), stoppingToken);
        await scheduler.ScheduleJob(triggerNow, stoppingToken);

        return;

        ITrigger CreateTrigger() => TriggerBuilder.Create()
            .ForJob(key)
            .WithSchedule(CronScheduleBuilder.DailyAtHourAndMinute(DefaultReminderTime.Hour, DefaultReminderTime.Minute)
                .WithMisfireHandlingInstructionFireAndProceed())
            .Build();
    }
}

using Quartz;

namespace Afra_App.Backbone.Scheduler.Templates;

/// <summary>
/// A base class for jobs that should be retried in case of failure.
/// </summary>
public abstract partial class RetryJob : IJob
{
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="RetryJob"/> class.
    /// </summary>
    protected RetryJob(ILogger logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task Execute(IJobExecutionContext context)
    {
        var hasRetryCount = context.MergedJobDataMap.TryGetIntValue("retryCount", out var retryCount);
        if (!hasRetryCount) retryCount = 0;

        if (retryCount != 0)
        {
            _logger.LogInformation("The job {JobName} is being retried. Attempt {RetryCount}/{MaxRetryCount}.",
                context.JobDetail.Key.Name, retryCount, MaxRetryCount);
        }

        try
        {
            await ExecuteAsync(context, retryCount);
        }
        catch (Exception e)
        {
            await HandleFailureAsync(context, retryCount, e);
        }
    }

    private async Task HandleFailureAsync(IJobExecutionContext context, int retryCount, Exception e)
    {
        retryCount++;

        if (retryCount > MaxRetryCount)
        {
            _logger.LogError("The job {JobName} has reached the maximum retry count of {MaxRetryCount}.",
                context.JobDetail.Key.Name, MaxRetryCount);
            try
            {
                await HandleFinalFailureAsync(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "The jobs {JobName} final failure handler failed.",
                    context.JobDetail.Key.Name);
            }

            throw new JobExecutionException(e)
            {
                RefireImmediately = false,
                UnscheduleAllTriggers = true,
            };
        }

        _logger.LogError(e, "The job {JobName} failed. Scheduling retry {RetryCount}/{MaxRetryCount}.",
            context.JobDetail.Key.Name, retryCount, MaxRetryCount);

        var trigger = TriggerBuilder.Create()
            .ForJob(context.JobDetail.Key)
            .UsingJobData("retryCount", retryCount)
            .StartAt(DateTimeOffset.Now.Add(GetRetryDelay(retryCount - 1)))
            .Build();
        await context.Scheduler.RescheduleJob(context.Trigger.Key, trigger);
        throw new JobExecutionException(e)
        {
            RefireImmediately = false,
            UnscheduleAllTriggers = false,
            UnscheduleFiringTrigger = true
        };
    }
}

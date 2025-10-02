using Quartz;

namespace Altafraner.AfraApp.Backbone.Scheduler.Templates;

public abstract partial class RetryJob
{
    /// <summary>
    /// How many times the job should be retried in case of failure.
    /// </summary>
    protected abstract int MaxRetryCount { get; }

    /// <summary>
    /// This method is called when the job is executed.
    /// </summary>
    /// <param name="context">The execution context</param>
    /// <param name="retryCount">How often this job has failed to execute in the past</param>
    protected abstract Task ExecuteAsync(IJobExecutionContext context, int retryCount);

    /// <summary>
    /// How long to wait before retrying the job in case of failure.
    /// </summary>
    /// <param name="retryCount">How often this job has been retried.</param>
    /// <remarks>Default implements random exponential backof with 3-4 min baseline</remarks>
    protected virtual TimeSpan GetRetryDelay(int retryCount) =>
        TimeSpan.FromMinutes((3 + Random.Shared.NextDouble()) * Math.Pow(2, retryCount));

    /// <summary>
    /// This method is called when the job has failed to execute and has reached the maximum retry count.
    /// </summary>
    /// <param name="context">The execution context</param>
    protected virtual Task HandleFinalFailureAsync(IJobExecutionContext context) => Task.CompletedTask;
}

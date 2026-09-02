using Altafraner.AfraApp.Profundum.Domain.DTO;

namespace Altafraner.AfraApp.Profundum.Services;

/// <summary>
///     Runs <see cref="ProfundumMatchingService.PerformMatching" /> in the background, detached from the HTTP
///     request that triggered it, and keeps it mutually exclusive with manual enrollment edits. The solver can run
///     for minutes, which is too long for a browser to hold a request open for - instead, starting a run returns
///     immediately and callers poll <see cref="GetStatus" /> for progress/completion. Registered as a singleton:
///     there is at most one matching run in the whole process at a time, and its state must survive past the
///     lifetime of the request that started it.
/// </summary>
public class ProfundumMatchingRunner
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ProfundumMatchingRunner> _logger;
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly object _statusLock = new();
    private DTOMatchingJobStatus _status = new() { Status = MatchingJobStatus.Idle };

    ///
    public ProfundumMatchingRunner(IServiceProvider serviceProvider, ILogger<ProfundumMatchingRunner> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    /// <summary>
    ///     Starts a matching run in the background if nothing (another run, a manual enrollment edit) currently
    ///     holds the exclusivity lock.
    /// </summary>
    /// <returns>False if the lock was already held and nothing was started.</returns>
    public bool TryStart()
    {
        if (!_semaphore.Wait(0)) return false;

        lock (_statusLock)
        {
            _status = new DTOMatchingJobStatus { Status = MatchingJobStatus.Running, StartedAt = DateTime.UtcNow };
        }

        _ = RunAsync();
        return true;
    }

    /// <summary>
    ///     Runs <paramref name="action" /> (e.g. a manual enrollment edit) if no matching run currently holds the
    ///     exclusivity lock.
    /// </summary>
    /// <returns>False if the lock was already held and <paramref name="action" /> did not run.</returns>
    public async Task<bool> TryRunExclusiveAsync(Func<Task> action)
    {
        if (!await _semaphore.WaitAsync(0)) return false;
        try
        {
            await action();
            return true;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <summary>
    ///     A snapshot of the current or most recently finished run.
    /// </summary>
    public DTOMatchingJobStatus GetStatus()
    {
        lock (_statusLock)
        {
            return _status;
        }
    }

    private async Task RunAsync()
    {
        // A fresh scope, since the request that called TryStart() may already have returned by the time this runs.
        using var scope = _serviceProvider.CreateScope();
        var matchingService = scope.ServiceProvider.GetRequiredService<ProfundumMatchingService>();

        try
        {
            var result = await matchingService.PerformMatching();
            lock (_statusLock)
            {
                _status = new DTOMatchingJobStatus
                {
                    Status = MatchingJobStatus.Completed,
                    Result = result,
                    StartedAt = _status.StartedAt,
                    FinishedAt = DateTime.UtcNow,
                };
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Profundum matching run failed");
            lock (_statusLock)
            {
                _status = new DTOMatchingJobStatus
                {
                    Status = MatchingJobStatus.Failed,
                    Error = e.Message,
                    StartedAt = _status.StartedAt,
                    FinishedAt = DateTime.UtcNow,
                };
            }
        }
        finally
        {
            _semaphore.Release();
        }
    }
}

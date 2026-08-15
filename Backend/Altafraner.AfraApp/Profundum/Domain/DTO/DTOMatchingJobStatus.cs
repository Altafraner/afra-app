namespace Altafraner.AfraApp.Profundum.Domain.DTO;

/// <summary>
///     The state of the (at most one, process-wide) background matching run.
/// </summary>
public enum MatchingJobStatus
{
    /// <summary>
    ///     No matching run has completed yet since the app started, and none is currently running.
    /// </summary>
    Idle,

    /// <summary>
    ///     A matching run is currently in progress.
    /// </summary>
    Running,

    /// <summary>
    ///     The most recent matching run finished successfully; see <see cref="DTOMatchingJobStatus.Result" />.
    /// </summary>
    Completed,

    /// <summary>
    ///     The most recent matching run threw; see <see cref="DTOMatchingJobStatus.Error" />.
    /// </summary>
    Failed,
}

/// <summary>
///     Snapshot of the background matching runner, returned both when a run is (re-)started and when polled.
/// </summary>
public record DTOMatchingJobStatus
{
    /// <summary>The current state of the runner.</summary>
    public required MatchingJobStatus Status { get; set; }

    /// <summary>The result of the most recent successful run, if <see cref="Status" /> is <see cref="MatchingJobStatus.Completed" />.</summary>
    public MatchingStats? Result { get; set; }

    /// <summary>The error message of the most recent failed run, if <see cref="Status" /> is <see cref="MatchingJobStatus.Failed" />.</summary>
    public string? Error { get; set; }

    /// <summary>When the current or most recent run was started.</summary>
    public DateTime? StartedAt { get; set; }

    /// <summary>When the most recent run finished (successfully or not); null while <see cref="Status" /> is <see cref="MatchingJobStatus.Running" />.</summary>
    public DateTime? FinishedAt { get; set; }
}

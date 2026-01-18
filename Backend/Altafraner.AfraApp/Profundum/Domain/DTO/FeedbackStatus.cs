namespace Altafraner.AfraApp.Profundum.Domain.DTO;

/// <summary>
///     Indicates how far along the feedback for a specific profundum instance is.
/// </summary>
public enum FeedbackStatus
{
    /// <summary>
    ///     There is no feedback
    /// </summary>
    Missing,

    /// <summary>
    ///     There is feedback for some students, but not all of them
    /// </summary>
    Partial,

    /// <summary>
    ///     Feedback has been provided for all students
    /// </summary>
    Done
}

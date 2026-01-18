namespace Altafraner.AfraApp.Profundum.Domain.DTO;

/// <summary>
///     Information on the status of the feedback process for a profundum instance
/// </summary>
public record FeedbackOverview
{
    /// <summary>
    ///     The instance the feedback relates to
    /// </summary>
    public required DTOProfundumInstanz Instanz { get; set; }

    /// <summary>
    ///     The slot the information is for
    /// </summary>
    public required DTOProfundumSlot Slot { get; set; }

    /// <summary>
    ///     The current status
    /// </summary>
    public required FeedbackStatus Status { get; set; }
}

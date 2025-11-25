namespace Altafraner.AfraApp.Otium.Domain.DTO.Notiz;

/// <summary>
///     A request to create a new note
/// </summary>
public record NotizCreationRequest
{
    /// <summary>
    ///     The notes content
    /// </summary>
    public required string Content { get; set; }

    /// <summary>
    ///     The block the note is for
    /// </summary>
    public Guid BlockId { get; set; }

    /// <summary>
    ///     The student the note is for
    /// </summary>
    public Guid StudentId { get; set; }
}

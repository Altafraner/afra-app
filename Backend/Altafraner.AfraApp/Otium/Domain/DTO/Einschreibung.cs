using Altafraner.AfraApp.Otium.Domain.Models;

namespace Altafraner.AfraApp.Otium.Domain.DTO;

/// <summary>
///     A DTO for an enrollment
/// </summary>
public record Einschreibung
{
    /// <summary>
    ///     The ID of the termin the enrollment is for
    /// </summary>
    public Guid TerminId { get; set; } = Guid.Empty;

    /// <summary>
    ///     The date the enrollment is for
    /// </summary>
    public required DateOnly Datum { get; set; }

    /// <summary>
    ///     The ID of the category the enrollment is for
    /// </summary>
    public Guid KategorieId { get; set; }

    /// <summary>
    ///     The name of the otium the enrollment is for
    /// </summary>
    public string? Otium { get; set; }

    /// <summary>
    ///     The location where the enrollment is happening
    /// </summary>
    public string? Ort { get; set; }

    /// <summary>
    ///     The name of the block the enrollment is for
    /// </summary>
    public required string Block { get; set; }

    /// <summary>
    ///     The users attendance status for the enrollment
    /// </summary>
    public OtiumAnwesenheitsStatus? Anwesenheit { get; set; }
}

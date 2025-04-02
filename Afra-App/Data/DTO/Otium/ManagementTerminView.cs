using System.Diagnostics.CodeAnalysis;

namespace Afra_App.Data.DTO.Otium;

/// <summary>
///     A DTO for the view of a Termin in the management ui
/// </summary>
public record ManagementTerminView
{
    /// <summary>
    ///     Construct an empty ManagementTerminView
    /// </summary>
    public ManagementTerminView()
    {
    }

    /// <summary>
    ///     Construct a ManagementTerminView from a Database Termin
    /// </summary>
    [SetsRequiredMembers]
    public ManagementTerminView(Data.Otium.Termin termin)
    {
        Id = termin.Id;
        Ort = termin.Ort;
        Tutor = termin.Tutor is not null ? new PersonInfoMinimal(termin.Tutor) : null;
        MaxEinschreibungen = termin.MaxEinschreibungen;
        Block = termin.Block.Nummer;
        Datum = termin.Block.Schultag.Datum;
        IstAbgesagt = termin.IstAbgesagt;
    }

    /// <summary>
    /// The id of the Termin database entry
    /// </summary>
    public required Guid Id { get; set; }

    /// <summary>
    ///     The number of the Block the Termin is on.
    /// </summary>
    public required sbyte Block { get; set; }

    /// <summary>
    ///     The date the Termin is on.
    /// </summary>
    public required DateOnly Datum { get; set; }

    /// <summary>
    ///     A maximum number of concurrent enrollments for the Termin. If null, no limit is set.
    /// </summary>
    public required int? MaxEinschreibungen { get; set; }

    /// <summary>
    ///     The Information on the tutor of the Otium. Could be a student or a teacher.
    /// </summary>
    public required PersonInfoMinimal? Tutor { get; set; }

    /// <summary>
    ///     The location for the Otium.
    /// </summary>
    public required string Ort { get; set; }

    /// <summary>
    ///     Whether the Termin is cancelled or not.
    /// </summary>
    public required bool IstAbgesagt { get; set; }
}
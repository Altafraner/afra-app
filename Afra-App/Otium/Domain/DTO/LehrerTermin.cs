using Afra_App.Otium.Domain.Models;
using Afra_App.User.Domain.DTO;

namespace Afra_App.Otium.Domain.DTO;

/// <summary>
///     A DTO for a termin as seen by a tutor
/// </summary>
public record LehrerTermin : ITermin
{
    /// <summary>
    ///     A list of enrollments for the termin
    /// </summary>
    public required IEnumerable<LehrerEinschreibung> Einschreibungen { get; set; }

    /// <summary>
    ///     The date of the termin
    /// </summary>
    public required DateOnly Datum { get; set; }

    /// <summary>
    ///     True iff the termin has been cancelled
    /// </summary>
    public bool IstAbgesagt { get; set; }

    /// <summary>
    /// true, iff the termin is currently running
    /// </summary>
    public bool IsRunning { get; set; } = false;

    /// <summary>
    ///     The maximum number of people that can be at the termin concurrently. Null if there is no limit
    /// </summary>
    public int? MaxEinschreibungen { get; set; }

    /// <summary>
    ///     The unique identifier of the block this termin belongs to
    /// </summary>
    public required Guid BlockId { get; set; }

    /// <inheritdoc />
    public required char Block { get; set; }

    /// <inheritdoc />
    public required Guid Id { get; set; }

    /// <inheritdoc />
    public required string Otium { get; set; }

    /// <inheritdoc />
    public required Guid OtiumId { get; set; }

    /// <inheritdoc />
    public required string Ort { get; set; }

    /// <inheritdoc />
    public required PersonInfoMinimal? Tutor { get; set; }
}

/// <summary>
///     A DTO for a enrollment in a termin as seen by a tutor
/// </summary>
/// <param name="Student">The student enrolled</param>
/// <param name="Anwesenheit">The current verification status</param>
public record LehrerEinschreibung(
    PersonInfoMinimal? Student,
    AnwesenheitsStatus Anwesenheit);

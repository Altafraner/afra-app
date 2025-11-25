using Altafraner.AfraApp.Otium.Domain.Models;
using Altafraner.AfraApp.Otium.Domain.Models.TimeInterval;
using Altafraner.AfraApp.User.Domain.DTO;

namespace Altafraner.AfraApp.Otium.Domain.DTO;

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
    ///     The time interval of the termin
    /// </summary>
    public required TimeOnlyInterval Uhrzeit { get; set; }

    /// <summary>
    ///     True iff the termin has been cancelled
    /// </summary>
    public bool IstAbgesagt { get; set; }

    /// <summary>
    ///     true, iff the termin is done or currently running
    /// </summary>
    public bool IsDoneOrRunning { get; set; } = false;

    /// <summary>
    ///     true, iff a supervisor may edit attendance for this termin
    /// </summary>
    public required bool IsSupervisionEnabled { get; set; }

    /// <summary>
    ///     The maximum number of people that can be at the termin concurrently. Null if there is no limit
    /// </summary>
    public int? MaxEinschreibungen { get; set; }

    /// <summary>
    ///     The unique identifier of the block this termin belongs to
    /// </summary>
    public required Guid BlockId { get; set; }

    /// <inheritdoc />
    public required char BlockSchemaId { get; set; }

    /// <inheritdoc />
    public required string Block { get; set; }

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

    /// <summary>
    ///     A one time override name for the Otium Termin
    /// </summary>
    public string? Bezeichnung { get; set; }

    /// <summary>
    ///     A one time override description for the Otium Termin
    /// </summary>
    public string? Beschreibung { get; set; }
}

/// <summary>
///     A DTO for an enrollment in a termin as seen by a tutor
/// </summary>
/// <param name="Student">The student enrolled</param>
/// <param name="Anwesenheit">The current verification status</param>
/// <param name="Notizen">Notes left regarding this attendance</param>
public record LehrerEinschreibung(
    PersonInfoMinimal? Student,
    OtiumAnwesenheitsStatus Anwesenheit,
    IEnumerable<Notiz.Notiz> Notizen);

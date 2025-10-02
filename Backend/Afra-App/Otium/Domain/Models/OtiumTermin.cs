using System.ComponentModel.DataAnnotations;
using Altafraner.AfraApp.Backbone.Database.Contracts;
using Altafraner.AfraApp.Schuljahr.Domain.Models;

namespace Altafraner.AfraApp.Otium.Domain.Models;

/// <summary>
///     A record representing a one-time instance of an Otium.
/// </summary>
public class OtiumTermin : OtiumInstanz, IHasTimestamps
{
    /// <summary>
    ///     A unique identifier for the Termin
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    ///     The name of the otium respecting overrides for the Termin
    /// </summary>
    public string Bezeichnung => OverrideBezeichnung != null ? OverrideBezeichnung : Otium.Bezeichnung;

    /// <summary>
    ///     The description of the otium respecting overrides for the Termin
    /// </summary>
    public string Beschreibung => OverrideBeschreibung != null ? OverrideBeschreibung : Otium.Beschreibung;

    /// <summary>
    ///     A name for the Otium Instanz. Null to use the OtiumDefinitions Bezeichnung
    /// </summary>
    [MaxLength(70)]
    public string? OverrideBezeichnung { get; set; }

    /// <summary>
    ///     A description for the Otium Instanz. Null to use the OtiumDefinitions Bezeichnung
    /// </summary>
    [MaxLength(500)]
    public string? OverrideBeschreibung { get; set; }

    /// <summary>
    ///     A reference to the recurrence rule for the Termin. May be null.
    /// </summary>
    public OtiumWiederholung? Wiederholung { get; set; }

    /// <summary>
    ///     A reference to the Block the Termin is on.
    /// </summary>
    public required Block Block { get; set; }

    /// <summary>
    ///     A reference to all enrollments for the Termin.
    /// </summary>
    public ICollection<OtiumEinschreibung> Enrollments { get; set; } = new List<OtiumEinschreibung>();

    /// <summary>
    ///     True, if the Termin is cancelled.
    /// </summary>
    public bool IstAbgesagt { get; set; }

    /// <summary>
    ///     True, iff a supervisor has checked the attendance for this Termin.
    /// </summary>
    public bool SindAnwesenheitenKontrolliert { get; set; } = false;

    /// <inheritdoc/>
    public DateTime CreatedAt { get; set; }

    /// <inheritdoc/>
    public DateTime LastModified { get; set; }
}

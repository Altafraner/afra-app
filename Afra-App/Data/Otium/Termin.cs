using Afra_App.Data.Schuljahr;

namespace Afra_App.Data.Otium;

/// <summary>
/// A record representing a one-time instance of an Otium.
/// </summary>
public class Termin : OtiumsInstanz
{
    /// <summary>
    /// A unique identifier for the Termin
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// A reference to the recurrence rule for the Termin. May be null.
    /// </summary>
    public Wiederholung? Wiederholung { get; set; }

    /// <summary>
    /// A reference to the Schultag the Termin is on.
    /// </summary>
    public required Schultag Schultag { get; set; }

    /// <summary>
    /// A reference to all enrollments for the Termin.
    /// </summary>
    public ICollection<Einschreibung> Enrollments { get; set; } = new List<Einschreibung>();

    /// <summary>
    /// True, if the Termin is cancelled.
    /// </summary>
    public bool IstAbgesagt { get; set; }

    /// <summary>
    /// A maximum number of concurrent enrollments for the Termin. If null, no limit is set.
    /// </summary>
    public int? MaxEinschreibungen { get; set; }
}
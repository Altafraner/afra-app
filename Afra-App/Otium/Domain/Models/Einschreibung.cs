using System.ComponentModel.DataAnnotations;
using Afra_App.Backbone.Domain.TimeInterval;
using Afra_App.User.Domain.Models;

namespace Afra_App.Otium.Domain.Models;

/// <summary>
///     A record representing an enrollment for a <see cref="Otium" />.
/// </summary>
public class Einschreibung
{
    /// <summary>
    ///     The unique identifier of the enrollment.
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    ///     A reference to the <see cref="Termin" /> the enrollment is for.
    /// </summary>
    public required Termin Termin { get; set; }

    /// <summary>
    ///     A reference to the person affected by the enrollment.
    /// </summary>
    public required Person BetroffenePerson { get; set; }

    /// <summary>
    /// The time the enrollment is in. Usually, this is the full duration of the <see cref="Termin"/>.
    /// </summary>
    public required TimeOnlyInterval Interval { get; set; }
}
using Altafraner.AfraApp.User.Domain.Models;

namespace Altafraner.AfraApp.Profundum.Domain.Models;

/// <summary>
///     Free-text supplementary information a student provides before ranking Profunda for an Einwahlzeitraum -
///     e.g. an upcoming exchange abroad or an agreed Lernvertrag - surfaced to staff during matching review.
///     One row per (student, EinwahlZeitraum), always upserted to the latest value the student entered; unlike
///     <see cref="ProfundumBelegWunsch" />/<see cref="ProfundumBelegWunschEntwurf" /> there is no draft/final
///     distinction, since this information isn't validated by the rules engine.
/// </summary>
public class ProfundumZusatzInformation
{
    /// <summary>
    ///     A reference to the person this information is about.
    /// </summary>
    public required Person BetroffenePerson { get; set; }

    /// <summary>
    ///     The primary key of the person this information is about.
    /// </summary>
    /// <remarks>Do not use directly!</remarks>
    protected internal Guid BetroffenePersonId { get; set; }

    /// <summary>
    ///     The enrollment timeframe this information was provided for.
    /// </summary>
    public required ProfundumEinwahlZeitraum EinwahlZeitraum { get; set; }

    /// <summary>
    ///     The primary key of the enrollment timeframe this information was provided for.
    /// </summary>
    /// <remarks>Do not use directly!</remarks>
    protected internal Guid EinwahlZeitraumId { get; set; }

    /// <summary>
    ///     The free-text information itself, as sent by the client. Empty if the student answered "no" to
    ///     everything.
    /// </summary>
    public required string Information { get; set; }
}

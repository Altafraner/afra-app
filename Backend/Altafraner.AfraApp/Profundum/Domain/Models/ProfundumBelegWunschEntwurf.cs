using Altafraner.AfraApp.User.Domain.Models;

namespace Altafraner.AfraApp.Profundum.Domain.Models;

/// <summary>
///     A student's in-progress, unsubmitted ranking of ProfundumDefinitionen for an Einwahlzeitraum - pure scratch
///     space the Einwahl form restores from, kept entirely separate from <see cref="ProfundumBelegWunsch" /> (the
///     actual submission) so that further edits/autosaves after a final submission can never overwrite or
///     downgrade it. A fresh final submission re-syncs this to match, so the two agree again until the student
///     edits further.
/// </summary>
public class ProfundumBelegWunschEntwurf
{
    /// <summary>
    ///     A reference to the person affected by the enrollment.
    /// </summary>
    public required Person BetroffenePerson { get; set; }

    /// <summary>
    ///     The primary key of the person affected by the enrollment.
    /// </summary>
    /// <remarks>Do not use directly!</remarks>
    protected internal Guid BetroffenePersonId { get; set; }

    /// <summary>
    ///     A reference to the profundum (topic) that the BelegWunsch refers to.
    /// </summary>
    public required ProfundumDefinition ProfundumDefinition { get; set; }

    /// <summary>
    ///     The primary key of the profundum that the BelegWunsch refers to.
    /// </summary>
    /// <remarks>Do not use directly!</remarks>
    protected internal Guid ProfundumDefinitionId { get; set; }

    /// <summary>
    ///     The rank (1 = most preferred) the student gave this Profundum among their currently drafted wishes.
    /// </summary>
    public required int Rang { get; set; }

    /// <summary>
    ///     The enrollment timeframe this draft is for.
    /// </summary>
    public required ProfundumEinwahlZeitraum EinwahlZeitraum { get; set; }

    /// <summary>
    ///     The primary key of the enrollment timeframe this draft is for.
    /// </summary>
    /// <remarks>Do not use directly!</remarks>
    protected internal Guid EinwahlZeitraumId { get; set; }
}

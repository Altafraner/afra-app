using Altafraner.AfraApp.User.Domain.Models;

namespace Altafraner.AfraApp.Profundum.Domain.Models;

/// <summary>
///     A record representing a ranked enrollment wish for a <see cref="ProfundumDefinition" />.
/// </summary>
public class ProfundumBelegWunsch
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
    ///     The rank (1 = most preferred) the student gave this Profundum among their submitted wishes.
    ///     Topics not ranked at all are implicitly "not wanted" and have no <see cref="ProfundumBelegWunsch" /> row.
    /// </summary>
    public required int Rang { get; set; }

    /// <summary>
    ///     The enrollment timeframe this wish was submitted in
    /// </summary>
    public required ProfundumEinwahlZeitraum EinwahlZeitraum { get; set; }

    /// <summary>
    ///     The primary key of the enrollment timeframe this wish was submitted in.
    /// </summary>
    /// <remarks>Do not use directly!</remarks>
    protected internal Guid EinwahlZeitraumId { get; set; }

    /// <summary>
    ///     Whether this wish is part of a final submission (<c>true</c>) or an unfinished, unvalidated draft the
    ///     student saved without submitting (<c>false</c>). All rows for one (student, EinwahlZeitraum) share the
    ///     same value, since they are always written as one delete-and-replace batch together.
    /// </summary>
    public bool IstAbgegeben { get; set; }
}

using Altafraner.AfraApp.User.Domain.Models;

namespace Altafraner.AfraApp.Profundum.Domain.Models;

/// <summary>
///     A record representing an enrollment wish for a <see cref="ProfundumDefinition" />.
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
    ///     A reference to the profundum instanz that the BelegWunsch refers to.
    /// </summary>
    public required ProfundumInstanz ProfundumInstanz { get; set; }

    /// <summary>
    ///     The primary key of the profundum instanz that the BelegWunsch refers to.
    /// </summary>
    /// <remarks>Do not use directly!</remarks>
    protected internal Guid ProfundumInstanzId { get; set; }

    /// <summary>
    ///     The level of the BelegWunsch
    /// </summary>
    public required ProfundumBelegWunschStufe Stufe { get; set; }

    /// <summary>
    ///     The enrollment timeframe this wish was submitted in
    /// </summary>
    /// <remarks>
    ///     When a profundum instance spans multiple enrollment timeframes you could submit a wish for a slot of a future
    ///     enrollment timeframe. This property is used to track that.
    /// </remarks>
    public required ProfundumEinwahlZeitraum EinwahlZeitraum { get; set; }
}

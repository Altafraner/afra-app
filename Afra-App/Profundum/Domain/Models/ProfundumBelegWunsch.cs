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
}

using Afra_App.User.Domain.Models;

namespace Afra_App.Profundum.Domain.Models;

/// <summary>
///     A record representing an enrollment wish for a <see cref="Profundum" />.
/// </summary>
public class BelegWunsch
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
    public required BelegWunschStufe Stufe { get; set; }
}

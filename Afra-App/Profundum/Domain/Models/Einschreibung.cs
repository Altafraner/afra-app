using Afra_App.User.Domain.Models;

namespace Afra_App.Profundum.Domain.Models;

/// <summary>
///     A record representing an enrollment for a <see cref="Profundum" />.
/// </summary>
public class Einschreibung
{
    /// <summary>
    ///     A reference to the person affected by the enrollment.
    /// </summary>
    public required Person BetroffenePerson { get; set; }

    ///
    protected internal Guid BetroffenePersonId { get; set; }

    ///
    public required ProfundumInstanz ProfundumInstanz { get; set; }

    ///
    protected internal Guid ProfundumInstanzId { get; set; }
}

using Altafraner.AfraApp.User.Domain.Models;

namespace Altafraner.AfraApp.Profundum.Domain.Models;

/// <summary>
///     A record representing an enrollment for a <see cref="ProfundumDefinition" />.
/// </summary>
public class ProfundumEinschreibung
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

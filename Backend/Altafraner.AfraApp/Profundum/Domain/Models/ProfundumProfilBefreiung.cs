using Altafraner.AfraApp.User.Domain.Models;

namespace Altafraner.AfraApp.Profundum.Domain.Models;

/// <summary>
///     used to express that the matching may ignore the profil-profundum rule for a student in a quartal.
/// </summary>
public class ProfundumProfilBefreiung
{
    /// <summary>
    ///     A reference to the person affected by the enrollment.
    /// </summary>
    public required Person BetroffenePerson { get; set; }

    /// <summary>
    ///     the id of <see cref="BetroffenePerson" />
    /// </summary>
    protected internal Guid BetroffenePersonId { get; set; }

    /// <summary>
    ///     The year the profilprofundum-rule is ignored in
    /// </summary>
    public required int Jahr { get; set; }

    /// <summary>
    ///     The quartal the profilprofundum-rule is ignored in
    /// </summary>
    public required ProfundumQuartal Quartal { get; set; }
}

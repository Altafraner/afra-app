using Altafraner.AfraApp.User.Domain.Models;

namespace Altafraner.AfraApp.Profundum.Domain.Models;

///
public class ProfundumProfilBefreiung
{
    /// <summary>
    ///     A reference to the person affected by the enrollment.
    /// </summary>
    public required Person BetroffenePerson { get; set; }
    ///
    protected internal Guid BetroffenePersonId { get; set; }

    ///
    public required int Jahr { get; set; }
    ///
    public required ProfundumQuartal Quartal { get; set; }
}

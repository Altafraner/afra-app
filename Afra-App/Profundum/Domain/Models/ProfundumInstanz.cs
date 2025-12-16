using Afra_App.User.Domain.Models;

namespace Afra_App.Profundum.Domain.Models;

/// <summary>
///     A db record representing a Profundum instance.
/// </summary>
public class ProfundumInstanz
{
    ///
    public Guid Id { get; set; }

    ///
    public required ProfundumDefinition Profundum { get; set; }

    ///
    public required ICollection<ProfundumSlot> Slots { get; set; }

    ///
    public Person? Tutor { get; set; } = null;

    ///
    public int? MaxEinschreibungen { get; set; } = null;

    ///
    public ICollection<ProfundumEinschreibung> Einschreibungen { get; set; } = new List<ProfundumEinschreibung>();
}

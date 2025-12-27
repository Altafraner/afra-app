using Altafraner.AfraApp.User.Domain.Models;

namespace Altafraner.AfraApp.Profundum.Domain.Models;

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
    public Person? Tutor { get; set; }

    ///
    public int? MaxEinschreibungen { get; set; } = null;

    ///
    public ICollection<ProfundumEinschreibung> Einschreibungen { get; set; } = [];
}

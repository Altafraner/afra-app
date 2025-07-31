namespace Afra_App.Profundum.Domain.Models;

/// <summary>
///     A db record representing a Profundum instance.
/// </summary>
public class ProfundumInstanz
{
    ///
    public Guid Id { get; set; }

    ///
    public required Profundum Profundum { get; set; }

    ///
    public required ICollection<ProfundumSlot> Slots { get; set; }

    ///
    public int? MaxEinschreibungen { get; set; } = null;
}

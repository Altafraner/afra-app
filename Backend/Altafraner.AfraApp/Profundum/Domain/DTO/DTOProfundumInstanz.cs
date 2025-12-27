using Altafraner.AfraApp.Profundum.Domain.Models;

namespace Altafraner.AfraApp.Profundum.Domain.DTO;

///
public record DtoProfundumInstanz
{
    /// <inheritdoc cref="ProfundumInstanz.Id"/>
    public required Guid? Id { get; set; }

    /// <inheritdoc cref="ProfundumInstanz.Profundum"/>
    public required Guid ProfundumId { get; set; }

    /// <inheritdoc cref="ProfundumInstanz.Tutor" />
    public required Guid TutorId { get; set; }

    /// <inheritdoc cref="ProfundumInstanz.Slots"/>
    public required ICollection<Guid> Slots { get; set; }

    /// <inheritdoc cref="ProfundumInstanz.MaxEinschreibungen"/>
    public required int? MaxEinschreibungen { get; set; } = null;
}

namespace Afra_App.Profundum.Domain.DTO;

using Afra_App.Profundum.Domain.Models;

///
public record DTOProfundumInstanz
{
    /// <inheritdoc cref="ProfundumInstanz.Id"/>
    public Guid? Id { get; set; }

    /// <inheritdoc cref="ProfundumInstanz.Profundum"/>
    public Guid ProfundumId { get; set; }

    /// <inheritdoc cref="ProfundumInstanz.Tutor"/>
    public Guid TutorId { get; set; } 

    /// <inheritdoc cref="ProfundumInstanz.Slots"/>
    public required ICollection<Guid> Slots { get; set; }

    /// <inheritdoc cref="ProfundumInstanz.MaxEinschreibungen"/>
    public int? MaxEinschreibungen { get; set; } = null;
}

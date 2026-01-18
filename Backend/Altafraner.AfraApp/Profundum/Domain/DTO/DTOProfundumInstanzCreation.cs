using Altafraner.AfraApp.Profundum.Domain.Models;

namespace Altafraner.AfraApp.Profundum.Domain.DTO;

/// <summary>
///     A request to change or create a profundum instance
/// </summary>
public record DTOProfundumInstanzCreation
{
    /// <inheritdoc cref="ProfundumInstanz.Profundum"/>
    public Guid ProfundumId { get; set; }

    /// <inheritdoc cref="ProfundumInstanz.Slots"/>
    public required ICollection<Guid> Slots { get; set; }

    /// <inheritdoc cref="ProfundumInstanz.MaxEinschreibungen"/>
    public int? MaxEinschreibungen { get; set; } = null;

    /// <inheritdoc cref="ProfundumInstanz.Ort"/>
    public required string Ort { get; set; }

    /// <inheritdoc cref="ProfundumInstanz.Verantwortliche"/>
    public ICollection<Guid> VerantwortlicheIds { get; set; } = [];
}

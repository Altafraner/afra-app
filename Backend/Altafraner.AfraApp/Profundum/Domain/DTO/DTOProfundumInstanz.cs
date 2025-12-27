using System.Diagnostics.CodeAnalysis;
using Altafraner.AfraApp.Profundum.Domain.Models;

namespace Altafraner.AfraApp.Profundum.Domain.DTO;

///
public record DTOProfundumInstanz
{
    ///
    [SetsRequiredMembers]
    public DTOProfundumInstanz(ProfundumInstanz dbInstanz)
    {
        Id = dbInstanz.Id;
        ProfundumId = dbInstanz.Profundum.Id;
        Slots = dbInstanz.Slots.Select(s => s.Id).ToArray();
        MaxEinschreibungen = dbInstanz.MaxEinschreibungen;
        DependencyIds = dbInstanz.Dependencies.Select(d => d.Id).ToArray();
    }

    /// <inheritdoc cref="ProfundumInstanz.Id"/>
    public Guid? Id { get; set; }

    /// <inheritdoc cref="ProfundumInstanz.Profundum"/>
    public Guid ProfundumId { get; set; }

    /// <inheritdoc cref="ProfundumInstanz.Slots"/>
    public required ICollection<Guid> Slots { get; set; }

    /// <inheritdoc cref="ProfundumInstanz.MaxEinschreibungen"/>
    public int? MaxEinschreibungen { get; set; } = null;

    /// <inheritdoc cref="ProfundumInstanz.Dependencies"/>
    public ICollection<Guid> DependencyIds { get; set; } = [];
}

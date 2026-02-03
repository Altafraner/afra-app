using System.Diagnostics.CodeAnalysis;
using Altafraner.AfraApp.Profundum.Domain.Models;
using Altafraner.AfraApp.User.Domain.DTO;

namespace Altafraner.AfraApp.Profundum.Domain.DTO;

/// <summary>
///     a dto representing a profundum instance
/// </summary>
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
        NumEinschreibungen = dbInstanz
            .Einschreibungen.Select(e => e.BetroffenePersonId)
            .Distinct()
            .Count();
        ProfundumInfo = new DTOProfundumDefinition(dbInstanz.Profundum);
        VerantwortlicheIds = dbInstanz.Verantwortliche.Select(e => e.Id);
        VerantwortlicheInfo = dbInstanz.Verantwortliche.Select(e => new PersonInfoMinimal(e));
        Ort = dbInstanz.Ort;
    }

    /// <inheritdoc cref="ProfundumInstanz.Id"/>
    public Guid Id { get; set; }

    /// <inheritdoc cref="ProfundumInstanz.Profundum"/>
    public Guid ProfundumId { get; set; }

    /// <inheritdoc cref="ProfundumInstanz.Profundum"/>
    public DTOProfundumDefinition ProfundumInfo { get; set; }

    /// <inheritdoc cref="ProfundumInstanz.Slots"/>
    public required ICollection<Guid> Slots { get; set; }

    /// <inheritdoc cref="ProfundumInstanz.MaxEinschreibungen"/>
    public int? MaxEinschreibungen { get; set; } = null;

    /// <inheritdoc cref="ProfundumInstanz.Verantwortliche"/>
    public IEnumerable<Guid> VerantwortlicheIds { get; set; } = [];

    /// <inheritdoc cref="ProfundumInstanz.Verantwortliche"/>
    public IEnumerable<PersonInfoMinimal> VerantwortlicheInfo { get; set; } = [];

    /// <summary>
    /// The number of people enrolled to this instance
    /// </summary>
    public int NumEinschreibungen { get; set; }

    /// <inheritdoc cref="ProfundumInstanz.Ort"/>
    public string Ort { get; set; }
}

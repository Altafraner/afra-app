using Altafraner.AfraApp.Profundum.Domain.Models;

namespace Altafraner.AfraApp.Profundum.Domain.DTO;

/// <summary>
///     A singular entry in a <see cref="BlockKatalog"/>.
///     Not a general DTO for <see cref="ProfundumInstanz"/>
///     because of context dependency of <see cref="AlsoIncludes"/>
/// </summary>
public record BlockOption
{
    /// <summary>
    ///     The label of the <see cref="ProfundumInstanz"/>
    /// </summary>
    public required string Label { get; set; }

    /// <summary>
    ///     The id of the <see cref="ProfundumInstanz"/>
    /// </summary>
    public required Guid Value { get; set; }

    /// <summary>
    ///     Additional Slots that are covered
    /// </summary>
    public string[]? AlsoIncludes { get; set; }
}

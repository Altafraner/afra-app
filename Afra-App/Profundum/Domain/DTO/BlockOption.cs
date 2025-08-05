namespace Afra_App.Profundum.Domain.DTO;

using Afra_App.Profundum.Domain.Models;

/// <summary>
///     A singular entry in a <see cref="BlockKatalog"/>.
///     Not a general DTO for <see cref="ProfundumInstanz"/>
///     because of context dependency of <see cref="alsoIncludes"/>
/// </summary>
public record BlockOption
{
    /// <summary>
    ///     The label of the <see cref="ProfundumInstanz"/>
    /// </summary>
    public required string label { get; set; }

    /// <summary>
    ///     The id of the <see cref="ProfundumInstanz"/>
    /// </summary>
    public required Guid value { get; set; }

    /// <summary>
    ///     Additional Slots that are covered
    /// </summary>
    public string[]? alsoIncludes { get; set; }
}

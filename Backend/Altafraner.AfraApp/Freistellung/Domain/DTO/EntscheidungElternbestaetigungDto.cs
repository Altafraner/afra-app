using System.ComponentModel.DataAnnotations;

namespace Altafraner.AfraApp.Freistellung.Domain.DTO;

/// <summary>
///     The Sekretariat's decision about whether a Elternbestätigung (parental confirmation) is
///     required for a leave request, and if so, whether it is already present.
/// </summary>
public record EntscheidungElternbestaetigungDto
{
    /// <summary>
    ///     Whether a Elternbestätigung is required for this request.
    /// </summary>
    public required bool Erforderlich { get; init; }

    /// <summary>
    ///     Whether the Elternbestätigung is already present. Ignored if <see cref="Erforderlich" />
    ///     is <c>false</c>.
    /// </summary>
    public required bool Vorhanden { get; init; }

    /// <summary>
    ///     A hint for the student about what is still missing. Required when <see cref="Erforderlich" />
    ///     is <c>true</c> and <see cref="Vorhanden" /> is <c>false</c>.
    /// </summary>
    [MaxLength(500)]
    public string? Hinweis { get; init; }
}

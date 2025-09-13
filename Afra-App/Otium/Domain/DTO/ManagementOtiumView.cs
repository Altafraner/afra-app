using Afra_App.User.Domain.DTO;
using Afra_App.Otium.Domain.Models;

namespace Afra_App.Otium.Domain.DTO;

/// <summary>
///     A DTO for the view of an Otium in the management ui
/// </summary>
public record ManagementOtiumView
{
    /// <inheritdoc cref="OtiumDefinition.Id"/>
    public Guid Id { get; set; }

    /// <inheritdoc cref="OtiumDefinition.Bezeichnung"/>
    public required string Bezeichnung { get; set; }

    /// <inheritdoc cref="OtiumDefinition.Beschreibung"/>
    public required string Beschreibung { get; set; }

    /// <inheritdoc cref="OtiumDefinition.Kategorie"/>
    public required Guid Kategorie { get; set; }

    /// <inheritdoc cref="OtiumDefinition.Verantwortliche"/>
    public required IEnumerable<PersonInfoMinimal> Verantwortliche { get; set; }

    /// <inheritdoc cref="OtiumDefinition.Termine"/>
    public IEnumerable<ManagementTerminView>? Termine { get; set; }

    /// <inheritdoc cref="OtiumDefinition.Wiederholungen"/>
    public IEnumerable<ManagementWiederholungView>? Wiederholungen { get; set; }

    /// <inheritdoc cref="OtiumDefinition.MinKlasse"/>
    public int? MinKlasse { get; set; } = null;

    /// <inheritdoc cref="OtiumDefinition.MaxKlasse"/>
    public int? MaxKlasse { get; set; } = null;
}

using System.ComponentModel.DataAnnotations;

namespace Altafraner.AfraApp.Otium.Domain.DTO;

/// <summary>
///     A DTO for sending grade ranges
/// </summary>
public record KlassenLimits : IValidatableObject
{
    ///
    public int? MinKlasse { get; set; } = null;

    ///
    public int? MaxKlasse { get; set; } = null;

    /// <inheritdoc/>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (MinKlasse.HasValue && MaxKlasse.HasValue && MaxKlasse < MinKlasse)
        {
            yield return new ValidationResult(
                "Klassen-Minimum darf nicht größer als Klassen-Maximum sein.",
                new[] { nameof(MaxKlasse), nameof(MinKlasse) }
            );
        }
    }
}

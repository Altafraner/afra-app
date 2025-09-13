using System.ComponentModel.DataAnnotations;
namespace Afra_App.Otium.Domain.DTO;

/// <summary>
///     A DTO for sending grade ranges
/// </summary>
public record KlassenLimits : IValidatableObject
{
    ///
    [Range(7, 12)]
    public int? MinKlasse { get; set; } = null;

    ///
    [Range(7, 12)]
    public int? MaxKlasse { get; set; } = null;

    /// <inheritdoc/>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (MinKlasse.HasValue && MaxKlasse.HasValue && MaxKlasse < MinKlasse)
        {
            yield return new ValidationResult(
                "Klassen-Minimum darf nicht größer als Klassen-Maximum sein.",
                new[] { nameof(MaxKlasse), nameof(MinKlasse) });
        }
    }
}

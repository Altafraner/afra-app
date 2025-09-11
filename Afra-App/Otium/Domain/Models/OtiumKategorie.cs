using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Afra_App.Otium.Domain.Models;

/// <summary>
///     A Kategorie for an Otium.
/// </summary>
/// <example>Studienzeit Mathematik</example>
public class OtiumKategorie
{
    /// <summary>
    ///     A unique identifier for the Kategorie.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    ///     A name for the Kategorie.
    /// </summary>
    /// <example>i. e. "Studienzeit Mathematik"</example>
    [MaxLength(50)]
    public required string Bezeichnung { get; set; }

    /// <summary>
    ///     A html-class applied to an i-tag in the frontend in an order of showing an icon.
    /// </summary>
    [MaxLength(20)]
    public string? Icon { get; set; }

    /// <summary>
    ///     A css-color applied to a box around the <see cref="Icon" /> in the frontend.
    /// </summary>
    [MaxLength(20)]
    public string? CssColor { get; set; }

    /// <summary>
    ///     If set, requires the user to enroll in at least one Otium with this Kategorie per week.
    /// </summary>
    public bool Required { get; set; } = false;

    /// <summary>
    ///     If set, ignores the kategorie enrollment rule for enrollment into otia with this categoriy.
    /// </summary>
    public bool IgnoreEnrollmentRule { get; set; } = false;

    /// <summary>
    ///     A parent Kategorie for this Kategorie. Used to build a tree of Kategories.
    /// </summary>
    public OtiumKategorie? Parent { get; set; }

    /// <summary>
    ///     A helper collection for navigating the tree
    /// </summary>
    public ICollection<OtiumKategorie> Children { get; init; } = new List<OtiumKategorie>();

    /// <summary>
    ///     A helper collection for navigating to all Otia with this Kategorie.
    /// </summary>
    /// <remarks>Does not list Otia with this Kategorie transitively.</remarks>
    [JsonIgnore]
    public ICollection<OtiumDefinition> Otia { get; init; } = new List<OtiumDefinition>();
}

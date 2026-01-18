using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Altafraner.AfraApp.Schuljahr.Domain.Models;

namespace Altafraner.AfraApp.Otium.Domain.Models;

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
    ///     If set, requires the user to enroll in at least one Otium with this Kategorie in weeks of the specified type.
    /// </summary>
    public List<Wochentyp> RequiredIn { get; set; } = null!;

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
    public List<OtiumKategorie> Children { get; init; } = null!;

    /// <summary>
    ///     A helper collection for navigating to all Otia with this Kategorie.
    /// </summary>
    /// <remarks>Does not list Otia with this Kategorie transitively.</remarks>
    [JsonIgnore]
    public List<OtiumDefinition> Otia { get; init; } = null!;
}

using System.Text.Json.Serialization;

namespace Afra_App.User.Domain.Models;

/// <summary>
///     Specifies the role of a person
/// </summary>
public enum Rolle
{
    /// <summary>
    ///     The person is in a tutoring role, most commonly a teacher
    /// </summary>
    [JsonStringEnumMemberName("Tutor")] Tutor,

    /// <summary>
    ///     The person is a student and in the Oberstufe
    /// </summary>
    [JsonStringEnumMemberName("Oberstufe")]
    Oberstufe,

    /// <summary>
    ///     The person is a student and in the Mittelstufe
    /// </summary>
    [JsonStringEnumMemberName("Mittelstufe")]
    Mittelstufe
}
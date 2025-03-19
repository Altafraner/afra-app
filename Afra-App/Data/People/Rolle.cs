using System.Text.Json.Serialization;

namespace Afra_App.Data.People;

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
    ///     The person is a student
    /// </summary>
    [JsonStringEnumMemberName("Student")] Student
}
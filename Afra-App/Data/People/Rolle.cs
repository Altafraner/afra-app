using System.Text.Json.Serialization;

namespace Afra_App.Data.People;

public enum Rolle
{
    [JsonStringEnumMemberName("Tutor")]
    Tutor,
    [JsonStringEnumMemberName("Student")]
    Student
}
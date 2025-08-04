namespace Afra_App.Otium.Domain.Models;

/// <summary>
///     An enum representing the status of a person's attendence
/// </summary>
public enum AnwesenheitsStatus
{
    /// <summary>
    ///     The person has attended
    /// </summary>
    Anwesend,

    /// <summary>
    /// The person has not attended but is excused
    /// </summary>
    Entschuldigt,

    /// <summary>
    /// The person has not attended and has (per our knowledge) no valid excuse
    /// </summary>
    Fehlend
}

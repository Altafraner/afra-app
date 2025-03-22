using Afra_App.Data.People;
using Afra_App.Data.Schuljahr;

namespace Afra_App.Data.Otium;

/// <summary>
/// Represents a DB entry with attendance information
/// </summary>
public class Anwesenheit
{
    /// <summary>
    /// The unique ID of the attendance entry
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// The block the attendance is for
    /// </summary>
    public required Block Block { get; set; }

    /// <summary>
    /// The person who is attending
    /// </summary>
    public required Person Student { get; set; }

    /// <summary>
    /// The status of the attendance
    /// </summary>
    public AnwesenheitsStatus Status { get; set; } = AnwesenheitsStatus.Fehlend;
}
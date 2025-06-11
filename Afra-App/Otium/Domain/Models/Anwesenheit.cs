using Afra_App.Otium.Domain.Models.Schuljahr;
using Afra_App.User.Domain.Models;

namespace Afra_App.Otium.Domain.Models;

/// <summary>
/// Represents a DB entry with attendance information
/// </summary>
public class Anwesenheit
{
    /// <summary>
    /// The block the attendance is for
    /// </summary>
    public Block Block { get; set; } = null!;

    /// <summary>
    /// The PK of the block the attendance is for
    /// </summary>
    protected internal Guid BlockId { get; set; }

    /// <summary>
    /// The person who is attending
    /// </summary>
    public Person Student { get; set; } = null!;

    /// <summary>
    /// The PK of the person who is attending
    /// </summary>
    protected internal Guid StudentId { get; set; }

    /// <summary>
    /// The status of the attendance
    /// </summary>
    public AnwesenheitsStatus Status { get; set; } = AnwesenheitsStatus.Fehlend;
}
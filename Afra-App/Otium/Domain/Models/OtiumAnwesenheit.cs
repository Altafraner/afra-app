using Altafraner.AfraApp.Schuljahr.Domain.Models;
using Altafraner.AfraApp.User.Domain.Models;

namespace Altafraner.AfraApp.Otium.Domain.Models;

/// <summary>
/// Represents a DB entry with attendance information
/// </summary>
public class OtiumAnwesenheit
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
    public OtiumAnwesenheitsStatus Status { get; set; } = OtiumAnwesenheitsStatus.Fehlend;
}

using System.ComponentModel.DataAnnotations;
using Altafraner.AfraApp.User.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Altafraner.AfraApp.Attendance.Domain.Models;

/// <summary>
/// Represents a DB entry with attendance information
/// </summary>
[PrimaryKey(nameof(Scope), nameof(SlotId), nameof(StudentId))]
public class Attendance
{
    /// <summary>
    ///     The scope of the slot
    /// </summary>
    [MaxLength(10)]
    public AttendanceScope Scope { get; set; }

    /// <summary>
    /// The block the attendance is for
    /// </summary>
    public Guid SlotId { get; set; }

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
    public AttendanceState Status { get; set; } = AttendanceState.Fehlend;

    /// <summary>
    ///     The type of attendance
    /// </summary>
    public required AttendanceEntryType EntryType;
}

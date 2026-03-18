using System.ComponentModel.DataAnnotations;
using Altafraner.AfraApp.Attendance.Domain.Models;

namespace Altafraner.AfraApp.Attendance.Domain.Dto.Notiz;

/// <summary>
///     A request to create a new note
/// </summary>
public record NotizCreationRequest
{
    /// <summary>
    ///     The notes content
    /// </summary>
    [MaxLength(500)]
    public required string Content { get; set; }

    /// <summary>
    ///     The scope the slot is in
    /// </summary>
    public AttendanceScope Scope { get; set; }

    /// <summary>
    ///     The slot the note is for
    /// </summary>
    public Guid SlotId { get; set; }

    /// <summary>
    ///     The student the note is for
    /// </summary>
    public Guid StudentId { get; set; }
}

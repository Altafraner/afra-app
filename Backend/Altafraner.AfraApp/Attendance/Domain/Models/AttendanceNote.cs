using System.ComponentModel.DataAnnotations;
using Altafraner.AfraApp.User.Domain.Models;
using Altafraner.Backbone.Utils;
using Microsoft.EntityFrameworkCore;

namespace Altafraner.AfraApp.Attendance.Domain.Models;

/// <summary>
///     A note that can be attached to an enrollment
/// </summary>
[PrimaryKey(nameof(Scope), nameof(SlotId), nameof(StudentId), nameof(AuthorId))]
public class AttendanceNote : IHasTimestamps
{
    /// <summary>
    ///     The content of the note
    /// </summary>
    [MaxLength(500)]
    public required string Content { get; set; }

    /// <summary>
    ///     The scope the slot is part of
    /// </summary>
    public required AttendanceScope Scope { get; set; }

    /// <summary>
    ///     The slot the note is for
    /// </summary>
    public required Guid SlotId { get; set; }

    /// <summary>
    ///     The student this note is for
    /// </summary>
    public Person Student { get; set; } = null!;

    /// <summary>
    ///     The ID of the student this note is for
    /// </summary>
    public Guid StudentId { get; set; }

    /// <summary>
    ///     The notes author
    /// </summary>
    public Person Author { get; set; } = null!;

    /// <summary>
    ///     The unique id of the author
    /// </summary>
    public Guid AuthorId { get; set; }

    /// <inheritdoc />
    public DateTime CreatedAt { get; set; }

    /// <inheritdoc />
    public DateTime LastModified { get; set; }
}

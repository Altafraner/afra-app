using System.ComponentModel.DataAnnotations;
using Altafraner.AfraApp.Schuljahr.Domain.Models;
using Altafraner.AfraApp.User.Domain.Models;
using Altafraner.Backbone.Utils;
using Microsoft.EntityFrameworkCore;

namespace Altafraner.AfraApp.Otium.Domain.Models;

/// <summary>
///     A note that can be attached to an enrollment
/// </summary>
[PrimaryKey(nameof(BlockId), nameof(StudentId), nameof(AuthorId))]
public class OtiumAnwesenheitsNotiz : IHasTimestamps
{
    /// <summary>
    ///     The content of the note
    /// </summary>
    [MaxLength(500)]
    public required string Content { get; set; }

    /// <summary>
    ///     The block this note is for
    /// </summary>
    public Block Block { get; set; } = null!;

    /// <summary>
    ///     The ID of the block this note is for
    /// </summary>
    public Guid BlockId { get; set; }

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

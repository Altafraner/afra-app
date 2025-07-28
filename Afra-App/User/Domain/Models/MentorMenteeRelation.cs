namespace Afra_App.User.Domain.Models;

/// <summary>
/// Represents a relationship between a mentor and a mentee.
/// </summary>
public class MentorMenteeRelation
{
    /// <summary>
    /// The mentor's (tutors) unique identifier.
    /// </summary>
    public Guid MentorId { get; set; }

    /// <summary>
    /// The mentee's (students) unique identifier.
    /// </summary>
    public Guid StudentId { get; set; }
}

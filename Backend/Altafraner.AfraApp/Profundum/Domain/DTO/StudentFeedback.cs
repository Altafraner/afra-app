using Altafraner.AfraApp.User.Domain.DTO;

namespace Altafraner.AfraApp.Profundum.Domain.DTO;

/// <summary>
///     Representation of a mentees feedback
/// </summary>
public record struct MenteeFeedback(
    PersonInfoMinimal Person,
    StudentFeedbackHierarchie Feedback
);

/// <summary>
///     Hierarchical feedback result for a student.
/// </summary>
public record struct StudentFeedbackHierarchie(
    Dictionary<Guid, FeedbackEnrollmentInfo> Enrollments,
    IEnumerable<FeedbackKategorieGroup> Kategorien
);

/// <summary>
///     Information about an enrollment in a slot.
/// </summary>
public record struct FeedbackEnrollmentInfo(
    DTOProfundumSlot Slot,
    string Profundum
);

/// <summary>
///     A group of feedback anchors within a category.
/// </summary>
public record struct FeedbackKategorieGroup(
    Guid Id,
    string Label,
    bool IsFachlich,
    IEnumerable<FeedbackAnkerGroup> Anker
);

/// <summary>
///     A group of ratings for a specific anchor across multiple slots.
/// </summary>
public record struct FeedbackAnkerGroup(
    Guid Id,
    string Label,
    Dictionary<Guid, int> RatingsBySlot
);

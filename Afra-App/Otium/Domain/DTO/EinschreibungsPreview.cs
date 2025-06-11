using Afra_App.Backbone.Domain.TimeInterval;

namespace Afra_App.Otium.Domain.DTO;

/// <summary>
///     A DTO for previewing a time-slot
/// </summary>
/// <param name="Anzahl">the amount of einschreibungen for this sub-slot</param>
/// <param name="KannBearbeiten">whether the user may edit his enrollment status</param>
/// <param name="Grund">the reason the user may not enroll / unenroll</param>
/// <param name="Eingeschrieben">whether the user is enrolled for this sub-slot</param>
/// <param name="Interval">the timeslot for this otium</param>
public record EinschreibungsPreview(
    int Anzahl,
    bool KannBearbeiten,
    string? Grund,
    bool Eingeschrieben,
    TimeOnlyInterval Interval);
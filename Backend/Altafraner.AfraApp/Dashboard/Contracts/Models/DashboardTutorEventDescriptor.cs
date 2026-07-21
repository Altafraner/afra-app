using System.ComponentModel.DataAnnotations;

namespace Altafraner.AfraApp.Dashboard.Contracts.Models;

/// <summary>
///     A descriptor for tutor events
/// </summary>
public class DashboardTutorEventDescriptor
{
    /// <summary>
    ///     A label for the event
    /// </summary>
    public required string Label { get; init; }

    /// <summary>
    ///     The DateTime the event starts on. Also used for sorting
    /// </summary>
    public required DateTime Start { get; init; }

    /// <summary>
    ///     A name for the slot this event is in
    /// </summary>
    public required string SlotLabel { get; init; }

    /// <summary>
    ///     A provider specific payload that may include further details used by consumers
    /// </summary>
    public required object? Payload { get; init; }

    /// <summary>
    ///     The optional occupancy rate for this event.
    /// </summary>
    [Range(0, 1)]
    public required float? Occupancy { get; init; }
}

/// <summary>
///     A descriptor for tutor events by a specific module
/// </summary>
public class ScopedDashboardTutorEventDescriptor : DashboardTutorEventDescriptor
{
    /// <summary>
    ///     The module scope
    /// </summary>
    public required string Scope { get; init; }


    internal static ScopedDashboardTutorEventDescriptor FromDashboardTutorEvent(
        DashboardTutorEventDescriptor descriptor,
        string scope)
    {
        return new ScopedDashboardTutorEventDescriptor
        {
            Scope = scope,
            Label = descriptor.Label,
            Start = descriptor.Start,
            SlotLabel = descriptor.SlotLabel,
            Payload = descriptor.Payload,
            Occupancy = descriptor.Occupancy
        };
    }
}

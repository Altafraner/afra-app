using System.ComponentModel.DataAnnotations;
using Afra_App.User.Domain.Models;

namespace Afra_App.Backbone.Calendar.Domain.Models;

/// <summary>
///     A db record representing a subscription to the calendar for a user
/// </summary>
public class CalendarSubscription
{
    /// <summary>
    ///     The unique identifier of the subscription. Used to authenticate so should be kept secret.
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    ///     A reference to the owner of the calendar subscription
    /// </summary>
    public required Person BetroffenePerson { get; set; }
}

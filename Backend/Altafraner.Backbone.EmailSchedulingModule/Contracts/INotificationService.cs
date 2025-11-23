namespace Altafraner.Backbone.EmailSchedulingModule;

/// <summary>
///     A service for sending notifications to a user
/// </summary>
public interface INotificationService
{
    /// <summary>
    ///     Schedule a notification for delivery in a batch within the specified timeframe
    /// </summary>
    Task ScheduleNotificationAsync(IEmailRecipient recipient, string subject, string body, TimeSpan deadline)
    {
        return ScheduleNotificationAsync(recipient.Id, subject, body, deadline);
    }

    /// <summary>
    ///     Schedule a notification for delivery in a batch within the specified timeframe
    /// </summary>
    Task ScheduleNotificationAsync(Guid recipientId, string subject, string body, TimeSpan deadline);
}

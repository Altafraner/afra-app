namespace Altafraner.Backbone.EmailSchedulingModule.Contracts;

public interface INotificationService
{
    /// <summary>
    ///     Schedule a notification for delivery in a batch within the specified timeframe
    /// </summary>
    public Task ScheduleNotificationAsync(IEmailRecipient recipient, string subject, string body, TimeSpan deadline) =>
        ScheduleNotificationAsync(recipient.Id, subject, body, deadline);

    /// <summary>
    ///     Schedule a notification for delivery in a batch within the specified timeframe
    /// </summary>
    public Task ScheduleNotificationAsync(Guid recipientId, string subject, string body, TimeSpan deadline);
}
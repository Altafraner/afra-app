namespace Altafraner.Backbone.EmailSchedulingModule;

/// <summary>
///     A service that delivers notifications via e-mail.
/// </summary>
public interface IEmailNotificationService
{
    /// <summary>
    ///     Schedule an e-mail notification for delivery in a batch within the specified timeframe.
    /// </summary>
    Task ScheduleNotificationAsync(Guid recipientId, string subject, string body, TimeSpan deadline);
}

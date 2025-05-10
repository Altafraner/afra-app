using Afra_App.Data.People;

namespace Afra_App.Services.Email;

/// <summary>
///     An interface representing a batching email notification sender
/// </summary>
public interface IBatchingEmailService
{
    /// <summary>
    ///     Schedule a notification for delivery in a batch within the specified timeframe
    /// </summary>
    public Task ScheduleEmailAsync(Person recipient, string subject, string body, TimeSpan deadline);
}
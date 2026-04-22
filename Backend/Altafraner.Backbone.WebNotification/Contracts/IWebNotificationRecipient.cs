namespace Altafraner.Backbone.WebNotifications;

/// <summary>
///     A recipient for a web notification
/// </summary>
public interface IWebNotificationRecipient
{
    /// <summary>
    ///     The recipients unique id
    /// </summary>
    Guid Id { get; }
}

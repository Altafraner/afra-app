namespace Altafraner.Backbone.WebNotifications.Services;

/// <summary>
///     Thrown when a Web Push service returns 404 or 410, indicating the subscription
///     is no longer valid and should be removed from the database.
/// </summary>
internal sealed class PushSubscriptionGoneException : Exception
{
    /// <summary>The subscription endpoint that is no longer valid.</summary>
    public string Endpoint { get; }

    /// <inheritdoc cref="PushSubscriptionGoneException" />
    public PushSubscriptionGoneException(string endpoint)
        : base($"Push subscription at {endpoint} is no longer valid.")
    {
        Endpoint = endpoint;
    }
}

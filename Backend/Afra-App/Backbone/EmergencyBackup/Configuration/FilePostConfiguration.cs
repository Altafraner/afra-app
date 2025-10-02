namespace Altafraner.AfraApp.Backbone.EmergencyBackup.Configuration;

/// <summary>
/// Configuration settings for posting files to an external endpoint for emergency backup purposes.
/// </summary>
public class FilePostConfiguration
{
    /// <summary>
    /// The name of the HttpClient to be used for emergency backup operations.
    /// </summary>
    public const string HttpClientName = "EmergencyBackup";

    /// <summary>
    /// The base URL of the endpoint where HTML content will be posted for emergency backup.
    /// </summary>
    public required string BaseUri { get; set; }

    /// <summary>
    /// The username for basic authentication when posting to the endpoint. Optional.
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// The password for basic authentication when posting to the endpoint. Optional.
    /// </summary>
    public string? Password { get; set; }
}

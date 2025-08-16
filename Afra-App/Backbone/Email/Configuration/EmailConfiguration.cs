using System.ComponentModel.DataAnnotations;
using MailKit.Security;

namespace Afra_App.Backbone.Email.Configuration;

/// <summary>
///     A configuration object for smtp settings
/// </summary>
public class EmailConfiguration
{
    /// <summary>
    /// The SMTP server host
    /// </summary>
    public required string Host { get; set; }

    /// <summary>
    /// The SMTP server port
    /// </summary>
    public required ushort Port { get; set; }

    /// <summary>
    /// The username to use for authenticating against the SMTP server. If this is null, no authentication will be used.
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// The password to use for authenticating against the SMTP server. See <see cref="Username"/>.
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    /// The SSL/TLS settings to use for the connection. See <see cref="SecureSocketOptions"/>.
    /// </summary>
    public required SecureSocketOptions SecureSocketOptions { get; set; }

    /// <summary>
    /// The email address to use as the sender. This is used for the "From" field in the email.
    /// </summary>
    [EmailAddress]
    public required string SenderEmail { get; set; }

    /// <summary>
    /// The name to use as the sender. This is used for the "From" field in the email.
    /// </summary>
    public required string SenderName { get; set; }

    /// <summary>
    /// Validates the configuration object. Should be called before using the configuration.
    /// </summary>
    /// <returns>True, iff the configuration is valid.</returns>
    public static bool Validate(EmailConfiguration config)
    {
        if (config.Username is not null && config.Password is null) return false;
        if (config.Username is null && config.Password is not null) return false;
        return true;
    }
}

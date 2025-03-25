using System.ComponentModel.DataAnnotations;
using MailKit.Security;

namespace Afra_App.Data.Configuration;

/// <summary>
///     A configuration object for smtp settings
/// </summary>
public class EmailConfiguration
{
    public required string Host { get; set; }

    public required ushort Port { get; set; }

    public string? Username { get; set; }

    public string? Password { get; set; }

    public required SecureSocketOptions SecureSocketOptions { get; set; }

    [EmailAddress]
    public required string SenderEmail { get; set; }

    public required string SenderName { get; set; }

    public static bool Validate(EmailConfiguration config)
    {
        if (config.Username is not null && config.Password is null) return false;
        if (config.Username is null && config.Password is not null) return false;
        return true;
    }
}
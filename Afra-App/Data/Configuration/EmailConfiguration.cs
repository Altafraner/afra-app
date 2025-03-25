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
    public required string Username { get; set; }
    public required string Password { get; set; }
    public required SecureSocketOptions SecureSocketOptions { get; set; }
    [EmailAddress]
    public required string SenderEmail { get; set; }
    public required string SenderName { get; set; }

}
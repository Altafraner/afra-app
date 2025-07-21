using System.DirectoryServices.Protocols;
using Afra_App.User.Services.LDAP;

namespace Afra_App.User.Configuration.LDAP;

/// <summary>
/// A configuration object containing the LDAP configuration
/// </summary>
public class LdapConfiguration
{
    /// <summary>
    /// Whether the LDAP authentication is enabled
    /// </summary>
    public required bool Enabled { get; set; }

    /// <summary>
    /// Whether to validate the certificate of the LDAP server
    /// </summary>
    public bool ValidateCertificate { get; set; }

    /// <summary>
    /// The host of the LDAP server
    /// </summary>
    public required string Host { get; set; }

    /// <summary>
    /// The port of the LDAP server
    /// </summary>
    public required int Port { get; set; }

    /// <summary>
    /// The username of a bind user with permissions to search the LDAP
    /// </summary>
    public required string Username { get; set; }

    /// <summary>
    /// The password of the bind user
    /// </summary>
    public required string Password { get; set; }

    /// <summary>
    /// The base group to search in when users are trying to authenticate
    /// </summary>
    public required LdapSearchDescription GlobalScope { get; set; }

    /// <summary>
    /// Descriptions of all user groups that should be synced from LDAP to the application.
    /// </summary>
    public required LdapUserSyncDescription[] UserGroups { get; set; }

    /// <summary>
    /// Descriptions of all permission groups that should be synced from LDAP to the application.
    /// </summary>
    public required LdapPermissionSyncDescription[] PermissionGroups { get; set; }

    /// <summary>
    /// A collection of e-mail addresses that should be notified when special events occur, such as a user being removed.
    /// </summary>
    public required string[] NotificationEmails { get; set; }

    internal static bool Validate(LdapConfiguration configuration)
    {
        if (!configuration.Enabled) return true;
        try
        {
            var connection = configuration.BuildConnection();
            connection.Bind();
        }
        catch (LdapException)
        {
            Console.WriteLine("Cannot connect to LDAP server.");
            return false;
        }

        return true;
    }
}

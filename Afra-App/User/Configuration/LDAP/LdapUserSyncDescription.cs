using Afra_App.User.Domain.Models;

namespace Afra_App.User.Configuration.LDAP;

/// <summary>
/// Describes the parameters for syncing a collection of users from LDAP to user entries.
/// </summary>
public class LdapUserSyncDescription : LdapSearchDescription
{
    /// <summary>
    /// The role that should be assigned to the users that are synced from this group.
    /// </summary>
    public required Rolle Rolle { get; set; }

    /// <summary>
    /// The group that the users should be assigned to in the application.
    /// </summary>
    public string Group { get; set; } = string.Empty;
}

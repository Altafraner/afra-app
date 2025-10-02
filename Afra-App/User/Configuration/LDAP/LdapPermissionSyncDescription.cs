using Altafraner.AfraApp.User.Domain.Models;

namespace Altafraner.AfraApp.User.Configuration.LDAP;

/// <summary>
/// Describes the parameters for granting permissions to users from LDAP.
/// </summary>
public class LdapPermissionSyncDescription : LdapSearchDescription
{
    /// <summary>
    /// The permission to be granted to the users found by the LDAP search.
    /// </summary>
    public GlobalPermission Permission { get; set; }

    /// <summary>
    /// The email-addresses of users that should be manually assigned this permission.
    /// </summary>
    public string[] ManuallyAssignedUsers { get; set; } = [];
}

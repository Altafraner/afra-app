namespace Afra_App.User.Configuration.LDAP;

/// <summary>
/// Describes the parameters for syncing a collection of users from LDAP.
/// </summary>
public class LdapSearchDescription
{
    /// <summary>
    /// The base distinguished name (DN) to search in the LDAP directory.
    /// </summary>
    public required string BaseDn { get; set; }

    /// <summary>
    /// The ldap filter to apply when searching for entries in the LDAP directory.
    /// </summary>
    public string Filter { get; set; } = "(objectClass=*)";
}

using Altafraner.AfraApp.User.Domain.Models;

namespace Altafraner.AfraApp.User.Configuration.LDAP;

/// <summary>
///     Extends <see cref="LdapSearchDescription" /> with a mentor type
/// </summary>
public class MentorSearchDescription : LdapSearchDescription
{
    /// <summary>
    ///     The type of Mentee-Mentor-Relation these groups contain
    /// </summary>
    public MentorType MentorType { get; set; }
}

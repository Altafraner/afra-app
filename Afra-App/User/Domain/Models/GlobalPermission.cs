namespace Afra_App.User.Domain.Models;

/// <summary>
/// A set of global permissions that can be assigned to a user.
/// </summary>
public enum GlobalPermission
{
    /// <summary>
    /// The user is an administrator with full access to all features and settings.
    /// </summary>
    Admin,

    /// <summary>
    /// The user has permission to manage everything related to the otium.
    /// </summary>
    Otiumsverantwortlich,

    /// <summary>
    /// The user has permission to manage everything related to the Profundum.
    /// </summary>
    Profundumsverantwortlich
}

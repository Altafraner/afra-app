namespace Afra_App.Models;

/// <summary>
/// Permissions are irreducible. Therefore, permissions cannot be inherited. To distribute permissions, roles must be used.
/// </summary>
public enum Permission
{
    /// <summary>
    /// Permission to assign roles
    /// </summary>
    RolesAssign,
    
    /// <summary>
    /// Permission to edit roles
    /// </summary>
    RolesEdit,
    
    /// <summary>
    /// Permission to add and edit people
    /// </summary>
    PeopleEdit,
    
    /// <summary>
    /// Permission to edit mentors
    /// </summary>
    MentorEdit,
    
    /// <summary>
    /// Permission to add otia
    /// </summary>
    OtiaAdd,
    
    /// <summary>
    /// Permission to edit all otia
    /// </summary>
    OtiaEdit,
    
    /// <summary>
    /// Permission to edit classes
    /// </summary>
    ClassesAdd
}
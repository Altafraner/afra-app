namespace Afra_App.Authentication;

/// <summary>
/// A static class containing constants for authorization policies.
/// </summary>
public static class AuthorizationPolicies
{
    /// <summary>
    /// Only tutors may access this endpoint.
    /// </summary>
    public const string TutorOnly = "TutorOnly";

    /// <summary>
    /// Only students may access this endpoint.
    /// </summary>
    public const string StudentOnly = "StudentOnly";
}
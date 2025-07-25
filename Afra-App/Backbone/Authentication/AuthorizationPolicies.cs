﻿namespace Afra_App.Backbone.Authentication;

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

    /// <summary>
    /// Only admins may access this endpoint.
    /// </summary>
    public const string AdminOnly = "AdminOnly";

    /// <summary>
    /// Only users with the permission "Otiumsverantwortliche:r" may access this endpoint.
    /// </summary>
    public const string Otiumsverantwortlich = "Otiumsverantwortliche:r";

    /// <summary>
    /// Only users with the permission "Profundumsverantwortliche:r" may access this endpoint.
    /// </summary>
    public const string Profundumserantwortlich = "Profundumsverantwortliche:r";
}

using Microsoft.AspNetCore.Http;

namespace Altafraner.Backbone.CookieAuthentication;

/// <summary>
///     Settings for handling cookie authentication
/// </summary>
public class CookieAuthenticationSettings
{
    /// <summary>
    ///     Specified whether 3rd party cookies are allowed
    /// </summary>
    public SameSiteMode SameSiteMode { get; set; } = SameSiteMode.Strict;

    /// <summary>
    ///     Specifies whether cookies mus be send using a secure context
    /// </summary>
    public CookieSecurePolicy SecurePolicy { get; set; } = CookieSecurePolicy.Always;

    /// <summary>
    ///     Specifies the default time after which the authentication cookie is invalidated
    /// </summary>
    public TimeSpan CookieTimeout { get; set; } = TimeSpan.FromHours(1);

    /// <summary>
    ///     If true, the authentication is automatically renewed
    /// </summary>
    public bool SlidingExpiration { get; set; } = true;
}

using Microsoft.AspNetCore.Http;

namespace Altafraner.Backbone.CookieAuthentication;

public class CookieAuthenticationSettings
{
    public SameSiteMode SameSiteMode { get; set; } = SameSiteMode.Strict;
    public CookieSecurePolicy SecurePolicy { get; set; } = CookieSecurePolicy.Always;
    public TimeSpan CookieTimeout { get; set; } = TimeSpan.FromHours(1);
    public bool SlidingExpiration { get; set; } = true;
}

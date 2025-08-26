using Afra_App.Backbone.Email.Configuration;
using Afra_App.Backbone.Email.Services.Contracts;
using Afra_App.Backbone.Email.Services.Implementations;

namespace Afra_App.Backbone.Email.Extensions;

/// <summary>
/// Contains a static extension method to add the Email service to the WebApplicationBuilder.
/// </summary>
internal static class AppBuilderExtension
{
    /// <summary>
    /// Adds the Email service to the WebApplicationBuilder.
    /// </summary>
    internal static void AddEmail(this WebApplicationBuilder builder)
    {
        builder.Services.AddOptions<EmailConfiguration>()
            .Bind(builder.Configuration.GetSection("SMTP"))
            .Validate(EmailConfiguration.Validate)
            .ValidateOnStart();
        builder.Services.AddTransient<IEmailService, SmtpEmailService>();
        builder.Services.AddTransient<IEmailOutbox, EmailOutbox>();
    }
}

using System.Text;
using System.Xml;
using Afra_App.Services;
using Microsoft.AspNetCore.Mvc;

namespace Afra_App.Endpoints;

/// <summary>
///     Extension methods for the SamlService
/// </summary>
/// <remarks>
///     <para>Not fully refactored from Controller. See <see cref="IUrlHelper" /> in <c>/SAML/Metadaa</c></para>
///     <para>Currently not in use because of changed scope.</para>
/// </remarks>
public static class SamlExtensions
{
    /// <summary>
    ///     Registers the SAML Endpoints
    /// </summary>
    public static void MapSaml(this WebApplication app)
    {
        // This will not work. It will not be able to resolve IUrlHelper in the minimal API context.
        app.MapGet("/SAML/Metadata", async (SamlService samlService, IConfiguration config, IUrlHelper url) =>
                Results.Content(
                    await samlService.GenerateMetadata(config, url),
                    "application/xml"))
            .WithName("SamlMetadata");

        app.MapPost("/SAML/POST", SamlResponseHandler);
    }

    private static async Task<IResult> SamlResponseHandler([FromForm(Name = "SAMLResponse")] string response,
        [FromForm(Name = "RelayState")] string? relayState, SamlService samlService, UserService userService,
        ILogger<SamlService> logger, HttpContext httpContext)
    {
        var responseXml = new XmlDocument();
        try
        {
            var xmlString = Encoding.UTF8.GetString(Convert.FromBase64String(response));
            responseXml.LoadXml(xmlString);
        }
        catch (FormatException)
        {
            return Results.BadRequest("The SAML-String was not valid Base64");
        }
        catch (XmlException)
        {
            return Results.BadRequest("The SAML-String was not valid XML");
        }

        var validationResult = samlService.Handle(responseXml);

        if (validationResult.Status == SamlService.SamlValidationStatus.Failed) return Results.Unauthorized();

        var user = validationResult.UserInfo?.FirstOrDefault(i => i.AttributeName == "firstName")?.AttributeValue;

        if (user == null) return Results.Unauthorized();

        logger.LogInformation("Signing in User: {userId}", user);

        await userService.SignInAsync(new Guid(user), httpContext);

        return Results.LocalRedirect(string.IsNullOrWhiteSpace(relayState) || relayState == "undefined"
            ? "/"
            : relayState);
    }
}
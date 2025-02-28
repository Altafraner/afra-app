using System.Text;
using System.Xml;
using Afra_App.Services;
using Microsoft.AspNetCore.Mvc;

namespace Afra_App.Controllers;

[Route("/SAML")]
public class SamlController : ControllerBase
{
    [HttpPost("POST")]
    public async Task<ActionResult> Post(
        [FromForm(Name = "SAMLResponse")] string response,
        [FromForm(Name = "RelayState")] string? relayState, 
        [FromServices] SamlService samlService,
        [FromServices] UserService userService,
        [FromServices] ILogger<SamlController> logger)
    {
        var responseXml = new XmlDocument();
        try
        {
            var xmlString = Encoding.UTF8.GetString(Convert.FromBase64String(response));
            responseXml.LoadXml(xmlString);
        }
        catch (FormatException)
        {
            return BadRequest("The SAML-String was not valid Base64");
        }
        catch (XmlException)
        {
            return BadRequest("The SAML-String was not valid XML");
        }

        var validationResult = samlService.Handle(responseXml);
        
        if (validationResult.Status == SamlService.SamlValidationStatus.Failed)
            return Unauthorized(validationResult.Message);

        var user = validationResult.UserInfo?
            .FirstOrDefault(i => i.AttributeName == "firstName")?.AttributeValue;

        if (user == null)
            return Unauthorized("The SAML Response did not contain a valid user");
        
        logger.LogInformation("Signing in User: {userId}", user);

        await userService.SignInAsync(new Guid(user), HttpContext);

        return LocalRedirect(string.IsNullOrWhiteSpace(relayState) || relayState=="undefined" ? "/" : relayState);
    }

    [HttpGet("Metadata")]
    public async Task<ActionResult<string>> Metadata(
        [FromServices] SamlService samlService,
        [FromServices] IConfiguration config)
    {
        return Content(
            await samlService.GenerateMetadata(config, Url),
            "application/xml");
    }

    
}
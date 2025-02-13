using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Xml;
using Afra_App.Services;
using Microsoft.AspNetCore.Mvc;

namespace Afra_App.Controllers;

[Route("/SAML")]
public class SamlController : ControllerBase
{
    [HttpPost("POST")]
    public ActionResult<SamlService.SamlValidationResponse> Post(
        [FromForm(Name = "SAMLResponse")] string response,
        [FromForm(Name = "RelayState")] string? relayState, 
        [FromServices] SamlService samlService)
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

        return samlService.Handle(responseXml);
    }

    
}
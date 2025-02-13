using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Xml;

namespace Afra_App.Services;

public class SamlService
{
    public enum SamlValidationStatus
    {
        Success,
        Failed
    }

    public record SamlValidationResponse(SamlValidationStatus Status, string? Message = null);

    
    private const string SamlAssertionNamespace = "urn:oasis:names:tc:SAML:2.0:assertion";
    private const string SamlAssertionLocalName = "Assertion";

    private readonly string _samlServiceProviderId;
    private readonly string _samlIdentityProviderId;
    private readonly X509Certificate2 _idPCertificate;
    
    private readonly HashSet<string> _responseIds = [];

    private readonly ILogger<SamlService> _logger;

    public SamlService(ILogger<SamlService> logger, IConfiguration configuration)
    {
        _logger = logger;
        IConfiguration samlConfiguration = configuration.GetSection("Saml");
        _idPCertificate = CertificateHelper.LoadX509Certificate(configuration, "SamlIdentityProvider");
        
        _samlServiceProviderId = samlConfiguration["ServiceProviderId"] ?? "";
        _samlIdentityProviderId = samlConfiguration["IdentityProviderId"] ?? "";
        if (string.IsNullOrWhiteSpace(_samlServiceProviderId) || string.IsNullOrWhiteSpace(_samlIdentityProviderId))
            throw new Exception("The SAML SP and IdP Id's need to be set");
    }

    public SamlValidationResponse Handle(XmlDocument responseXml)
    {
        _logger.LogInformation("Checking Saml Response");
        var signedResponseXml = new SignedXml(responseXml);
        var signatures = responseXml.GetElementsByTagName("Signature")
            .OfType<XmlElement>()
            .ToHashSet();
        
        _logger.LogInformation("Saml Response contains {sigCount} Assertions", signatures.Count);
        
        if (signatures.Count == 0)
            return new SamlValidationResponse(SamlValidationStatus.Failed,"The SAML Request contains no signatures.");

        // Validate Signatures
        List<string> validIds = [];
        foreach (var signature in signatures)
        {
            signedResponseXml.LoadXml(signature);
            
            var signatureValid = signedResponseXml.CheckSignature(_idPCertificate, true);
            _logger.LogInformation("Checked Signature. Valid: {valid}", signatureValid);
            if (!signatureValid)
                return new SamlValidationResponse(SamlValidationStatus.Failed, "Could not validate SAML Signature");

            var references = signedResponseXml.SignedInfo?.References ?? [];
            validIds.AddRange(references
                .Cast<Reference>()
                .Select(reference => reference.Uri?[1..] ?? "")
                );
        }

        // Validate every Assertion is signed
        var samlAssertionList = responseXml.GetElementsByTagName(SamlAssertionLocalName, SamlAssertionNamespace);

        var signedAssertions = validIds.Select(id => signedResponseXml.GetIdElement(responseXml, id))
            .Where(assertion => assertion is
                { NamespaceURI: SamlAssertionNamespace, LocalName: SamlAssertionLocalName });

        var signedAssertionsArray = signedAssertions as XmlElement[] ?? signedAssertions.ToArray();
        
        _logger.LogInformation("{sigAsCount} Signatures where valid", signedAssertionsArray.Length);
        if (samlAssertionList.Count != signedAssertionsArray.Length)
            return new SamlValidationResponse(SamlValidationStatus.Failed ,"Not all Assertions signatures could be verified.");

        if (samlAssertionList.Count > 1)
            return new SamlValidationResponse(SamlValidationStatus.Failed ,"Multiple Assertions are not supported");

        _logger.LogInformation("Validating Assertions");
        var assertionValid = ValidateAssertion(signedAssertionsArray.First());
        if (!assertionValid)
            return new SamlValidationResponse(SamlValidationStatus.Failed, "Validation Failed");

        // TODO: Retrieve User Info
        return new SamlValidationResponse(SamlValidationStatus.Success);
    }
    
    private bool ValidateAssertion(XmlElement? assertion)
    {
        ArgumentNullException.ThrowIfNull(assertion);

        // Find Conditions
        var samlConditionsElement = assertion.GetElementsByTagName("Conditions", SamlAssertionNamespace).OfType<XmlElement>().First();
        
        _logger.LogInformation("Checking Saml Conditions");
        // Check Time Window
        if (!samlConditionsElement.HasAttribute("NotBefore") || !samlConditionsElement.HasAttribute("NotOnOrAfter"))
            return false;

        var currentTime = DateTime.Now;
        var notBefore = Convert.ToDateTime(samlConditionsElement.GetAttribute("NotBefore"));
        var notOnOrAfter = Convert.ToDateTime(samlConditionsElement.GetAttribute("NotOnOrAfter"));

        if (currentTime < notBefore || currentTime >= notOnOrAfter)
            return false;
        
        _logger.LogInformation("Check Audience");
        // Check Audience
        var samlAudienceRestriction =
            samlConditionsElement.GetElementsByTagName("AudienceRestriction", SamlAssertionNamespace).OfType<XmlElement>().FirstOrDefault();

        if (samlAudienceRestriction is null)
            return false;

        var samlAudience = samlAudienceRestriction.GetElementsByTagName("Audience", SamlAssertionNamespace).OfType<XmlElement>().FirstOrDefault();
        if (samlAudience is null || samlAudience.InnerText != _samlServiceProviderId) return false;
        
        _logger.LogInformation("Check Issuer");
        // Check Issuer
        var samlIssuer = assertion.GetElementsByTagName("Issuer", SamlAssertionNamespace).OfType<XmlElement>()
            .FirstOrDefault()?.InnerText;

        if (samlIssuer != _samlIdentityProviderId)
            return false;
        
        _logger.LogInformation("Check Uniqueness");
        // Validate Uniqueness
        var assertionId = assertion.GetAttribute("ID");
        if (!_responseIds.Add(assertionId))
            return false;

        return true;
    }
}
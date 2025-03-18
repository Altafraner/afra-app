using System.Collections.Concurrent;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Afra_App.Authentication.SamlMetadata;
using Microsoft.AspNetCore.Mvc;
using KeyInfo = Afra_App.Authentication.SamlMetadata.KeyInfo;

namespace Afra_App.Services;

/// <summary>
/// A service for handling all SAML SP related tasks.
/// </summary>
public class SamlService
{
    /// <summary>
    /// The status of the validation process for a SAML Response.
    /// </summary>
    public enum SamlValidationStatus
    {
        /// <summary>
        /// The validation was successful.
        /// </summary>
        Success,
        /// <summary>
        /// The validation failed.
        /// </summary>
        Failed
    }

    private const string SamlAssertionNamespace = "urn:oasis:names:tc:SAML:2.0:assertion";
    private const string SamlAssertionLocalName = "Assertion";
    private readonly X509Certificate2 _idPCertificate;

    private readonly ILogger<SamlService> _logger;

    // This is a weird .NET thing as there is no ConcurrentHashSet.
    private readonly ConcurrentDictionary<string, DateTime> _responseIds = [];
    private readonly string _samlIdentityProviderId;

    private readonly string _samlServiceProviderId;

    /// <summary>
    /// Constructs a new <see cref="SamlService"/>.
    /// </summary>
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

    /// <summary>
    /// Handles a SAML Response.
    /// </summary>
    /// <param name="responseXml">The XML Representation of the SAML Response</param>
    public SamlValidationResponse Handle(XmlDocument responseXml)
    {
        var signedResponseXml = new SignedXml(responseXml);
        var signatures = responseXml.GetElementsByTagName("Signature")
            .OfType<XmlElement>()
            .ToHashSet();


        if (signatures.Count == 0)
        {
            _logger.LogWarning("The SAML Request contains no signatures.");
            return new SamlValidationResponse(SamlValidationStatus.Failed, "The SAML Request contains no signatures.");
        }

        // Validate Signatures
        List<string> validIds = [];
        foreach (var signature in signatures)
        {
            signedResponseXml.LoadXml(signature);

            var signatureValid = signedResponseXml.CheckSignature(_idPCertificate, true);
            if (!signatureValid)
            {
                _logger.LogWarning("SAML Request contains a signature that is not valid: {signature}",
                    signature.OuterXml);
                return new SamlValidationResponse(SamlValidationStatus.Failed, "Could not validate SAML Signature");
            }

            var references = signedResponseXml.SignedInfo?.References ?? [];
            validIds.AddRange(references
                .Cast<Reference>()
                .Select(reference => reference.Uri?[1..] ?? "")
            );
        }

        // Validate every Assertion is signed
        var samlAssertionList = responseXml.GetElementsByTagName(SamlAssertionLocalName, SamlAssertionNamespace);

        var signedAssertions = validIds
            .Select(id => signedResponseXml.GetIdElement(responseXml, id))
            .OfType<XmlElement>()
            .Where(assertion => assertion is
            { NamespaceURI: SamlAssertionNamespace, LocalName: SamlAssertionLocalName });

        var signedAssertionsArray = signedAssertions as XmlElement[] ?? signedAssertions.ToArray();

        if (samlAssertionList.Count != signedAssertionsArray.Length)
        {
            _logger.LogWarning("Not all Assertions signatures could be verified: {reponseXml}", responseXml.OuterXml);
            return new SamlValidationResponse(SamlValidationStatus.Failed,
                "Not all Assertions signatures could be verified.");
        }

        if (samlAssertionList.Count > 1)
        {
            _logger.LogWarning("Multiple Assertions found: {reponseXml}", responseXml.OuterXml);
            return new SamlValidationResponse(SamlValidationStatus.Failed, "Multiple Assertions are not supported");
        }

        var assertionValid = ValidateAssertion(signedAssertionsArray.First());
        if (!assertionValid)
        {
            _logger.LogWarning("Assertion Validation Failed: {reponseXml}", responseXml.OuterXml);
            return new SamlValidationResponse(SamlValidationStatus.Failed, "Validation Failed");
        }

        var userInfo = signedAssertionsArray.First()
            .GetElementsByTagName("AttributeStatement", SamlAssertionNamespace)
            .OfType<XmlElement>()
            .First()
            .GetElementsByTagName("Attribute", SamlAssertionNamespace)
            .OfType<XmlElement>()
            .Select(e => new SamlUserAttribute(
                e.GetAttribute("Name"),
                e.GetElementsByTagName("AttributeValue", SamlAssertionNamespace)
                    .OfType<XmlElement>().First().InnerText));

        return new SamlValidationResponse(SamlValidationStatus.Success, UserInfo: userInfo);
    }

    private bool ValidateAssertion(XmlElement? assertion)
    {
        ArgumentNullException.ThrowIfNull(assertion);

        // Find Conditions
        var samlConditionsElement = assertion.GetElementsByTagName("Conditions", SamlAssertionNamespace)
            .OfType<XmlElement>().First();

        // Check Time Window
        if (!samlConditionsElement.HasAttribute("NotBefore") || !samlConditionsElement.HasAttribute("NotOnOrAfter"))
            return false;

        var currentTime = DateTime.Now;
        var notBefore = Convert.ToDateTime(samlConditionsElement.GetAttribute("NotBefore"));
        var notOnOrAfter = Convert.ToDateTime(samlConditionsElement.GetAttribute("NotOnOrAfter"));

        if (currentTime < notBefore || currentTime >= notOnOrAfter)
            return false;

        // Check Audience
        var samlAudienceRestriction =
            samlConditionsElement.GetElementsByTagName("AudienceRestriction", SamlAssertionNamespace)
                .OfType<XmlElement>().FirstOrDefault();

        if (samlAudienceRestriction is null)
        {
            _logger.LogWarning("Received Assertion without AudienceRestriction");
            return false;
        }

        var samlAudience = samlAudienceRestriction.GetElementsByTagName("Audience", SamlAssertionNamespace)
            .OfType<XmlElement>().FirstOrDefault();
        if (samlAudience is null || samlAudience.InnerText != _samlServiceProviderId)
        {
            _logger.LogWarning("Received Assertion with wrong Audience: {audience}", samlAudience?.InnerText);
            return false;
        }

        // Check Issuer
        var samlIssuer = assertion.GetElementsByTagName("Issuer", SamlAssertionNamespace)
            .OfType<XmlElement>()
            .FirstOrDefault()?.InnerText;

        if (samlIssuer != _samlIdentityProviderId)
        {
            _logger.LogWarning("Received Assertion with wrong Issuer: {issuer}", samlIssuer);
            return false;
        }

        // Validate Uniqueness
        var assertionId = assertion.GetAttribute("ID");
        return _responseIds.TryAdd(assertionId, DateTime.Now);
    }

    /// <summary>
    ///     Removes outdated response ids from the cache.
    /// </summary>
    public void CleanUp()
    {
        var threshold = DateTime.Now.AddHours(-3);
        var outdated = _responseIds
            .Where(e => e.Value < threshold)
            .Select(e => e.Key);
        foreach (var id in outdated) _responseIds.TryRemove(id, out _);
    }

    /// <summary>
    /// Generates the SAML Metadata for the Service Provider.
    /// </summary>
    /// <param name="configuration">The current configuration provider</param>
    /// <param name="urlHelper">The UrlHelper of a controller</param>
    /// <returns></returns>
    public async Task<string> GenerateMetadata(IConfiguration configuration, IUrlHelper urlHelper)
    {
        var certificate = CertificateHelper.LoadX509CertificateAndKey(configuration, "SamlServiceProvider");
        // I know, this is very ugly, but that is a problem with the protocol, that requires the already base64 encoded
        //  certificate to be base64 encoded again
        var certificateText = Convert.ToBase64String(Encoding.UTF8.GetBytes(certificate.ExportCertificatePem()));
        var samlConfiguration = configuration.GetSection("Saml");
        var metadata = new EntityDescriptor
        {
            SpSsoDescriptor = new SpSsoDescriptor
            {
                WantAssertionsSigned = true,
                AuthnRequestsSigned = true,
                KeyDescriptor = new KeyDescriptor
                {
                    KeyInfo = new KeyInfo
                    {
                        X509Data = new X509Data
                        {
                            X509Certificate = certificateText
                        }
                    },
                    Use = "signing"
                },
                AssertionConsumerService = new AssertionConsumerService
                {
                    Index = 0,
                    Binding = "urn:oasis:names:tc:SAML:2.0:bindings:HTTP-POST",
                    Location = urlHelper.RouteUrl("SamlController.Post") ?? ""
                },
                Extensions = new Extensions
                {
                    UiInfo = new UiInfo
                    {
                        DisplayName =
                        [
                            new DisplayName
                            {
                                Lang = "de",
                                Text = samlConfiguration["UiInfo:DisplayNameDe"]!
                            },
                            new DisplayName
                            {
                                Lang = "en",
                                Text = samlConfiguration["UiInfo:DisplayNameEn"] ??
                                       samlConfiguration["UiInfo:DisplayNameDe"]!
                            }
                        ],
                        Description =
                        [
                            new Description
                            {
                                Lang = "de",
                                Text = samlConfiguration["UiInfo:DescriptionDe"]!
                            },
                            new Description
                            {
                                Lang = "en",
                                Text = samlConfiguration["UiInfo:DescriptionEn"] ??
                                       samlConfiguration["UiInfo:DescriptionDe"]!
                            }
                        ],
                        Logo = new Logo
                        {
                            Height = 100,
                            Width = 100,
                            Text = samlConfiguration["UiInfo:LogoUrl"]!
                        }
                    }
                },
                ProtocolSupportEnumeration = "urn:oasis:names:tc:SAML:2.0:protocol"
            },
            Organization = new Organization
            {
                OrganizationName = new OrganizationName
                {
                    Lang = "de",
                    Text = samlConfiguration["Organization:Name"]!
                },
                OrganizationDisplayName = new OrganizationDisplayName
                {
                    Lang = "de",
                    Text = samlConfiguration["Organization:DisplayName"] ?? samlConfiguration["Organization:Name"]!
                },
                OrganizationUrl = new OrganizationUrl
                {
                    Lang = "de",
                    Text = samlConfiguration["Organization:Url"]!
                }
            },
            ContactPerson =
            [
                new ContactPerson
                {
                    GivenName = samlConfiguration["TechnicalContact:GivenName"]!,
                    SurName = samlConfiguration["TechnicalContact:SurName"]!,
                    EmailAddress = samlConfiguration["TechnicalContact:Email"]!,
                    ContactType = "technical"
                },
                new ContactPerson
                {
                    GivenName = samlConfiguration["AdministrativeContact:GivenName"]!,
                    SurName = samlConfiguration["AdministrativeContact:SurName"]!,
                    EmailAddress = samlConfiguration["AdministrativeContact:Email"]!,
                    ContactType = "administrative"
                }
            ],
            EntityId = samlConfiguration["ServiceProviderId"]!
        };

        var namespaces = new XmlSerializerNamespaces();
        namespaces.Add("md", "urn:oasis:names:tc:SAML:2.0:metadata");
        namespaces.Add("mdui", "urn:oasis:names:tc:SAML:metadata:ui");
        namespaces.Add("ds", "http://www.w3.org/2000/09/xmldsig#");
        var doc = new XmlDocument();
        var navigator = doc.CreateNavigator()!;
        var xmlWriter = navigator.AppendChild();
        var serializer = new XmlSerializer(typeof(EntityDescriptor));
        serializer.Serialize(xmlWriter, metadata, namespaces);
        xmlWriter.Flush();
        xmlWriter.Close();

        // Sign doc with certificate
        var signedXml = new SignedXml(doc)
        {
            SigningKey = certificate.GetRSAPrivateKey()
        };

        var reference = new Reference
        {
            Uri = ""
        };
        reference.AddTransform(new XmlDsigEnvelopedSignatureTransform());
        signedXml.AddReference(reference);
        signedXml.ComputeSignature();

        var xmlDigitalSignature = signedXml.GetXml();
        doc.DocumentElement!.AppendChild(doc.ImportNode(xmlDigitalSignature, true));

        var stringWriter = new StringWriter();

        doc.InsertBefore(doc.CreateXmlDeclaration("1.0", "utf-8", null), doc.DocumentElement);
        doc.Save(stringWriter);
        await stringWriter.FlushAsync();
        return stringWriter.ToString();
    }

    /// <summary>
    /// A record containing information about the validation of a SAML Response.
    /// </summary>
    /// <param name="Status"><see cref="SamlValidationStatus.Success"/> if the assertions and response where valid</param>
    /// <param name="Message">A message containing a reason for a failed validation</param>
    /// <param name="UserInfo">Information about the validated user</param>
    public record SamlValidationResponse(
        SamlValidationStatus Status,
        string? Message = null,
        IEnumerable<SamlUserAttribute>? UserInfo = null);

    /// <summary>
    /// Represents a user attribute transmitted in a SAML assertion
    /// </summary>
    /// <param name="AttributeName">The attribute name from the Assertion</param>
    /// <param name="AttributeValue">The attribute value from the assertion.</param>
    public record SamlUserAttribute(string AttributeName, string AttributeValue);
}
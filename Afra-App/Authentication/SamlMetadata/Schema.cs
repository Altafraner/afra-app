using System.Xml.Serialization;
// For documentation see the official SAML Metadata specification.
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Afra_App.Authentication.SamlMetadata;

[XmlRoot(ElementName="X509Data", Namespace="http://www.w3.org/2000/09/xmldsig#")]
public class X509Data { 

	[XmlElement(ElementName="X509Certificate", Namespace="http://www.w3.org/2000/09/xmldsig#")] 
	public required string X509Certificate; 
}

[XmlRoot(ElementName="KeyInfo", Namespace="http://www.w3.org/2000/09/xmldsig#")]
public class KeyInfo { 

	[XmlElement(ElementName="X509Data", Namespace="http://www.w3.org/2000/09/xmldsig#")] 
	public required X509Data X509Data; 

}

[XmlRoot(ElementName="KeyDescriptor", Namespace="urn:oasis:names:tc:SAML:2.0:metadata")]
public class KeyDescriptor { 

	[XmlElement(ElementName="KeyInfo", Namespace="http://www.w3.org/2000/09/xmldsig#")] 
	public required KeyInfo KeyInfo; 

	[XmlAttribute(AttributeName="use", Namespace="")] 
	public required string Use; 
}

[XmlRoot(ElementName="AssertionConsumerService", Namespace="urn:oasis:names:tc:SAML:2.0:metadata")]
public class AssertionConsumerService { 

	[XmlAttribute(AttributeName="index", Namespace="")] 
	public int Index; 

	[XmlAttribute(AttributeName="Binding", Namespace="")] 
	public required string Binding; 

	[XmlAttribute(AttributeName="Location", Namespace="")] 
	public required string Location; 
}

[XmlRoot(ElementName="DisplayName", Namespace="urn:oasis:names:tc:SAML:metadata:ui")]
public class DisplayName { 

	[XmlAttribute(AttributeName="lang", Namespace="http://www.w3.org/XML/1998/namespace")] 
	public required string Lang; 

	[XmlText] 
	public required string Text; 
}

[XmlRoot(ElementName="Description", Namespace="urn:oasis:names:tc:SAML:metadata:ui")]
public class Description { 

	[XmlAttribute(AttributeName="lang", Namespace="http://www.w3.org/XML/1998/namespace")] 
	public required string Lang; 

	[XmlText] 
	public required string Text; 
}

[XmlRoot(ElementName="Logo", Namespace="urn:oasis:names:tc:SAML:metadata:ui")]
public class Logo { 

	[XmlAttribute(AttributeName="height", Namespace="")] 
	public int Height; 

	[XmlAttribute(AttributeName="width", Namespace="")] 
	public int Width; 

	[XmlText] 
	public required string Text; 
}

[XmlRoot(ElementName="UIInfo", Namespace="urn:oasis:names:tc:SAML:metadata:ui")]
public class UiInfo { 

	[XmlElement(ElementName="DisplayName", Namespace="urn:oasis:names:tc:SAML:metadata:ui")] 
	public required List<DisplayName> DisplayName; 

	[XmlElement(ElementName="Description", Namespace="urn:oasis:names:tc:SAML:metadata:ui")] 
	public required List<Description> Description; 

	[XmlElement(ElementName="Logo", Namespace="urn:oasis:names:tc:SAML:metadata:ui")] 
	public required Logo Logo; 
}

[XmlRoot(ElementName="Extensions", Namespace="urn:oasis:names:tc:SAML:2.0:metadata")]
public class Extensions { 

	[XmlElement(ElementName="UIInfo", Namespace="urn:oasis:names:tc:SAML:metadata:ui")] 
	public required UiInfo UiInfo; 
}

[XmlRoot(ElementName="SPSSODescriptor", Namespace="urn:oasis:names:tc:SAML:2.0:metadata")]
public class SpSsoDescriptor { 

	[XmlElement(ElementName="KeyDescriptor", Namespace="urn:oasis:names:tc:SAML:2.0:metadata")] 
	public required KeyDescriptor KeyDescriptor; 

	[XmlElement(ElementName="AssertionConsumerService", Namespace="urn:oasis:names:tc:SAML:2.0:metadata")] 
	public required AssertionConsumerService AssertionConsumerService; 

	[XmlElement(ElementName="Extensions", Namespace="urn:oasis:names:tc:SAML:2.0:metadata")] 
	public required Extensions Extensions; 

	[XmlAttribute(AttributeName="AuthnRequestsSigned", Namespace="")] 
	public bool AuthnRequestsSigned; 

	[XmlAttribute(AttributeName="WantAssertionsSigned", Namespace="")] 
	public bool WantAssertionsSigned; 

	[XmlAttribute(AttributeName="protocolSupportEnumeration", Namespace="")] 
	public required string ProtocolSupportEnumeration; 
}

[XmlRoot(ElementName="OrganizationName", Namespace="urn:oasis:names:tc:SAML:2.0:metadata")]
public class OrganizationName { 

	[XmlAttribute(AttributeName="lang", Namespace="http://www.w3.org/XML/1998/namespace")] 
	public required string Lang; 

	[XmlText] 
	public required string Text; 
}

[XmlRoot(ElementName="OrganizationDisplayName", Namespace="urn:oasis:names:tc:SAML:2.0:metadata")]
public class OrganizationDisplayName { 

	[XmlAttribute(AttributeName="lang", Namespace="http://www.w3.org/XML/1998/namespace")] 
	public required string Lang; 

	[XmlText] 
	public required string Text; 
}

[XmlRoot(ElementName="OrganizationURL", Namespace="urn:oasis:names:tc:SAML:2.0:metadata")]
public class OrganizationUrl { 

	[XmlAttribute(AttributeName="lang", Namespace="http://www.w3.org/XML/1998/namespace")] 
	public required string Lang; 

	[XmlText] 
	public required string Text; 
}

[XmlRoot(ElementName="Organization", Namespace="urn:oasis:names:tc:SAML:2.0:metadata")]
public class Organization { 

	[XmlElement(ElementName="OrganizationName", Namespace="urn:oasis:names:tc:SAML:2.0:metadata")] 
	public required OrganizationName OrganizationName; 

	[XmlElement(ElementName="OrganizationDisplayName", Namespace="urn:oasis:names:tc:SAML:2.0:metadata")] 
	public required OrganizationDisplayName OrganizationDisplayName; 

	[XmlElement(ElementName="OrganizationURL", Namespace="urn:oasis:names:tc:SAML:2.0:metadata")] 
	public required OrganizationUrl OrganizationURL; 
}

[XmlRoot(ElementName="ContactPerson", Namespace="urn:oasis:names:tc:SAML:2.0:metadata")]
public class ContactPerson { 

	[XmlElement(ElementName="GivenName", Namespace="urn:oasis:names:tc:SAML:2.0:metadata")] 
	public required string GivenName; 

	[XmlElement(ElementName="SurName", Namespace="urn:oasis:names:tc:SAML:2.0:metadata")] 
	public required string SurName; 

	[XmlElement(ElementName="EmailAddress", Namespace="urn:oasis:names:tc:SAML:2.0:metadata")] 
	public required string EmailAddress; 

	[XmlAttribute(AttributeName="contactType", Namespace="")] 
	public required string ContactType;
}

[XmlRoot(ElementName="EntityDescriptor", Namespace="urn:oasis:names:tc:SAML:2.0:metadata")]
public class EntityDescriptor { 

	[XmlElement(ElementName="SPSSODescriptor", Namespace="urn:oasis:names:tc:SAML:2.0:metadata")] 
	public required SpSsoDescriptor SpSsoDescriptor; 

	[XmlElement(ElementName="Organization", Namespace="urn:oasis:names:tc:SAML:2.0:metadata")] 
	public required Organization Organization; 

	[XmlElement(ElementName="ContactPerson", Namespace="urn:oasis:names:tc:SAML:2.0:metadata")] 
	public required List<ContactPerson> ContactPerson; 

	[XmlAttribute(AttributeName="entityID", Namespace="")] 
	public required string EntityId; 
}


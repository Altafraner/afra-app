# Security Assertion Markup Language
SAML ist ein Protokoll, dass Single Sign On, daher das einmalige anmelden an einer zentralen Stelle (dem **IdentityProvider**) ermöglicht.

Wichtige Begriffe:
- **Identity Provider** (IdP): Zentraler Authentifizierungsdienst
- **Service Provider** (SP): Dezentraler Dienst, der seine Nutzer beim IdP Authentifiziert
- **Assertion**: Ein XML-Element, welches die Identität eines Nutzers bestätigt
- **Assertion Consumer Service**: Endpunkt, dem der IdP die Assertion zur Verarbeitung beim SP zusendet.
- **Metadaten**: Ein XML-Dokument mit dem der Protokollpartner (IdP oder SP) alle relevanten Daten für die Kommunikation mit ihm sammelt.

## Ablauf
Es gibt zwei mögliche Abläufe:
- SP Initiiert
- IdP Initiiert

Wir unterstützen aktuell nur IdP initiiert.

Beim IdP initiierten Ablauf sendet der IdP ohne vorherige Anfrage des SP ein SAML-Protokollelement (`<Response>`) mit einer Assertion an den SP, der dann den Nutzer anmeldet und zu seiner Startseite weiterleitet.

## Assertion / `<Response>`

Beispiel:
```xml
<?xml version="1.0" encoding="UTF-8"?>
<samlp:Response Destination="http://localhost:5043/Api/SAML" ID="_18029eab6c3b63006593"
                IssueInstant="2025-02-05T17:54:16.709Z" Version="2.0" xmlns:samlp="urn:oasis:names:tc:SAML:2.0:protocol"
                xmlns:xs="http://www.w3.org/2001/XMLSchema">
    <saml:Issuer Format="urn:oasis:names:tc:SAML:2.0:nameid-format:entity"
                 xmlns:saml="urn:oasis:names:tc:SAML:2.0:assertion">https://saml.example.com/entityid
    </saml:Issuer>
    <Signature xmlns="http://www.w3.org/2000/09/xmldsig#">
        <SignedInfo>
            <CanonicalizationMethod Algorithm="http://www.w3.org/2001/10/xml-exc-c14n#"/>
            <SignatureMethod Algorithm="http://www.w3.org/2001/04/xmldsig-more#rsa-sha256"/>
            <Reference URI="#_18029eab6c3b63006593">
                <Transforms>
                    <Transform Algorithm="http://www.w3.org/2000/09/xmldsig#enveloped-signature"/>
                    <Transform Algorithm="http://www.w3.org/2001/10/xml-exc-c14n#"/>
                </Transforms>
                <DigestMethod Algorithm="http://www.w3.org/2001/04/xmlenc#sha256"/>
                <DigestValue>O/t9BzzSzdQdRIvDn1qPg//F6CS+hiY6iS97Bu/8qfM=
                </DigestValue>
            </Reference>
        </SignedInfo>
        <SignatureValue>
            Y3hZvPiFtYh2jiAiY+Iohnuu0R1eyTxL50TMEt1DbIGJDfLWyP0Fa++zMaoJzOdMmqh0Xeh3EbVn5SYIkQWwu73XfZxU47WwB1Y65j2vrZSBvQwqlIIyx6EhEaZkcqm8QMwyARiMmOviqRYHeBagdqEP88SJwgJDlEv6AOj6trB22TRIlMHFRdLP5qGgcIJRxEkcEmoqgpCp+rknKGUOhnUb+vVbL95JbC264ywPHy19/Wingh1czQWJT5P5ocyyLf6e8X1wauFRASKMsNFRera+mK+m8Mops7XCswOpZLXD702mpRugQXacgaWF30s1FTJzAnSn+/hjANdsmPnWOw==
        </SignatureValue>
        <KeyInfo>
            <X509Data>
                <X509Certificate>
                    <!-- Langes Zertifikat -->
                </X509Certificate>
            </X509Data>
        </KeyInfo>
    </Signature>
    <samlp:Status xmlns:samlp="urn:oasis:names:tc:SAML:2.0:protocol">
        <samlp:StatusCode Value="urn:oasis:names:tc:SAML:2.0:status:Success"/>
    </samlp:Status>
    <saml:Assertion ID="_adfdbe327da2cc901420" IssueInstant="2025-02-05T17:54:16.709Z"
                    Version="2.0" xmlns:saml="urn:oasis:names:tc:SAML:2.0:assertion"
                    xmlns:xs="http://www.w3.org/2001/XMLSchema">
        <saml:Issuer Format="urn:oasis:names:tc:SAML:2.0:nameid-format:entity"
                     xmlns:saml="urn:oasis:names:tc:SAML:2.0:assertion">https://saml.example.com/entityid
        </saml:Issuer>
        <Signature xmlns="http://www.w3.org/2000/09/xmldsig#">
            <SignedInfo>
                <CanonicalizationMethod Algorithm="http://www.w3.org/2001/10/xml-exc-c14n#"/>
                <SignatureMethod Algorithm="http://www.w3.org/2001/04/xmldsig-more#rsa-sha256"/>
                <Reference URI="#_adfdbe327da2cc901420">
                    <Transforms>
                        <Transform Algorithm="http://www.w3.org/2000/09/xmldsig#enveloped-signature"/>
                        <Transform Algorithm="http://www.w3.org/2001/10/xml-exc-c14n#"/>
                    </Transforms>
                    <DigestMethod Algorithm="http://www.w3.org/2001/04/xmlenc#sha256"/>
                    <DigestValue>ZjHxIkjPSmwxEps6lzN026JnyBKAxnnCDqBHOsIg2ds=</DigestValue>
                </Reference>
            </SignedInfo>
            <SignatureValue>
                <!-- Signaturdaten -->
            </SignatureValue>
            <KeyInfo>
                <X509Data>
                    <X509Certificate>
                        <!-- Langes Zertifikat -->
                    </X509Certificate>
                </X509Data>
            </KeyInfo>
        </Signature>
        <saml:Subject xmlns:saml="urn:oasis:names:tc:SAML:2.0:assertion">
            <saml:NameID Format="urn:oasis:names:tc:SAML:1.1:nameid-format:emailAddress">
                richard@example.com
            </saml:NameID>
            <saml:SubjectConfirmation Method="urn:oasis:names:tc:SAML:2.0:cm:bearer">
                <saml:SubjectConfirmationData NotOnOrAfter="2025-02-05T17:59:16.709Z"
                                              Recipient="http://localhost:5043/Api/SAML"/>
            </saml:SubjectConfirmation>
        </saml:Subject>
        <saml:Conditions NotBefore="2025-02-05T17:49:16.709Z" NotOnOrAfter="2025-02-05T17:59:16.709Z"
                         xmlns:saml="urn:oasis:names:tc:SAML:2.0:assertion">
            <saml:AudienceRestriction>
                <saml:Audience>localhost</saml:Audience>
            </saml:AudienceRestriction>
        </saml:Conditions>
        <saml:AuthnStatement AuthnInstant="2025-02-05T17:54:16.709Z"
                             xmlns:saml="urn:oasis:names:tc:SAML:2.0:assertion">
            <saml:AuthnContext>
                <saml:AuthnContextClassRef>urn:oasis:names:tc:SAML:2.0:ac:classes:PasswordProtectedTransport
                </saml:AuthnContextClassRef>
            </saml:AuthnContext>
        </saml:AuthnStatement>
        <saml:AttributeStatement xmlns:saml="urn:oasis:names:tc:SAML:2.0:assertion">
            <saml:Attribute Name="email"
                            NameFormat="urn:oasis:names:tc:SAML:2.0:attrname-format:unspecified">
                <saml:AttributeValue xmlns:xs="http://www.w3.org/2001/XMLSchema"
                                     xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xsi:type="xs:string">
                    richard@example.com
                </saml:AttributeValue>
            </saml:Attribute>
        </saml:AttributeStatement>
    </saml:Assertion>
</samlp:Response>
```

Das `<Response>` Element enthält Informationen zum Protokollverlauf, eine Signatur und eine `Assertion`.

Es ist Aufgabe des SP's, alle Signaturen zu prüfen und sicherzugehen, dass alle `Assertions` signiert sind. (Siehe XML-Signatur-Wrapping Angriff)

Anschließend müssen die in der Assertion gegebenen Parameter geprüft werden. Insbesondere:
- `NotBefore` und `NotOnOrAfter` werden eingehalten,
- `Issuer` und `Audience` stimmen mit den vorher vereinbarten Werten überein.
- `Id` ist einzigartig (keine vorherigen Assertions mit der Id wurden eingelesen)

Ist jede dieser Bedingungen erfüllt, können die Daten als `<AttributeStatement>` ausgelesen werden.

## Metadaten

Beispiel: 
```xml
<?xml
version="1.0" encoding="utf-16"?>
<md:EntityDescriptor xmlns:mdui="urn:oasis:names:tc:SAML:metadata:ui" xmlns:ds="http://www.w3.org/2000/09/xmldsig#" entityID="https://altafraner.de/afra-app" xmlns:md="urn:oasis:names:tc:SAML:2.0:metadata">
    <md:SPSSODescriptor AuthnRequestsSigned="true" WantAssertionsSigned="true" protocolSupportEnumeration="urn:oasis:names:tc:SAML:2.0:protocol">
        <md:KeyDescriptor use="signing">
            <ds:KeyInfo>
                <ds:X509Data>
                    <ds:X509Certificate><!-- Langes Zertifikat --></ds:X509Certificate>
                </ds:X509Data>
            </ds:KeyInfo>
        </md:KeyDescriptor>
        <md:AssertionConsumerService index="0" Binding="urn:oasis:names:tc:SAML:2.0:bindings:HTTP-POST" Location=""/>
        <md:Extensions>
            <mdui:UIInfo>
                <mdui:DisplayName xml:lang="de">Afra App</mdui:DisplayName>
                <mdui:DisplayName xml:lang="en">Afra App</mdui:DisplayName>
                <mdui:Description xml:lang="de">Einschreibung und Verwaltung des Otium</mdui:Description>
                <mdui:Description xml:lang="en">Enrollment and management of the Otium</mdui:Description>
                <mdui:Logo height="100" width="100"/>
            </mdui:UIInfo>
        </md:Extensions>
    </md:SPSSODescriptor>
    <md:Organization>
        <md:OrganizationName xml:lang="de">Verein der Altafraner e. V.</md:OrganizationName>
        <md:OrganizationDisplayName xml:lang="de">Verein der Altafraner</md:OrganizationDisplayName>
        <md:OrganizationURL xml:lang="de">https://verein-der-altafraner.de</md:OrganizationURL>
    </md:Organization>
    <md:ContactPerson contactType="technical">
        <md:GivenName>Max</md:GivenName>
        <md:SurName>Mustermann</md:SurName>
    </md:ContactPerson>
    <md:ContactPerson contactType="administrative">
        <md:GivenName>Max</md:GivenName>
        <md:SurName>Mustermann</md:SurName>
    </md:ContactPerson>
    <Signature xmlns="http://www.w3.org/2000/09/xmldsig#">
        <SignedInfo>
            <CanonicalizationMethod Algorithm="http://www.w3.org/TR/2001/REC-xml-c14n-20010315"/>
            <SignatureMethod Algorithm="http://www.w3.org/2001/04/xmldsig-more#rsa-sha256"/>
            <Reference URI="">
                <Transforms>
                    <Transform Algorithm="http://www.w3.org/2000/09/xmldsig#enveloped-signature"/>
                </Transforms>
                <DigestMethod Algorithm="http://www.w3.org/2001/04/xmlenc#sha256"/>
                <DigestValue>HLVsJl+k2Nc+/CY+5WSmiLczmOPDnqkrYcRVof4xl/M=</DigestValue>
            </Reference>
        </SignedInfo>
        <SignatureValue><!-- Lange Signatur --></SignatureValue>
    </Signature>
</md:EntityDescriptor>
```

Die SP-Metadaten enthalten alle Informationen, damit der IdP mit dem SP kommunizieren kann. Insbesondere sind enthalten:
- Der Public-Key des SP
- Die Information, dass Assertions signiert werden sollen
- Informationen zur Anzeige in der Oberfläche des IdPs
- Technische- und administrative Kontaktdaten

Optional können (wie hier) die Metadaten digital signiert werden.

Die Metadaten des SP's der Afra-App können unter `/SAML/Metadata` abgerufen werden.
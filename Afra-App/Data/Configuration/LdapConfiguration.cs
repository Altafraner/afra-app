using System.DirectoryServices.Protocols;
using System.Net;

namespace Afra_App.Data.Configuration;

/// <summary>
/// A configuration object containing the LDAP configuration
/// </summary>
public class LdapConfiguration
{
    /// <summary>
    /// The host of the LDAP server
    /// </summary>
    public required string Host { get; set; }

    /// <summary>
    /// The port of the LDAP server
    /// </summary>
    public required int Port { get; set; }

    /// <summary>
    /// The username of a bind user with permissions to search the LDAP
    /// </summary>
    public required string Username { get; set; }

    /// <summary>
    /// The password of the bind user
    /// </summary>
    public required string Password { get; set; }

    /// <summary>
    /// The base DN to search in
    /// </summary>
    public required string BaseDn { get; set; }

    /// <summary>
    /// A group that all students are in
    /// </summary>
    public required string StudentGroup { get; set; }

    /// <summary>
    /// A group that all teachers are in
    /// </summary>
    public required string TutorGroup { get; set; }

    internal static bool Validate(LdapConfiguration configuration)
    {
        var identifier = new LdapDirectoryIdentifier(configuration.Host, configuration.Port);
        var credentials = new NetworkCredential(configuration.Username, configuration.Password);

        using var connection = new LdapConnection(identifier, credentials);
        connection.SessionOptions.ProtocolVersion = 3;
        connection.SessionOptions.SecureSocketLayer = true;
        connection.SessionOptions.VerifyServerCertificate = (_, _) => true;
        connection.AuthType = AuthType.Basic;

        try
        {
            connection.Bind();
        }
        catch (Exception)
        {
            return false;
        }
        /*var request = new SearchRequest("OU=Schule,DC=sankt-afra,DC=de", "(objectClass=user)",
            SearchScope.Subtree);
        var response = (SearchResponse)connection.SendRequest(request);

        Console.WriteLine("Request sent");
        if (response is null)
        {
            return false;
        }

        foreach (var entry in response.Entries.Cast<SearchResultEntry>())
        {
            Console.WriteLine(entry.DistinguishedName);
        }*/

        return true;
    }
}
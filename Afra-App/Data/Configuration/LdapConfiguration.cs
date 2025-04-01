using System.DirectoryServices.Protocols;
using Afra_App.Authentication.Ldap;

namespace Afra_App.Data.Configuration;

/// <summary>
/// A configuration object containing the LDAP configuration
/// </summary>
public class LdapConfiguration
{
    /// <summary>
    /// Whether the LDAP authentication is enabled
    /// </summary>
    public required bool Enabled { get; set; }

    /// <summary>
    /// Whether to validate the certificate of the LDAP server
    /// </summary>
    public bool ValidateCertificate { get; set; }

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
        if (!configuration.Enabled) return true;
        LdapConnection connection;
        try
        {
            connection = LdapHelper.BuildConnection(configuration);
        }
        catch (LdapException)
        {
            Console.WriteLine("Cannot connect to LDAP server.");
            return false;
        }

        var studentGroupRequest = new SearchRequest(configuration.StudentGroup, "(objectClass=group)",
            SearchScope.Base);
        var studentGroupResponse = (SearchResponse)connection.SendRequest(studentGroupRequest);
        if (studentGroupResponse.Entries.Count == 0)
        {
            Console.WriteLine("Student group does not exist in directory");
            return false;
        }

        var tutorGroupRequest = new SearchRequest(configuration.TutorGroup, "(objectClass=group)",
            SearchScope.Base);
        var tutorGroupResponse = (SearchResponse)connection.SendRequest(tutorGroupRequest);
        if (tutorGroupResponse.Entries.Count == 0)
        {
            Console.WriteLine("Tutor group does not exist in directory");
            return false;
        }

        return true;
    }
}
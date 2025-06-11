using System.DirectoryServices.Protocols;
using System.Net;
using System.Text;
using Afra_App.User.Configuration;

namespace Afra_App.User.Services.LDAP;

/// <summary>
/// A helper class for building LDAP connections
/// </summary>
public static class LdapHelper
{
    /// <summary>
    /// Builds a new LDAP connection from the given configuration
    /// </summary>
    public static LdapConnection BuildConnection(LdapConfiguration configuration, ILogger? logger = null)
    {
        var identifier = new LdapDirectoryIdentifier(configuration.Host, configuration.Port);
        var credentials = new NetworkCredential(configuration.Username, configuration.Password);

        var connection = new LdapConnection(identifier, credentials)
        {
            SessionOptions =
            {
                ProtocolVersion = 3,
                SecureSocketLayer = true,
                ReferralChasing = ReferralChasingOptions.None
            },
            AuthType = AuthType.Basic
        };
        if (!configuration.ValidateCertificate)
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                connection.SessionOptions.VerifyServerCertificate = (_, _) => true;
            }
            else if (Environment.GetEnvironmentVariable("LDAPTLS_REQCERT") != "never")
            {
                void Log(string message)
                {
                    if (logger is not null)
                    {
                        logger.Log(LogLevel.Warning, message);
                        return;
                    }

                    Console.WriteLine(message);
                }

                Log(
                    "LDAP: Certificate validation is disabled, but 'LDAPTLS_REQCERT' is not set to 'never'. This will not work outside windows.");
            }
        }

        connection.Bind();

        return connection;
    }

    /// <summary>
    /// Try to get a <see cref="Guid"/> from an LDAP entry
    /// </summary>
    /// <param name="entry">The entry to get the <see cref="Guid"/> from</param>
    /// <param name="objGuid">The objects <see cref="Guid"/>, if exists and valid; Otherwise, <see cref="Guid.Empty">Guid.Empty</see>
    /// </param>
    /// <returns>True, if the entry has a valid Guid; Otherwise, false</returns>
    public static bool TryGetGuidFromEntry(SearchResultEntry entry, out Guid objGuid)
    {
        if (entry.Attributes["objectGuid"]?.GetValues(typeof(byte[])).FirstOrDefault() is not byte[] objGuidBytes)
        {
            objGuid = Guid.Empty;
            return false;
        }

        try
        {
            objGuid = new Guid(objGuidBytes);
            return true;
        }
        catch (ArgumentException)
        {
            objGuid = Guid.Empty;
            return false;
        }
    }

    /// <summary>
    /// Get a single attribute from an LDAP entry
    /// </summary>
    /// <param name="entry">The entry to get the attribute from</param>
    /// <param name="attributeName">If exists and is a string, the value of the first instance of the attribute; Otherwise, null</param>
    /// <returns></returns>
    public static string? GetSingleAttribute(SearchResultEntry entry, string attributeName)
    {
        return entry.Attributes[attributeName]?.GetValues(typeof(string)).FirstOrDefault() as string;
    }

    /// <summary>
    /// Sanitize a string for use in LDAP queries
    /// </summary>
    /// <param name="value">The string to sanizize</param>
    /// <returns>The sanitized string</returns>
    /// <remarks>This is an approx. port of the java implementation for DefaultEncoder.encodeForLDAP(string, true) from the esapi-java-legacy project</remarks>
    public static string Sanitize(string value)
    {
        StringBuilder sb = new(value.Length);
        foreach (var c in value)
            switch (c)
            {
                case '\\':
                    sb.Append("\\5c");
                    break;
                case '/':
                    sb.Append("\\2f");
                    break;
                case '*':
                    sb.Append("\\2a");
                    break;
                case '(':
                    sb.Append("\\28");
                    break;
                case ')':
                    sb.Append("\\29");
                    break;
                case '\0':
                    sb.Append("\\00");
                    break;
                default:
                    if (c >= 0x80)
                    {
                        var bytes = Encoding.UTF8.GetBytes([c]);
                        foreach (var b in bytes) sb.Append($"\\{b:X2}");

                        break;
                    }

                    sb.Append(c);
                    break;
            }

        return sb.ToString();
    }
}

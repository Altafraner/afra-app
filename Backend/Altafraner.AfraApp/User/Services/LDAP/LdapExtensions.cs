using System.DirectoryServices.Protocols;
using System.Net;
using Altafraner.AfraApp.User.Configuration.LDAP;

namespace Altafraner.AfraApp.User.Services.LDAP;

/// <summary>
/// Provides extension methods for working with LDAP connections and entries.
/// </summary>
public static class LdapExtensions
{
    /// <summary>
    /// Builds a new LDAP connection a the given configuration
    /// </summary>
    public static LdapConnection BuildConnection(
        this LdapConfiguration configuration,
        ILogger? logger = null
    )
    {
        var identifier = new LdapDirectoryIdentifier(configuration.Host, configuration.Port);
        var credentials = new NetworkCredential(configuration.Username, configuration.Password);

        var connection = new LdapConnection(identifier, credentials)
        {
            SessionOptions =
            {
                ProtocolVersion = 3,
                SecureSocketLayer = true,
                ReferralChasing = ReferralChasingOptions.None,
            },
            AuthType = AuthType.Basic,
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
                    "LDAP: Certificate validation is disabled, but 'LDAPTLS_REQCERT' is not set to 'never'. This will not work outside windows."
                );
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
    public static bool TryGetGuid(this SearchResultEntry entry, out Guid objGuid)
    {
        if (
            entry.Attributes["objectGuid"]?.GetValues(typeof(byte[])).FirstOrDefault()
            is not byte[] objGuidBytes
        )
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
    public static string? GetSingleAttribute(this SearchResultEntry entry, string attributeName)
    {
        return entry.Attributes[attributeName]?.GetValues(typeof(string)).FirstOrDefault()
            as string;
    }

    /// <summary>
    /// Gets a multivalued attribute from an LDAP entry
    /// </summary>
    /// <param name="entry">The entry to get the attribute from</param>
    /// <param name="attributeName">The values of the attribute</param>
    public static IEnumerable<string> GetMulitAttribute(
        this SearchResultEntry entry,
        string attributeName
    )
    {
        return entry.Attributes[attributeName]?.GetValues(typeof(string)).Cast<string>() ?? [];
    }
}

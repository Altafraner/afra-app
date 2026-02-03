using System.Text;

namespace Altafraner.AfraApp.User.Services.LDAP;

/// <summary>
/// A helper class for building LDAP connections
/// </summary>
public static class LdapSanitizer
{
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
                        foreach (var b in bytes)
                            sb.Append($"\\{b:X2}");

                        break;
                    }

                    sb.Append(c);
                    break;
            }

        return sb.ToString();
    }
}

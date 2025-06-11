using System.Security.Cryptography.X509Certificates;

namespace Afra_App.Backbone.Utilities;

internal static class CertificateHelper
{
    public static X509Certificate2 LoadX509CertificateAndKey(IConfiguration configuration, string name)
    {
        var certPath = configuration[$"Certificates:{name}Cert"];
        var keyPath = configuration[$"Certificates:{name}Key"];

        if (certPath == null)
            throw new KeyNotFoundException($"The certificate with the name {name} was not configured");
        return X509Certificate2.CreateFromPemFile(certPath, keyPath);
    }

    public static X509Certificate2 LoadX509Certificate(IConfiguration configuration, string name)
    {
        var certPath = configuration[$"Certificates:{name}Cert"];

        if (certPath == null)
            throw new KeyNotFoundException($"The certificate with the name {name} was not configured");

        return X509CertificateLoader.LoadCertificateFromFile(certPath);
    }
}
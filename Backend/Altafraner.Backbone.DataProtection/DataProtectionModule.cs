using System.Security.Cryptography;
using Altafraner.Backbone.Abstractions;
using Altafraner.Backbone.Utils;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Altafraner.Backbone.DataProtection;

public class DataProtectionModule<T> : IModule where T : DbContext, IDataProtectionKeyContext
{
    public void ConfigureServices(IServiceCollection services, IConfiguration config, IHostEnvironment env)
    {
        try
        {
            var dataProtectionCert =
                CertificateHelper.LoadX509CertificateAndKey(config, "DataProtection");
            services.AddDataProtection()
                .SetApplicationName(env.ApplicationName)
                .PersistKeysToDbContext<T>()
                .ProtectKeysWithCertificate(dataProtectionCert);
        }
        catch (CryptographicException exception)
        {
            Console.WriteLine($"Could not load certificate for Domain Protection {exception.Message}");
            Environment.Exit(1);
        }
    }

    public void Configure(WebApplication app)
    {
    }
}

using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json.Serialization;
using Afra_App;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers()
    .AddJsonOptions(options => { options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles; });

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddDbContext<AfraAppContext>();
builder.Services.AddOpenApi();
builder.Services.AddCors(options =>
{
    options.DefaultPolicyName = "default";
    options.AddPolicy("default", corsPolicyBuilder => corsPolicyBuilder.AllowAnyMethod().AllowAnyOrigin().AllowAnyHeader());
});

// TODO Add login service
builder.Services.AddAuthentication()
    .AddCookie();
builder.Services.AddAuthorization();

try
{
    var dataProtectionCertPath = builder.Configuration["Security:DataProtectionCertPath"] ?? "cert.pem";
    Console.WriteLine(dataProtectionCertPath);
    var dataProtectionCertKey = builder.Configuration["Security:DataProtectionCertKey"];
    var dataProtectionCert = dataProtectionCertKey is null ? 
        X509Certificate2.CreateFromPemFile(dataProtectionCertPath) : 
        X509Certificate2.CreateFromEncryptedPemFile(dataProtectionCertPath, dataProtectionCertKey);
    builder.Services.AddDataProtection()
        .SetApplicationName("Afra-App")
        .PersistKeysToDbContext<AfraAppContext>()
        .ProtectKeysWithCertificate(dataProtectionCert);
}
catch (CryptographicException exception)
{
    Console.WriteLine($"Could not load Certificate for Data Protection {exception.Message}");
    Environment.Exit(1);
}

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    using var context = scope.ServiceProvider.GetService<AfraAppContext>();
    context?.Database.Migrate();
    
    app.UseCors();
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
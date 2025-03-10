using System.Security.Cryptography;
using System.Text.Json.Serialization;
using Afra_App.Authentication;
using Afra_App.Data;
using Afra_App.Endpoints;
using Afra_App.Services;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add services to the container.

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.Converters.Add(new TimeOnlyJsonConverter());
    });

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddDbContext<AfraAppContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddOpenApi();
builder.Services.AddCors(options =>
{
    options.DefaultPolicyName = "default";
    options.AddPolicy("default",
        corsPolicyBuilder => corsPolicyBuilder.AllowAnyMethod().AllowAnyOrigin().AllowAnyHeader());
});

builder.Services.AddAuthentication()
    .AddCookie(options =>
    {
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        options.LoginPath = "/SAML/LoginRedirect";
        options.AccessDeniedPath = "/AccessDenied";
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    });
builder.Services.AddAuthorization();
if (builder.Configuration.GetValue<bool>("Saml:Enabled")) builder.Services.AddSingleton<SamlService>();
builder.Services.AddScoped<UserService>();

try
{
    var dataProtectionCert = CertificateHelper.LoadX509CertificateAndKey(builder.Configuration, "DataProtection");
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

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    using var context = scope.ServiceProvider.GetService<AfraAppContext>();
    context!.Database.Migrate();

    app.UseCors();
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Afra-App API");
        }
    );
}

app.UseAuthorization();

app.MapControllers();

app.MapUserEndpoints();
if (app.Configuration.GetValue<bool>("Saml:Enabled")) app.MapSaml();

app.Run();

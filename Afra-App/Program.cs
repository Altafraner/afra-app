using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using Afra_App.Authentication;
using Afra_App.Authentication.Ldap;
using Afra_App.Authentication.Saml;
using Afra_App.Data;
using Afra_App.Data.Configuration;
using Afra_App.Data.People;
using Afra_App.Endpoints;
using Afra_App.Endpoints.Otium;
using Afra_App.Services.Email;
using Afra_App.Services.Otium;
using Afra_App.Services.User;
using Afra_App.Utilities;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Quartz;
using Quartz.AspNetCore;

CultureInfo.CurrentCulture = CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfoByIetfLanguageTag("de-DE");

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configuration binding

builder.Services.AddOptions<OtiumConfiguration>()
    .Bind(builder.Configuration.GetSection("Otium"))
    .Validate(OtiumConfiguration.Validate)
    .ValidateOnStart();

builder.Services.AddOptions<EmailConfiguration>()
    .Bind(builder.Configuration.GetSection("SMTP"))
    .Validate(EmailConfiguration.Validate)
    .ValidateOnStart();

builder.Services.AddOptions<LdapConfiguration>()
    .Bind(builder.Configuration.GetSection("LDAP"))
    .Validate(LdapConfiguration.Validate)
    .ValidateOnStart();

// Add services to the container.

builder.Services.ConfigureHttpJsonOptions(options => ConfigureJsonOptions(options.SerializerOptions));
builder.Services.AddSignalR()
    .AddJsonProtocol(options => ConfigureJsonOptions(options.PayloadSerializerOptions));
builder.Services.AddControllers();
builder.Services.AddHybridCache();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddDbContext<AfraAppContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"),
        AfraAppContext.ConfigureNpgsql);
    options.ConfigureWarnings(w => w.Throw(RelationalEventId.MultipleCollectionIncludeWarning));
});
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
        options.Events.OnRedirectToAccessDenied = context =>
        {
            var authenticated = context.HttpContext.User.Identity?.IsAuthenticated ?? false;
            if (authenticated)
            {
                context.Response.Clear();
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                context.Response.WriteAsJsonAsync(new ProblemDetails
                {
                    Title = "Access Denied",
                    Detail = "You are not allowed to access this resource. You do not seem to have the right roles.",
                    Status = StatusCodes.Status403Forbidden,
                    Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.3"
                });
                return Task.CompletedTask;
            }

            context.Response.Redirect(context.RedirectUri);
            return Task.CompletedTask;
        };
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    });

builder.Services.AddAuthorizationBuilder()
    .AddPolicy(AuthorizationPolicies.StudentOnly,
        policy => policy.RequireClaim(AfraAppClaimTypes.Role, nameof(Rolle.Oberstufe), nameof(Rolle.Mittelstufe)))
    .AddPolicy(AuthorizationPolicies.TutorOnly,
        policy => policy.RequireClaim(AfraAppClaimTypes.Role, nameof(Rolle.Tutor)));

if (builder.Configuration.GetValue<bool>("Saml:Enabled")) builder.Services.AddSingleton<SamlService>();
builder.Services.AddScoped<UserSigninService>();
builder.Services.AddScoped<UserAccessor>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<BlockHelper>();
builder.Services.AddScoped<KategorieService>();
builder.Services.AddScoped<OtiumEndpointService>();
builder.Services.AddScoped<EnrollmentService>();
builder.Services.AddScoped<IAttendanceService, AttendanceService>();
builder.Services.AddScoped<LdapService>();

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
    Console.WriteLine($"Could not load certificate for Data Protection {exception.Message}");
    Environment.Exit(1);
}

builder.Services.AddQuartz(q =>
    {
        q.UsePersistentStore(storeOptions =>
            {
                var conString = builder.Configuration.GetConnectionString("DefaultConnection")!;
                storeOptions.UsePostgres(pgOptions =>
                    pgOptions.ConnectionString = conString
                );
                storeOptions.UseSystemTextJsonSerializer();
            }
        );
    }
);
builder.Services.AddQuartzServer(options => { options.WaitForJobsToComplete = true; });
builder.Services.AddHostedService<LdapAutoSyncScheduler>();
builder.Services.AddHostedService<EnrollmentReminderService>();

builder.Services.AddTransient<IEmailService, SmtpEmailService>();
builder.Services.AddTransient<IBatchingEmailService, BatchingEmailService>();

var app = builder.Build();

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

using (var scope = app.Services.CreateScope())
{
    using var context = scope.ServiceProvider.GetService<AfraAppContext>()!;

    if (context.Database.GetPendingMigrations().Any())
    {
        if (builder.Configuration.GetValue<bool>("MigrateOnStartup"))
        {
            app.Logger.LogInformation("Migrating database");
            context.Database.Migrate();
            app.Logger.LogInformation("Database migrated");
        }
        else
        {
            throw new ValidationException("The database is not up to date. Please run the migrations.");
        }
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseCors();
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "Afra-App API"); }
    );
}

app.UseAuthorization();

app.MapOtium();
app.MapSchuljahr();
app.MapUserEndpoints();
app.MapPeopleEndpoints();
if (app.Configuration.GetValue<bool>("Saml:Enabled")) app.MapSaml();

if (app.Environment.IsDevelopment()) app.MapControllers();

app.Run();
return;

void ConfigureJsonOptions(JsonSerializerOptions options)
{
    options.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    options.Converters.Add(new JsonStringEnumConverter());
    options.Converters.Add(new TimeOnlyJsonConverter());
}

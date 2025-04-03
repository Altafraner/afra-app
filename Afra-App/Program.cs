using System.Security.Cryptography;
using System.Text.Json.Serialization;
using Afra_App.Data;
using Afra_App.Data.Configuration;
using Afra_App.Endpoints;
using Afra_App.Services;
using Afra_App.Services.Otium;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Quartz;
using Quartz.AspNetCore;

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

// Add services to the container.

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
    options.SerializerOptions.Converters.Add(new TimeOnlyJsonConverter());
});
builder.Services.AddControllers();

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
        options.LoginPath = "/SAML/LoginRedirect";
        options.AccessDeniedPath = "/AccessDenied";
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    });
builder.Services.AddAuthorization();
if (builder.Configuration.GetValue<bool>("Saml:Enabled")) builder.Services.AddSingleton<SamlService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<KategorieService>();
builder.Services.AddScoped<OtiumEndpointService>();
builder.Services.AddScoped<EnrollmentService>();

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
        q.UsePersistentStore(x =>
            {
                var conString = builder.Configuration.GetConnectionString("DefaultConnection")!;
                x.UsePostgres(x =>
                    x.ConnectionString = conString
                );
                x.UseSystemTextJsonSerializer();
            }
        );
    }
);
builder.Services.AddQuartzServer(options => { options.WaitForJobsToComplete = true; });


builder.Services.AddTransient<IEmailService, EmailService>();
builder.Services.AddTransient<IBatchingEmailService, BatchingEmailService>();

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
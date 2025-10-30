using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Afra_App.Backbone.Extensions;
using Afra_App.Backbone.Utilities;
using Afra_App.Calendar.Extensions;
using Afra_App.Otium.Extensions;
using Afra_App.Profundum.Extensions;
using Afra_App.Schuljahr.Extensions;
using Afra_App.User.Extensions;
using Afra_App.User.Services;
using Microsoft.AspNetCore.HttpOverrides;
using OpenTelemetry.Metrics;

CultureInfo.CurrentCulture = CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfoByIetfLanguageTag("de-DE");

var builder = WebApplication.CreateBuilder(args);

var otel = builder.Services.AddOpenTelemetry();

otel.WithMetrics(metrics => metrics
    .AddAspNetCoreInstrumentation()
    .AddRuntimeInstrumentation()
    .AddMeter("System.Net.Http")
    .AddMeter("System.Net.NameResolution")
    .AddMeter("Microsoft.AspNetCore.Hosting")
    .AddMeter("Microsoft.AspNetCore.Server.Kestrel")
    .AddPrometheusExporter()
);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.ConfigureHttpJsonOptions(options => ConfigureJsonOptions(options.SerializerOptions));
builder.Services.AddSignalR()
    .AddJsonProtocol(options => ConfigureJsonOptions(options.PayloadSerializerOptions));
builder.Services.AddControllers();
builder.Services.AddHybridCache();
builder.Services.AddOpenApi();
builder.Services.AddCors(options =>
{
    options.DefaultPolicyName = "default";
    options.AddPolicy("default",
        corsPolicyBuilder => corsPolicyBuilder.AllowAnyMethod().AllowAnyOrigin().AllowAnyHeader());
});

builder.AddBackbone();
builder.AddCalendar();
builder.AddOtium();
builder.AddSchuljahr();
builder.AddProfundum();
builder.AddUser();

builder.Services.AddScoped<ProfundumsBewertungService>();
builder.Services.AddScoped<UserAccessor>();

var app = builder.Build();


app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor
                       | ForwardedHeaders.XForwardedProto
                       | ForwardedHeaders.XForwardedHost,
    KnownNetworks =
    {
        IPNetwork.Parse("10.0.0.0/8"),
        IPNetwork.Parse("172.16.0.0/12"),
        IPNetwork.Parse("192.168.0.0/16"),
        IPNetwork.Parse("fc00::/7")
    }
});

app.MapPrometheusScrapingEndpoint();

app.UseBackbone();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseCors();
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "Afra-App API"); });
    app.MapControllers();
}

app.MapOtium();
app.MapSchuljahr();
app.MapProfundum();
app.MapUser();
app.MapCalendar();

app.Run();
return;

void ConfigureJsonOptions(JsonSerializerOptions options)
{
    options.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    options.Converters.Add(new JsonStringEnumConverter());
    options.Converters.Add(new TimeOnlyJsonConverter());
}

using System.Globalization;
using Altafraner.AfraApp;
using Altafraner.AfraApp.Backbone.Extensions;
using Altafraner.AfraApp.Calendar.Extensions;
using Altafraner.AfraApp.Otium.Extensions;
using Altafraner.AfraApp.Profundum.Extensions;
using Altafraner.AfraApp.Schuljahr.Extensions;
using Altafraner.AfraApp.User.Domain.Models;
using Altafraner.AfraApp.User.Extensions;
using Altafraner.Backbone;
using Altafraner.Backbone.CookieAuthentication;
using Altafraner.Backbone.DataProtection;
using Altafraner.Backbone.EmailOutbox;
using Altafraner.Backbone.Defaults;
using Altafraner.Backbone.Defaults.Submodules;
using Altafraner.Backbone.EmailSchedulingModule;
using Altafraner.Backbone.Scheduling;

CultureInfo.CurrentCulture = CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfoByIetfLanguageTag("de-DE");

var builder = WebApplication.CreateBuilder(args);

builder.UseAltafranerBackbone(configure: altafranerBuilder => altafranerBuilder
    .AddModuleAndConfigure<CookieAuthenticationModule, CookieAuthenticationSettings>()
    .AddModule<DataProtectionModule<AfraAppContext>>()
    .AddModule<EmailOutboxModule>()
    .AddModuleAndConfigure<EmailSchedulingModule<Person>, EmailSchedulingSettings<Person>>(settings =>
        settings.WithDbContextStore<AfraAppContext>())
    .AddModule<DefaultsModule>()
    .AddModule<ReverseProxyHandlerModule>()
    .AddModule<SchedulingModule>()
);

builder.Services.AddControllers();

builder.AddBackbone();
builder.AddCalendar();
builder.AddOtium();
builder.AddSchuljahr();
builder.AddProfundum();
builder.AddUser();

var app = builder.Build();

app.AddAltafranerMiddleware();

app.UseBackbone();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapControllers();
}

app.MapAltafranerBackbone();
app.Services.WarmupAltafranerBackbone(CancellationToken.None);

app.MapOtium();
app.MapSchuljahr();
app.MapProfundum();
app.MapUser();
app.MapCalendar();

app.Run();

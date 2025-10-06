using System.Globalization;
using Altafraner.AfraApp;
using Altafraner.AfraApp.Backbone.Authorization;
using Altafraner.AfraApp.Backbone.EmergencyBackup;
using Altafraner.AfraApp.Calendar;
using Altafraner.AfraApp.Otium;
using Altafraner.AfraApp.Profundum.Extensions;
using Altafraner.AfraApp.Schuljahr;
using Altafraner.AfraApp.User;
using Altafraner.AfraApp.User.Domain.Models;
using Altafraner.Backbone;
using Altafraner.Backbone.CookieAuthentication;
using Altafraner.Backbone.DataProtection;
using Altafraner.Backbone.Defaults;
using Altafraner.Backbone.Defaults.Submodules;
using Altafraner.Backbone.EmailOutbox;
using Altafraner.Backbone.EmailSchedulingModule;
using Altafraner.Backbone.Scheduling;

CultureInfo.CurrentCulture = CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfoByIetfLanguageTag("de-DE");

var builder = WebApplication.CreateBuilder(args);

builder.UseAltafranerBackbone(configure: altafranerBuilder => altafranerBuilder
    .AddModule<CalendarModule>()
    .AddModule<DatabaseModule>()
    .AddModule<OtiumModule>()
    .AddModule<UserModule>()
    .AddModule<SchuljahrModule>()
    .AddModule<AuthorizationModule>()
    .AddModuleAndConfigure<CookieAuthenticationModule, CookieAuthenticationSettings>()
    .AddModule<DataProtectionModule<AfraAppContext>>()
    .AddModule<EmailOutboxModule>()
    .AddModuleAndConfigure<EmailSchedulingModule<Person>, EmailSchedulingSettings<Person>>(settings =>
        settings.WithDbContextStore<AfraAppContext>())
    .AddModule<DefaultsModule>()
    .AddModule<ReverseProxyHandlerModule>()
    .AddModule<SchedulingModule>()
    .AddModule<EmergencyBackupModule>()
);

builder.Services.AddControllers();

builder.AddProfundum();

var app = builder.Build();

app.AddAltafranerMiddleware();

if (app.Environment.IsDevelopment())
{
    app.MapControllers();
}

app.MapAltafranerBackbone();
await app.WarmupAltafranerBackbone();

app.MapProfundum();

app.Run();

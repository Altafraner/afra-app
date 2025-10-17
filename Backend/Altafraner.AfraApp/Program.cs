using System.Globalization;
using Altafraner.AfraApp;
using Altafraner.AfraApp.Backbone.Authorization;
using Altafraner.AfraApp.Backbone.EmergencyBackup;
using Altafraner.AfraApp.Calendar;
using Altafraner.AfraApp.Otium;
using Altafraner.AfraApp.Profundum;
using Altafraner.AfraApp.Schuljahr;
using Altafraner.AfraApp.User;
using Altafraner.AfraApp.User.Domain.Models;
using Altafraner.Backbone;
using Altafraner.Backbone.CookieAuthentication;
using Altafraner.Backbone.DataProtection;
using Altafraner.Backbone.Defaults;
using Altafraner.Backbone.EmailOutbox;
using Altafraner.Backbone.EmailSchedulingModule;
using Altafraner.Backbone.Scheduling;

CultureInfo.CurrentCulture = CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfoByIetfLanguageTag("de-DE");

var builder = WebApplication.CreateBuilder(args);

builder.UseAltafranerBackbone(configure: altafranerBuilder => altafranerBuilder
// Afra-App modules
    .AddModule<CalendarModule>()
    .AddModule<DatabaseModule>()
    .AddModule<OtiumModule>()
    .AddModule<UserModule>()
    .AddModule<SchuljahrModule>()
    .AddModule<ProfundumModule>()
    .AddModule<AuthorizationModule>()
    .AddModule<EmergencyBackupModule>()
// Backbone modules
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

var app = builder.Build();

app.AddAltafranerMiddleware();
app.MapAltafranerBackbone();
if (app.Environment.IsDevelopment()) app.MapControllers();
await app.WarmupAltafranerBackbone();

app.Run();

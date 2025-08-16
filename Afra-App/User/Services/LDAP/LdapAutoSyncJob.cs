﻿using Afra_App.Backbone.Email.Services.Contracts;
using Afra_App.Backbone.Scheduler.Templates;
using Afra_App.User.Configuration.LDAP;
using Microsoft.Extensions.Options;
using Quartz;

namespace Afra_App.User.Services.LDAP;

internal sealed class LdapAutoSyncJob : RetryJob
{
    private readonly IServiceProvider _serviceProvider;

    public LdapAutoSyncJob(IServiceProvider serviceProvider, ILogger<LdapAutoSyncJob> logger) : base(logger)
    {
        _serviceProvider = serviceProvider;
    }

    protected override int MaxRetryCount => 3;

    protected override async Task ExecuteAsync(IJobExecutionContext context, int _)
    {
        using var scope = _serviceProvider.CreateScope();
        var ldapService = scope.ServiceProvider.GetRequiredService<LdapService>();
        await ldapService.SynchronizeAsync();
    }

    protected override async Task HandleFinalFailureAsync(IJobExecutionContext context)
    {
        using var scope = _serviceProvider.CreateScope();
        var outbox = scope.ServiceProvider.GetRequiredService<IEmailOutbox>();
        var config = scope.ServiceProvider.GetRequiredService<IOptions<LdapConfiguration>>();

        const string subject = "LDAP Synchronization Failed";
        const string body =
            "LDAP synchronisierung ist mehrmals fehlgeschlagen. Bitte überprüfen Sie die Konfiguration.";

        foreach (var mail in config.Value.NotificationEmails) await outbox.SendReportAsync(mail, subject, body);
    }
}

using Quartz;

namespace Altafraner.AfraApp.User.Services.LDAP;

internal sealed class LdapAutoSyncJob : IJob
{
    private readonly LdapService _ldapService;

    public LdapAutoSyncJob(LdapService ldapService)
    {
        _ldapService = ldapService;
    }

    public async ValueTask Execute(IJobExecutionContext context,
        CancellationToken cancellationToken)
    {
        await _ldapService.SynchronizeAsync();
    }
}

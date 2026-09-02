using Altafraner.AfraApp.User.Domain.Contracts;
using Altafraner.AfraApp.User.Domain.Models;

namespace Altafraner.AfraApp.User.Services;

internal class DefaultUserEventHandler : IUserEventHandler
{
    public Task OnUserSoftDeletedAsync(Person user)
    {
        user.Deleted = true;

        user.FirstName = "Ausgeschieden";
        user.LastName = "Ausgeschieden";

        user.CevexId = null;
        user.CevexIdManuallyEntered = true;
        user.CevexSyncFailureTime = null;

        user.LdapObjectId = null;
        user.LdapSyncFailureTime = null;
        user.LdapSyncTime = null;

        user.Email = "";
        user.GlobalPermissions = [];
        user.Gruppe = "0";

        return Task.CompletedTask;
    }
}

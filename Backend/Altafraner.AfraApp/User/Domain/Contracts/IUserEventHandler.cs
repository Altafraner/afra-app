using Altafraner.AfraApp.User.Domain.Models;

namespace Altafraner.AfraApp.User.Domain.Contracts;

/// <summary>
///     A contract for user event handlers
/// </summary>
public interface IUserEventHandler
{
    /// <summary>
    ///     Called each time a user is soft deleted
    /// </summary>
    Task OnUserSoftDeletedAsync(Person user);
}

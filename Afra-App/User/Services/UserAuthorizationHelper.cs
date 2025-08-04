using Afra_App.User.Domain.Models;

namespace Afra_App.User.Services;

/// <summary>
///     A helper class for user authorization in the Afra-App.
/// </summary>
public class UserAuthorizationHelper
{
    private readonly UserAccessor _userAccessor;
    private readonly UserService _userService;

    /// <summary>
    ///     Called by DI
    /// </summary>
    public UserAuthorizationHelper(UserService userService, UserAccessor userAccessor)
    {
        _userService = userService;
        _userAccessor = userAccessor;
    }

    /// <summary>
    ///     Gets the currently authenticated user.
    /// </summary>
    /// <exception cref="UnauthorizedAccessException">No user is authenticated</exception>
    public async Task<Person> GetUserAsync()
    {
        var user = await _userAccessor.GetUserAsync();
        if (user is null)
            throw new UnauthorizedAccessException("User is not authenticated.");

        return user;
    }

    /// <summary>
    ///     Checks if the current user has the given user ID.
    /// </summary>
    /// <param name="userId">The id to check the current users id against</param>
    /// <returns>True, iff the currently authenticated user has the given id</returns>
    /// <exception cref="UnauthorizedAccessException">No user is authenticated</exception>
    public async Task<bool> CurrentUserHasId(Guid userId)
    {
        var currentUser = await GetUserAsync();
        return currentUser.Id == userId;
    }

    /// <summary>
    ///     Checks if the current user has the given role.
    /// </summary>
    /// <param name="role">The role to check</param>
    /// <returns>True, iff the currently authenticated user has the given role</returns>
    /// <exception cref="UnauthorizedAccessException">No user is authenticated</exception>
    public async Task<bool> CurrentUserHasRole(Rolle role)
    {
        var currentUser = await GetUserAsync();
        return currentUser.Rolle == role;
    }

    /// <summary>
    ///     Checks if the current user has the given global permission.
    /// </summary>
    /// <param name="permission">The global permission to check against</param>
    /// <returns>True, iff the user has the global permission</returns>
    public async Task<bool> CurrentUserHasGlobalPermission(GlobalPermission permission)
    {
        var currentUser = await GetUserAsync();
        return currentUser.GlobalPermissions.Contains(permission);
    }

    /// <summary>
    ///     Checks if the current user is a mentor of the given student.
    /// </summary>
    /// <param name="student"></param>
    /// <returns></returns>
    public async Task<bool> CurrentUserIsMentorOf(Person student)
    {
        var currentUser = await GetUserAsync();
        var mentors = await _userService.GetMentorsAsync(student);
        return mentors.Any(m => m.Id == currentUser.Id);
    }
}

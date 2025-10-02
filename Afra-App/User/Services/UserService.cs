using Altafraner.AfraApp.User.Configuration.LDAP;
using Altafraner.AfraApp.User.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Altafraner.AfraApp.User.Services;

/// <summary>
///     A service for managing users in the Afra-App.
/// </summary>
public class UserService
{
    private readonly AfraAppContext _dbContext;
    private readonly LdapConfiguration _ldapConfiguration;

    /// <summary>
    ///     Called by DI
    /// </summary>
    public UserService(AfraAppContext dbContext, IOptions<LdapConfiguration> ldapConfiguration)
    {
        _dbContext = dbContext;
        _ldapConfiguration = ldapConfiguration.Value;
    }

    /// <summary>
    ///     Gets a user by their ID.
    /// </summary>
    /// <returns>The users Person entity</returns>
    public async Task<Person> GetUserByIdAsync(Guid userId)
    {
        try
        {
            return await _dbContext.Personen
                .FirstAsync(p => p.Id == userId);
        }
        catch (InvalidOperationException)
        {
            throw new KeyNotFoundException("User not found.");
        }
    }

    /// <summary>
    ///     Fetches all users by their role.
    /// </summary>
    public async Task<IEnumerable<Person>> GetUsersWithRoleAsync(Rolle role)
    {
        return await _dbContext.Personen
            .Where(p => p.Rolle == role)
            .ToListAsync();
    }

    /// <summary>
    ///     Gets a list of users with a specific global permission.
    /// </summary>
    public async Task<IEnumerable<Person>> GetUsersWithGlobalPermissionAsync(GlobalPermission permission)
    {
        return await _dbContext.Personen
            .Where(p => p.GlobalPermissions.Contains(permission))
            .ToListAsync();
    }

    /// <summary>
    ///     Gets a list of mentors for a given student.
    /// </summary>
    /// <param name="student">The student to get the mentors of</param>
    /// <returns>A list of the students mentors</returns>
    public async Task<List<Person>> GetMentorsAsync(Person student)
    {
        if (student.Rolle == Rolle.Tutor)
            throw new InvalidOperationException("Tutors do not have mentors.");

        await _dbContext.Entry(student).Collection(s => s.Mentors).LoadAsync();

        return student.Mentors.ToList();
    }

    /// <summary>
    /// Gets the mentees of a given mentor.
    /// </summary>
    /// <param name="mentor"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task<List<Person>> GetMenteesAsync(Person mentor)
    {
        if (mentor.Rolle != Rolle.Tutor)
            throw new InvalidOperationException("Only tutors can have mentees.");

        await _dbContext.Entry(mentor).Collection(s => s.Mentees).LoadAsync();

        return mentor.Mentees.ToList();
    }

    /// <summary>
    ///     Gets the grade level of a student based on their group.
    /// </summary>
    /// <exception cref="InvalidOperationException">The person is not a student</exception>
    /// <exception cref="InvalidDataException">The persons group does not contain a valid grade level</exception>
    public int GetKlassenstufe(Person person)
    {
        if (person.Rolle == Rolle.Tutor)
            throw new InvalidOperationException("Only students have a grade level.");

        if (string.IsNullOrWhiteSpace(person.Gruppe) || !char.IsAsciiDigit(person.Gruppe[0]))
            throw new InvalidDataException("The person does not have a valid group.");

        return Convert.ToInt32(String.Concat(person.Gruppe.TakeWhile(char.IsAsciiDigit)));
    }

    /// <summary>
    ///     Gets all grade levels
    /// </summary>
    public IEnumerable<int> GetKlassenstufen()
    {
        return _ldapConfiguration.UserGroups.Select(x => x.Group)
            .Select(s => string.Concat(s.TakeWhile(char.IsAsciiDigit)))
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Select(int.Parse)
            .Order()
            .Distinct();
    }
}

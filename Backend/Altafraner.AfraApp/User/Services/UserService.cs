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
    ///     Gets users by their ID
    /// </summary>
    /// <exception cref="KeyNotFoundException">Not all provided IDs correspond to users</exception>
    public async Task<List<Person>> GetUsersByIdsAsync(IEnumerable<Guid> userIds)
    {
        var distinctUserIds = userIds.Distinct().ToArray();
        var users = await _dbContext.Personen.Where(p => distinctUserIds.Contains(p.Id)).ToListAsync();
        return users.Count == distinctUserIds.Length ? users : throw new KeyNotFoundException();
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

        var mentors = await _dbContext.Entry(student).Collection(s => s.Mentors).Query().Distinct().ToListAsync();

        return mentors;
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

        var mentees = await _dbContext.Entry(mentor).Collection(s => s.Mentees).Query().Distinct().ToListAsync();

        return mentees;
    }

    /// <summary>
    ///     Gets the current grade level of a student based on their group. Equivalent to
    ///     <see cref="GetKlassenstufe(Person, DateTime)" /> as of now.
    /// </summary>
    /// <exception cref="InvalidOperationException">The person is not a student</exception>
    /// <exception cref="InvalidDataException">The persons group does not contain a valid grade level</exception>
    public int GetKlassenstufe(Person person) => GetKlassenstufe(person, DateTime.UtcNow);

    /// <summary>
    ///     Gets the grade level a student's group implied as of a specific point in time, based on the historized
    ///     group log (<see cref="PersonGruppenHistorie" />). Falls back to the person's current
    ///     <see cref="Person.Gruppe" /> if no historical entry predates <paramref name="asOf" /> - this covers data
    ///     that predates group history being tracked at all.
    /// </summary>
    /// <exception cref="InvalidOperationException">The person is not a student</exception>
    /// <exception cref="InvalidDataException">The persons group does not contain a valid grade level</exception>
    public int GetKlassenstufe(Person person, DateTime asOf)
    {
        if (person.Rolle == Rolle.Tutor)
            throw new InvalidOperationException("Only students have a grade level.");

        var gruppe = GetGruppe(person, asOf);

        if (string.IsNullOrWhiteSpace(gruppe) || !char.IsAsciiDigit(gruppe[0]))
            throw new InvalidDataException("The person does not have a valid group.");

        return Convert.ToInt32(String.Concat(gruppe.TakeWhile(char.IsAsciiDigit)));
    }

    /// <summary>
    ///     Gets the raw group (e.g. "9a") a person had as of a specific point in time, based on the historized
    ///     group log (<see cref="PersonGruppenHistorie" />) - falls back to <see cref="Person.Gruppe" /> if no
    ///     historical entry predates <paramref name="asOf" />. Unlike <see cref="GetKlassenstufe(Person, DateTime)" />,
    ///     this returns the full group string (including the class-letter suffix), for display/reporting purposes.
    /// </summary>
    public string? GetGruppe(Person person, DateTime asOf)
    {
        var entry = _dbContext.PersonGruppenHistorien
            .Where(h => h.PersonId == person.Id && h.GueltigAb <= asOf)
            .OrderByDescending(h => h.GueltigAb)
            .FirstOrDefault();
        return entry is not null ? entry.Gruppe : person.Gruppe;
    }

    /// <summary>
    ///     Sets a person's <see cref="Person.Gruppe" />, logging the change to <see cref="PersonGruppenHistorie" />
    ///     if the value actually differs from the current one - this is the only place <see cref="Person.Gruppe" />
    ///     should ever be written, so that <see cref="GetKlassenstufe(Person, DateTime)" /> can later reconstruct
    ///     what it used to be. Does not save changes; the caller is expected to as part of its own batch (e.g. one
    ///     LDAP sync run).
    /// </summary>
    public void SetGruppe(Person person, string? gruppe, DateTime asOf)
    {
        if (gruppe == person.Gruppe)
            return;

        _dbContext.PersonGruppenHistorien.Add(new PersonGruppenHistorie
        {
            Person = person,
            Gruppe = gruppe,
            GueltigAb = asOf,
        });
        person.Gruppe = gruppe;
    }

    /// <summary>
    ///     Gets all grade levels
    /// </summary>
    public IEnumerable<int> GetKlassenstufen()
    {
        return _dbContext.Personen.Select(x => x.Gruppe)
            .Distinct()
            .ToArray()
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Select(s => string.Concat(s!.TakeWhile(char.IsAsciiDigit)))
            .Select(int.Parse)
            .Order()
            .Distinct();
    }
}

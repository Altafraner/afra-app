using System.DirectoryServices.Protocols;
using System.Net;
using Afra_App.Backbone.Email.Services.Contracts;
using Afra_App.Backbone.Utilities;
using Afra_App.User.Configuration.LDAP;
using Afra_App.User.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Afra_App.User.Services.LDAP;

/// <summary>
///     A service for synchronizing with an LDAP server and authenticating users.
/// </summary>
public class LdapService
{
    private readonly LdapConfiguration _configuration;
    private readonly AfraAppContext _dbContext;
    private readonly IEmailOutbox _emailOutbox;
    private readonly ILogger<LdapService> _logger;
    private Dictionary<string, Person> _studentsByDn = [];

    private Dictionary<string, Person> _tutorsByDn = [];

    /// <summary>
    ///     Creates a new instance of the LdapService.
    /// </summary>
    public LdapService(IOptions<LdapConfiguration> configuration, ILogger<LdapService> logger, AfraAppContext dbContext,
        IEmailOutbox emailOutbox)
    {
        _configuration = configuration.Value;
        _logger = logger;
        _dbContext = dbContext;
        _emailOutbox = emailOutbox;
    }

    /// <summary>
    ///     Checks whether the LDAP service is enabled.
    /// </summary>
    public bool IsEnabled => _configuration.Enabled;

    /// <summary>
    ///     Synchronizes the database with the LDAP server.
    /// </summary>
    /// <exception cref="LdapException">The LDAP Server did not respond</exception>
    /// <exception cref="InvalidOperationException">The LDAP Service is not enabled. Check with <see cref="IsEnabled" />.</exception>
    public async Task SynchronizeAsync()
    {
        if (!_configuration.Enabled)
            throw new InvalidOperationException("Ldap is not enabled");

        _logger.LogInformation("LDAP synchronization started");
        using var connection = _configuration.BuildConnection(_logger);

        // This time is used to determine which users have been synced since the last sync. It must stay constant for the whole sync process
        var syncTime = DateTime.UtcNow;

        // Get all users once so we don't have to query the database for every user
        var dbUsers = await _dbContext.Personen
            .Where(p => p.LdapObjectId != null)
            .ToListAsync();

        foreach (var description in _configuration.UserGroups) UpdateUserGroup(description);
        foreach (var description in _configuration.PermissionGroups) UpdateGlobalPermissionGroup(description);

        await _dbContext.SaveChangesAsync(); // Save here to ensure all users have a valid ID.

        await UpdateMentors(_configuration.MentorGroups);

        var unsyncedUsers = await _dbContext.Personen
            .Where(p => p.LdapObjectId != null && p.LdapSyncTime < syncTime)
            .ToListAsync();
        if (unsyncedUsers.Count != 0)
        {
            _logger.LogWarning("There are {count} users that could not be synchronized", unsyncedUsers.Count);
            var unsyncedUsersWithoutNotification =
                unsyncedUsers.Where(user => user.LdapSyncFailureTime == null).ToList();
            if (unsyncedUsersWithoutNotification.Count != 0)
            {
                foreach (var user in unsyncedUsersWithoutNotification) user.LdapSyncFailureTime = syncTime;
                foreach (var email in _configuration.NotificationEmails)
                    await _emailOutbox.SendReportAsync(email, "LDAP Nutzer konnten nicht synchronisiert werden",
                        $"""
                         Es konnten nicht alle Benutzer synchronisiert werden. Möglicherweise wurden die Benutzer im Verzeichnisdienst gelöscht. Sollte dies der Fall sein, löschen Sie die Benutzer bitte manuell aus der Afra-App.
                         Neue nicht synchronisierte Benutzer:
                          - {string.Join($"{Environment.NewLine} - ", unsyncedUsersWithoutNotification.Select(u => $"{u.Vorname} {u.Nachname} ({u.Email})"))}

                         Falls der Benutzer versehentlich gelöscht wurde, erstellen Sie den Benutzer neu und, kontaktieren Sie den Datenbank-Administrator, um den Benutzer neu zu verknüpfen.

                         Falls der Benutzer nicht gelöscht wurde, überprüfen Sie bitte die LDAP-Konfiguration und die Verbindung zum LDAP-Server.
                         """);
            }
        }

        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("LDAP synchronization finished");
        return;

        void UpdateUserGroup(LdapUserSyncDescription ldapGroup)
        {
            var groupEntries = SubtreeSearch(connection, ldapGroup);
            foreach (SearchResultEntry entry in groupEntries)
            {
                var success =
                    TryGetOrCreatePersonFromEntry(entry, ldapGroup.Rolle, ldapGroup.Group, dbUsers, out var person);
                if (!success)
                {
                    _logger.LogError("Sync: Failed to create of find user from entry");
                    continue;
                }

                person!.LdapSyncTime = syncTime;
                person.LdapSyncFailureTime = null;
            }
        }

        void UpdateGlobalPermissionGroup(LdapPermissionSyncDescription ldapGroup)
        {
            // Keep track of users that already have the permission so we can remove them later if they are not in the group anymore.
            var usersWithPermission = dbUsers
                .Where(p => p.GlobalPermissions.Contains(ldapGroup.Permission))
                .ToHashSet();

            // Add users from LDAP group
            var groupEntries = SubtreeSearch(connection, ldapGroup);
            foreach (SearchResultEntry entry in groupEntries)
            {
                if (!entry.TryGetGuid(out var objGuid))
                {
                    _logger.LogError("Failed to obtain objectGuid, skipping");
                    continue;
                }

                var user = dbUsers.FirstOrDefault(p => p.LdapObjectId == objGuid);
                if (user is null)
                {
                    _logger.LogError("User with objectGuid {guid} not found in database, skipping", objGuid);
                    continue;
                }

                user.GlobalPermissions.AddOnce(ldapGroup.Permission);
                usersWithPermission.Remove(user);
            }

            // Add manually assigned users
            var manuallyAddedUsersMails = ldapGroup.ManuallyAssignedUsers.Select(e => e.ToLower().Trim()).ToArray();
            var manuallyAddedUsers = dbUsers.Where(u => manuallyAddedUsersMails.Contains(u.Email.ToLower().Trim()));
            foreach (var user in manuallyAddedUsers)
            {
                user.GlobalPermissions.AddOnce(ldapGroup.Permission);
                usersWithPermission.Remove(user);
            }

            // Remove users that were in the group but not anymore
            foreach (var user in usersWithPermission) user.GlobalPermissions.Remove(ldapGroup.Permission);
        }

        async Task UpdateMentors(LdapSearchDescription searchDescription)
        {
            var groups = SubtreeSearch(connection, searchDescription, "member");
            var dbEntries = await _dbContext.MentorMenteeRelations
                .ToListAsync();

            var dbEntriesByStruct =
                dbEntries.ToDictionary(e => new MentorMenteeRelationStruct(e.MentorId, e.StudentId), e => e);

            var dbEntriesSet = dbEntriesByStruct.Keys.ToHashSet();

            var allMentorGroups = groups
                .OfType<SearchResultEntry>()
                .ToDictionary(e => e.DistinguishedName, e => e.GetMulitAttribute("member"));

            foreach (var group in allMentorGroups)
            {
                var entriesList = group.Value.ToList();
                var tutorDn = entriesList.FirstOrDefault(e => _tutorsByDn.ContainsKey(e));
                var tutorSuccess = _tutorsByDn.TryGetValue(tutorDn ?? "", out var tutor);
                if (!tutorSuccess || tutorDn is null)
                {
                    _logger.LogWarning("No tutor found in group {dn}, skipping", group.Key);
                    continue;
                }

                entriesList.Remove(tutorDn);

                foreach (var studentDn in entriesList)
                {
                    var studentSuccess = _studentsByDn.TryGetValue(studentDn, out var student);
                    if (!studentSuccess)
                    {
                        _logger.LogWarning("Student {dn} not found", studentDn);
                        continue;
                    }

                    var relation = new MentorMenteeRelationStruct(tutor!.Id, student!.Id);
                    var exists = dbEntriesSet.Remove(relation);
                    if (!exists)
                        _dbContext.MentorMenteeRelations.Add(new MentorMenteeRelation()
                        {
                            MentorId = tutor.Id,
                            StudentId = student.Id
                        });
                }
            }

            foreach (var relation in dbEntriesSet) _dbContext.MentorMenteeRelations.Remove(dbEntriesByStruct[relation]);
        }
    }

    /// <summary>
    ///     Verifies a user by their username and password.
    /// </summary>
    /// <param name="username">The users username</param>
    /// <param name="password">The users (secret) password</param>
    /// <param name="shouldRetry">Whether to retry if the user exists in LDAP but not in DB</param>
    /// <returns>
    ///     The user authenticated by <paramref name="username" /> and <paramref name="password" /> if the credentials are
    ///     valid; Otherwise, null
    /// </returns>
    /// <exception cref="InvalidOperationException">The LDAP Service is not enabled. Check with <see cref="IsEnabled" />.</exception>
    public async Task<Person?> VerifyUserAsync(string username, string password, bool shouldRetry = true)
    {
        if (!_configuration.Enabled)
            throw new InvalidOperationException("Ldap is not enabled");

        // See https://datatracker.ietf.org/doc/html/rfc4513#section-5.1.2
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            return null;

        using var connection = _configuration.BuildConnection(_logger);
        var request = new SearchRequest(_configuration.GlobalScope.BaseDn,
            $"(&{_configuration.GlobalScope.Filter}(sAMAccountName={LdapSanitizer.Sanitize(username)}))",
            SearchScope.Subtree);

        var response = (SearchResponse)connection.SendRequest(request);
        if (response is null) return null;
        if (response.Entries.Count == 0) return null;

        var entry = response.Entries[0];
        var credential = new NetworkCredential(entry.DistinguishedName, password);
        try
        {
            connection.Bind(credential);
        }
        catch (LdapException)
        {
            return null;
        }

        if (!entry.TryGetGuid(out var objGuid))
        {
            _logger.LogError("Could not get objectGuid from entry on login\n dn: {dn}", entry.DistinguishedName);
            return null;
        }

        var user = await _dbContext.Personen.FirstOrDefaultAsync(p => p.LdapObjectId == objGuid);

        if (user is not null || !shouldRetry) return user;

        _logger.LogWarning("User not found in database, starting sync, this may take a while");
        await SynchronizeAsync();

        user = await _dbContext.Personen.FirstOrDefaultAsync(p => p.LdapObjectId == objGuid);
        if (user is null)
            _logger.LogError("User not found after sync. \n dn: {dn}\n guid: {guid}",
                entry.DistinguishedName, objGuid);

        return user;
    }

    private static SearchResultEntryCollection SubtreeSearch(LdapConnection connection, LdapSearchDescription group,
        params string[] attributes)
    {
        if (attributes.Length == 0) attributes = ["objectGuid", "givenName", "sn", "mail"];
        var searchRequest = new SearchRequest(group.BaseDn, group.Filter, SearchScope.Subtree, attributes);
        var response = (SearchResponse)connection.SendRequest(searchRequest);

        if (response is null) throw new LdapException("No response from LDAP server");
        return response.Entries;
    }

    private bool TryGetOrCreatePersonFromEntry(SearchResultEntry entry, Rolle rolle, string? gruppe,
        IEnumerable<Person> users,
        out Person? user)
    {
        if (!entry.TryGetGuid(out var objGuid))
        {
            _logger.LogError("Failed to obtain objectGuid for user {dn}, skipping", entry.DistinguishedName);
            user = null;
            return false;
        }

        var givenName = entry.GetSingleAttribute("givenName");
        var surname = entry.GetSingleAttribute("sn");
        var mail = entry.GetSingleAttribute("mail");

        if (givenName is null || surname is null || mail is null)
            _logger.LogWarning("User {dn} has missing attributes, filling with empty strings", entry.DistinguishedName);

        givenName ??= "";
        surname ??= "";
        mail ??= "";

        user = users.FirstOrDefault(p => p.LdapObjectId == objGuid);
        if (user is null)
        {
            user = new Person
            {
                LdapObjectId = objGuid,
                Vorname = givenName,
                Nachname = surname,
                Email = mail,
                Rolle = rolle,
                Gruppe = gruppe
            };

            _dbContext.Personen.Add(user);
            AddPersonToDict(entry, user);
            return true;
        }

        user.Vorname = givenName;
        user.Nachname = surname;
        user.Email = mail;
        user.Rolle = rolle;
        user.Gruppe = gruppe;
        AddPersonToDict(entry, user);
        return true;
    }

    private void AddPersonToDict(SearchResultEntry entry, Person person)
    {
        var dict = person.Rolle switch
        {
            Rolle.Tutor => _tutorsByDn,
            Rolle.Mittelstufe => _studentsByDn,
            Rolle.Oberstufe => _studentsByDn,
            _ => throw new InvalidOperationException("Unknown role")
        };
        dict[entry.DistinguishedName] = person;
    }

    private record struct MentorMenteeRelationStruct(Guid MentorId, Guid MenteeId);
}

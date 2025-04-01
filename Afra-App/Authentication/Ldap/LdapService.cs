using System.DirectoryServices.Protocols;
using System.Net;
using Afra_App.Data;
using Afra_App.Data.Configuration;
using Afra_App.Data.People;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Afra_App.Authentication.Ldap;

/// <summary>
///     A service for synchronizing with an LDAP server and authenticating users.
/// </summary>
public class LdapService
{
    private readonly LdapConfiguration _configuration;
    private readonly AfraAppContext _context;
    private readonly ILogger<LdapService> _logger;

    /// <summary>
    ///     Creates a new instance of the LdapService.
    /// </summary>
    public LdapService(IOptions<LdapConfiguration> configuration, ILogger<LdapService> logger, AfraAppContext context)
    {
        _configuration = configuration.Value;
        _logger = logger;
        _context = context;
    }

    /// <summary>
    ///     Synchronizes the database with the LDAP server.
    /// </summary>
    /// <exception cref="LdapException">The LDAP Server did not respond</exception>
    public async Task SynchronizeAsync()
    {
        _logger.LogInformation("Starting LDAP synchronization");
        using var connection = LdapHelper.BuildConnection(_configuration);
        var syncTime = DateTime.UtcNow;
        var dbUsers = await _context.Personen.Where(p => p.LdapObjectId != null).ToListAsync();

        var teacherEntries = GetGroupEntries(connection, _configuration.TutorGroup);
        foreach (SearchResultEntry entry in teacherEntries)
        {
            var success = TryGetOrCreatePersonFromEntry(entry, Rolle.Tutor, dbUsers, out var person);
            if (!success)
            {
                _logger.LogWarning("Sync: Failed to create of find user from entry");
                continue;
            }

            person!.LdapSyncTime = syncTime;
        }

        var studentEntries = GetGroupEntries(connection, _configuration.StudentGroup);
        foreach (SearchResultEntry entry in studentEntries)
        {
            var success = TryGetOrCreatePersonFromEntry(entry, Rolle.Student, dbUsers, out var person);
            if (!success)
            {
                _logger.LogWarning("Sync: Failed to create of find user from entry");
                continue;
            }

            person!.LdapSyncTime = syncTime;
        }

        await _context.SaveChangesAsync();

        var unsyncedUsers = await _context.Personen
            .Where(p => p.LdapObjectId != null && p.LdapSyncTime < syncTime)
            .ToListAsync();

        if (unsyncedUsers.Count != 0)
            _logger.LogWarning("There are {count} users that could not be synchronized", unsyncedUsers.Count);
    }

    /// <summary>
    ///     Verifies a user by their username and password.
    /// </summary>
    /// <param name="username">The users username</param>
    /// <param name="password">The users (secret) password</param>
    /// <param name="shouldRetry">Whether to retry if the user exists in LDAP but not in DB</param>
    /// <returns></returns>
    public async Task<Person?> VerifyUserAsync(string username, string password, bool shouldRetry = true)
    {
        using var connection = LdapHelper.BuildConnection(_configuration);
        var request = new SearchRequest(_configuration.BaseDn, $"(sAMAccountName={LdapHelper.Sanitize(username)})",
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

        if (!LdapHelper.TryGetGuidFromEntry(entry, out var objGuid))
        {
            _logger.LogWarning("Could not get objectGuid from entry on login\n dn: {dn}", entry.DistinguishedName);
            return null;
        }

        var user = await _context.Personen.FirstOrDefaultAsync(p => p.LdapObjectId == objGuid);

        if (user is not null || !shouldRetry) return user;

        _logger.LogWarning("User not found in database, starting sync");
        await SynchronizeAsync();

        user = await _context.Personen.FirstOrDefaultAsync(p => p.LdapObjectId == objGuid);
        if (user is null)
            _logger.LogError("User not found after sync. \n dn: {dn}\n guid: {guid}",
                entry.DistinguishedName, objGuid);

        return user;
    }

    private SearchResultEntryCollection GetGroupEntries(LdapConnection connection, string group)
    {
        var searchRequest = new SearchRequest(_configuration.BaseDn, $"(&(objectClass=user)(memberOf={group}))",
            SearchScope.Subtree);
        var response = (SearchResponse)connection.SendRequest(searchRequest);

        if (response is null) throw new LdapException("No response from LDAP server");
        return response.Entries;
    }

    private bool TryGetOrCreatePersonFromEntry(SearchResultEntry entry, Rolle rolle, IEnumerable<Person> users,
        out Person? user)
    {
        if (!LdapHelper.TryGetGuidFromEntry(entry, out var objGuid))
        {
            _logger.LogWarning("Failed to obtain objectGuid, skipping");
            user = null;
            return false;
        }

        var givenName = LdapHelper.GetSingleAttribute(entry, "givenName");
        var surname = LdapHelper.GetSingleAttribute(entry, "sn");
        var mail = LdapHelper.GetSingleAttribute(entry, "mail");

        if (givenName is null || surname is null || mail is null)
            _logger.LogWarning("User has missing attributes, filling with empty strings");

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
                Rolle = rolle
            };

            _context.Personen.Add(user);
            return true;
        }

        user.Vorname = givenName;
        user.Nachname = surname;
        user.Email = mail;
        user.Rolle = rolle;
        return true;
    }
}
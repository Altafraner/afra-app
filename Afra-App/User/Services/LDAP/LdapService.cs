﻿using System.DirectoryServices.Protocols;
using System.Net;
using Afra_App.Backbone.Services.Email;
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

        await _dbContext.SaveChangesAsync();

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

                await _dbContext.SaveChangesAsync();
            }
        }

        _logger.LogInformation("LDAP synchronization finished");
        return;

        void UpdateUserGroup(LdapUserSyncDescription ldapGroup)
        {
            var groupEntries = GetGroupEntries(connection, ldapGroup);
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
            var groupEntries = GetGroupEntries(connection, ldapGroup);
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

    private static SearchResultEntryCollection GetGroupEntries(LdapConnection connection, LdapSearchDescription group)
    {
        var searchRequest = new SearchRequest(group.BaseDn, group.Filter, SearchScope.Subtree);
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
            _logger.LogError("Failed to obtain objectGuid, skipping");
            user = null;
            return false;
        }

        var givenName = entry.GetSingleAttribute("givenName");
        var surname = entry.GetSingleAttribute("sn");
        var mail = entry.GetSingleAttribute("mail");

        if (givenName is null || surname is null || mail is null)
            _logger.LogWarning("User {guid} has missing attributes, filling with empty strings", objGuid);

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
            return true;
        }

        user.Vorname = givenName;
        user.Nachname = surname;
        user.Email = mail;
        user.Rolle = rolle;
        user.Gruppe = gruppe;
        return true;
    }
}

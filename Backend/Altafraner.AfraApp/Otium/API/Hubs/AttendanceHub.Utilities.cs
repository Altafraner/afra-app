using Altafraner.AfraApp.Otium.Domain.DTO;
using Altafraner.AfraApp.Otium.Domain.DTO.Notiz;
using Altafraner.AfraApp.Otium.Domain.HubClients;
using Altafraner.AfraApp.Otium.Domain.Models;
using Altafraner.AfraApp.User.Domain.DTO;
using Person = Altafraner.AfraApp.User.Domain.Models.Person;

namespace Altafraner.AfraApp.Otium.API.Hubs;

internal partial class AttendanceHub
{
    internal static string TerminGroupName(Guid terminId)
    {
        return $"termin-{terminId}";
    }

    internal static string BlockGroupName(Guid blockId)
    {
        return $"block-{blockId}";
    }

    private async Task UpdateAttendance(OtiumAnwesenheitsStatus status, Guid studentId, Guid blockId, Guid terminId)
    {
        await _attendanceService.SetAttendanceForStudentInBlockAsync(studentId, blockId, status);
        await Clients.Groups([TerminGroupName(terminId), BlockGroupName(blockId)])
            .UpdateAttendance(new IAttendanceHubClient.AttendanceUpdate(studentId, terminId, blockId, status));
    }

    private async Task<List<IAttendanceHubClient.TerminInformation>> GetBlockAttendances(Guid blockId)
    {
        var (attendancesByTermin, missingPersons, missingPersonsChecked) =
            await _attendanceService.GetAttendanceForBlockAsync(blockId);

        var notesByPerson = await _notesService.GetNotesByBlockAsync(blockId);

        List<IAttendanceHubClient.TerminInformation> updates = [];
        foreach (var (termin, anwesenheitByPerson) in attendancesByTermin)
        {
            var enrollments = anwesenheitByPerson
                .Select(entry =>
                    new LehrerEinschreibung(new PersonInfoMinimal(entry.Key),
                        entry.Value,
                        notesByPerson.GetValueOrDefault(entry.Key.Id, []).Select(n => new Notiz(n))))
                .OrderBy(e => e.Student?.Vorname)
                .ThenBy(e => e.Student?.Nachname)
                .ToList();
            updates.Add(new IAttendanceHubClient.TerminInformation(termin.Id,
                termin.Bezeichnung,
                termin.Ort,
                enrollments,
                termin.SindAnwesenheitenKontrolliert));
        }

        updates = updates.OrderBy(e => e.Ort).ToList();

        var missingPersonsEnrollments = missingPersons
            .Select(entry =>
                new LehrerEinschreibung(new PersonInfoMinimal(entry.Key),
                    entry.Value,
                    notesByPerson.GetValueOrDefault(entry.Key.Id, []).Select(n => new Notiz(n))))
            .OrderBy(e => e.Student?.Vorname)
            .ThenBy(e => e.Student?.Nachname)
            .ToList();
        updates.Insert(0,
            new IAttendanceHubClient.TerminInformation(Guid.Empty,
                "Nicht eingeschrieben",
                "FEHLEND",
                missingPersonsEnrollments,
                missingPersonsChecked));

        return updates;
    }

    private async Task<List<LehrerEinschreibung>> GetTerminAttendances(Guid terminId, Guid blockId)
    {
        var enrollments = await _attendanceService.GetAttendanceForTerminAsync(terminId);
        return await enrollments
            .ToAsyncEnumerable()
            .Select<KeyValuePair<Person, OtiumAnwesenheitsStatus>, LehrerEinschreibung>(async (entry, _) =>
                new LehrerEinschreibung(
                    new PersonInfoMinimal(entry.Key),
                    entry.Value,
                    (await _notesService.GetNotesAsync(entry.Key.Id, blockId)).Select(n => new Notiz(n))))
            .ToListAsync();
    }

    private Task SendUpdateToAffected(Guid blockId, Guid terminId)
    {
        return SendUpdateToAffected(blockId, Guid.Empty, terminId);
    }

    private async Task SendNotificationToAffected(string subject,
        string message,
        Guid blockId,
        Guid fromTerminId,
        Guid toTerminId)
    {
        var notification =
            new IAttendanceHubClient.Notification(subject, message, IAttendanceHubClient.NotificationSeverity.Info);
        await Clients.Groups([BlockGroupName(blockId)]).Notify(notification);
        if (fromTerminId != Guid.Empty)
            await Clients.Groups([TerminGroupName(fromTerminId)]).Notify(notification);
        if (toTerminId != Guid.Empty)
            await Clients.Groups([TerminGroupName(toTerminId)]).Notify(notification);
    }

    private async Task SendUpdateToAffected(Guid blockId, Guid fromTerminId, Guid toTerminId)
    {
        var blockUpdates = await GetBlockAttendances(blockId);
        await Clients.Group(BlockGroupName(blockId)).UpdateBlockAttendances(blockUpdates);

        var toTerminUpdates = blockUpdates.FirstOrDefault(t => t.TerminId == toTerminId);
        if (toTerminUpdates is not null)
            await Clients.Group(TerminGroupName(toTerminId))
                .UpdateTerminAttendances(toTerminUpdates.Einschreibungen);
        else
            _logger.LogWarning("Tried to update termine for {fromTerminId}, but did not find any", fromTerminId);

        if (fromTerminId == Guid.Empty)
            return;
        var fromTerminUpdates = blockUpdates.FirstOrDefault(t => t.TerminId == fromTerminId);
        if (fromTerminUpdates is not null)
            await Clients.Group(TerminGroupName(fromTerminId))
                .UpdateTerminAttendances(fromTerminUpdates.Einschreibungen);
        else
            _logger.LogWarning("Tried to update termine for {fromTerminId}, but did not find any", fromTerminId);
    }
}

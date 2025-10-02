using Altafraner.AfraApp.Otium.Domain.Contracts.Services;
using Altafraner.AfraApp.Otium.Domain.DTO;
using Altafraner.AfraApp.Otium.Domain.HubClients;
using Altafraner.AfraApp.Otium.Domain.Models;
using Altafraner.AfraApp.Otium.Jobs;
using Altafraner.AfraApp.Otium.Services;
using Altafraner.AfraApp.Schuljahr.Domain.Models;
using Altafraner.AfraApp.User.Domain.DTO;
using Altafraner.AfraApp.User.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Quartz;

namespace Altafraner.AfraApp.Otium.API.Hubs;

/// <summary>
///     A hub for managing attendance updates in the Otium application.
/// </summary>
public class AttendanceHub : Hub<IAttendanceHubClient>
{
    private readonly IAttendanceService _attendanceService;
    private readonly ILogger<AttendanceHub> _logger;

    /// <summary>
    ///     Constructs a new instance of the <see cref="AttendanceHub" /> class.
    /// </summary>
    public AttendanceHub(IAttendanceService attendanceService, ILogger<AttendanceHub> logger)
    {
        _attendanceService = attendanceService;
        _logger = logger;
    }

    /// <summary>
    ///     Subscribes a user to get updates for a specific termin.
    /// </summary>
    /// <param name="terminId">The <see cref="Guid" /> of the <see cref="Domain.Models.OtiumTermin" /> to subscribe to.</param>
    /// <param name="managementService">From DI</param>
    public async Task SubscribeToTermin(Guid terminId, ManagementService managementService)
    {
        var termin = await managementService.GetTerminByIdAsync(terminId);
        var block = await managementService.GetBlockOfTerminAsync(termin);
        if (Context.User is null || !_attendanceService.MaySupervise(Context.User, block))
        {
            _logger.LogWarning("User {userId} tried to subscribe to termin {terminId} without authority",
                Context.UserIdentifier, terminId);
            throw new HubException("You do not have permission to subscribe to this termin.");
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, TerminGroupName(terminId));
        var updates = await GetTerminAttendances(terminId);
        await Clients.Caller.UpdateTerminAttendances(updates);
    }

    /// <summary>
    ///     Unsubscribes a user from updates for a specific termin.
    /// </summary>
    /// <param name="terminId">The <see cref="Guid" /> of the <see cref="Domain.Models.OtiumTermin" /> to unsubscribe from.</param>
    public Task UnsubscribeFromTermin(Guid terminId)
    {
        return Groups.RemoveFromGroupAsync(Context.ConnectionId, TerminGroupName(terminId));
    }

    /// <summary>
    ///     Subscribes a user to get updates for a specific block.
    /// </summary>
    /// <param name="blockId">The <see cref="Guid" /> of the <see cref="Block" /> to subscribe to.</param>
    /// <param name="schedulerFactory"></param>
    /// <param name="blockHelper"></param>
    /// <param name="context"></param>
    public async Task SubscribeToBlock(Guid blockId, ISchedulerFactory schedulerFactory, BlockHelper blockHelper,
        AfraAppContext context)
    {
        var block = await context.Blocks.FindAsync(blockId);
        if (block is null)
            throw new HubException($"Block with ID {blockId} not found");
        if (Context.User is null || !_attendanceService.MaySupervise(Context.User, block))
        {
            _logger.LogWarning("User {userId} tried to subscribe to block {blockId} without authority",
                Context.UserIdentifier, blockId);
            throw new HubException("You do not have permission to subscribe to this block.");
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, BlockGroupName(blockId));
        var updates = await GetBlockAttendances(blockId);
        await Clients.Caller.UpdateBlockAttendances(updates);
        var scheduler = await schedulerFactory.GetScheduler();
        var jobKey = new JobKey($"MissingStudentNotification-{blockId}");

        var metadata = blockHelper.Get(block.SchemaId)!;

        var now = DateTime.Now;
        var today = DateOnly.FromDateTime(now);
        var time = TimeOnly.FromDateTime(now);

        if (!metadata.Verpflichtend || block.SchultagKey != today || time >= metadata.Interval.Start.AddMinutes(30))
            return;

        if (await scheduler.CheckExists(jobKey))
            return;

        var job = JobBuilder.Create<MissingStudentNotificationJob>()
            .WithIdentity(jobKey)
            .UsingJobData("block", blockId.ToString())
            .UsingJobData("fire_after", metadata.Interval.Start.AddMinutes(30).ToShortTimeString())
            .UsingJobData("block_schema", block.SchemaId)
            .DisallowConcurrentExecution()
            .PersistJobDataAfterExecution()
            .Build();
        var trigger = TriggerBuilder.Create()
            .WithIdentity($"MissingStudentNotificationTrigger-{blockId}")
            .WithSimpleSchedule(x => x.WithIntervalInMinutes(1).RepeatForever())
            .StartNow()
            .Build();
        await scheduler.ScheduleJob(job, trigger);
    }

    /// <summary>
    ///     Unsubscribes a user from updates for a specific block.
    /// </summary>
    /// <param name="blockId">The <see cref="Guid" /> of the <see cref="Block" /> to unsubscribe from.</param>
    public Task UnsubscribeFromBlock(Guid blockId)
    {
        return Groups.RemoveFromGroupAsync(Context.ConnectionId, BlockGroupName(blockId));
    }

    /// <summary>
    ///     Sets the attendance status for a user in a block.
    /// </summary>
    /// <param name="blockId">The id of the <see cref="Block" /></param>
    /// <param name="studentId">The id of the Students <see cref="User.Domain.Models.Person" /> entity.</param>
    /// <param name="status">The new status</param>
    /// <param name="dbContext">Injected via DI</param>
    public async Task SetAttendanceStatusInBlock(Guid blockId, Guid studentId, OtiumAnwesenheitsStatus status,
        AfraAppContext dbContext)
    {
        var block = await dbContext.Blocks
            .FirstOrDefaultAsync(b => b.Id == blockId);
        if (block is null)
            throw new HubException($"Block with ID {blockId} not found");
        if (Context.User is null || !_attendanceService.MaySupervise(Context.User, block))
        {
            _logger.LogWarning("User {userId} tried to set attendance status for block {blockId} without authority",
                Context.UserIdentifier, blockId);
            throw new HubException("You do not have permission to set attendance status for this block.");
        }

        // Might return Guid.Empty if the user is not enrolled in any termin of the block
        var terminId = await dbContext.OtiaEinschreibungen
            .Where(e => e.Termin.Block.Id == blockId)
            .Where(e => e.BetroffenePerson.Id == studentId)
            .Select(t => t.Termin.Id)
            .FirstOrDefaultAsync();

        await UpdateAttendance(status, studentId, blockId, terminId);
    }

    /// <summary>
    ///     Set the attendance status for a specific termin for a specific student.
    /// </summary>
    /// <param name="terminId">The id of the <see cref="Domain.Models.OtiumTermin" /></param>
    /// <param name="studentId">The id of the students <see cref="User.Domain.Models.Person" /> entity</param>
    /// <param name="status">The new status</param>
    /// <param name="dbContext">Injected from DI-Container</param>
    /// <exception cref="KeyNotFoundException"></exception>
    public async Task SetAttendanceStatusInTermin(Guid terminId, Guid studentId, OtiumAnwesenheitsStatus status,
        AfraAppContext dbContext)
    {
        var block = await dbContext.OtiaTermine.Where(t => t.Id == terminId)
            .Select(t => t.Block)
            .FirstOrDefaultAsync();
        if (block is null)
            throw new HubException($"Block for Termin ID {terminId} not found");
        if (Context.User is null || !_attendanceService.MaySupervise(Context.User, block))
        {
            _logger.LogWarning("User {userId} tried to set attendance status for termin {terminId} without authority",
                Context.UserIdentifier, terminId);
            throw new HubException("You do not have permission to set attendance status for this termin.");
        }

        var blockId = block.Id;
        if (blockId == Guid.Empty)
            throw new HubException($"Termin ID {terminId} not found");

        await UpdateAttendance(status, studentId, blockId, terminId);
    }

    /// <summary>
    ///     Updates the checked-status of a specific termin or all missing persons in a block.
    /// </summary>
    /// <param name="blockId">The id of the block the update is for</param>
    /// <param name="terminId">
    ///     The id of the termin the update is for. Use <see cref="Guid.Empty">Guid.Empty</see> for missing
    ///     persons.
    /// </param>
    /// <param name="status">The new status</param>
    /// <param name="dbContext">From DI</param>
    public async Task SetTerminStatus(Guid blockId, Guid terminId, bool status, AfraAppContext dbContext)
    {
        var block = await dbContext.Blocks
            .FirstOrDefaultAsync(b => b.Id == blockId);
        if (block is null)
            throw new HubException($"Block with ID {blockId} not found");
        if (Context.User is null || !_attendanceService.MaySupervise(Context.User, block))
        {
            _logger.LogWarning("User {userId} tried to set termin status for block {blockId} without authority",
                Context.UserIdentifier, blockId);
            throw new HubException("You do not have permission to set termin status for this block.");
        }

        if (terminId == Guid.Empty)
            await _attendanceService.SetStatusForMissingPersonsAsync(blockId, status);
        else
            await _attendanceService.SetStatusForTerminAsync(terminId, status);

        await Clients.Groups(TerminGroupName(terminId), BlockGroupName(blockId))
            .UpdateTerminStatus(new IAttendanceHubClient.TerminStatusUpdate(terminId, status));
    }

    /// <summary>
    /// Retrieves the parallel running termins for a given termin.
    /// </summary>
    /// <param name="terminId">The id of the termin to find alternatives to</param>
    /// <param name="dbContext">From DI</param>
    /// <returns></returns>
    public async Task<IEnumerable<MinimalTermin>> GetTerminAlternatives(Guid terminId, AfraAppContext dbContext)
    {
        var termin = await dbContext.OtiaTermine
            .Include(t => t.Block)
            .FirstOrDefaultAsync(t => t.Id == terminId);

        if (termin is null)
            throw new HubException($"Termin ID {terminId} not found");

        var alternatives = await dbContext.OtiaTermine
            .Include(t => t.Tutor)
            .Include(t => t.Block)
            .Include(t => t.Otium)
            .Where(t => t.Block.Id == termin.Block.Id && t.Id != terminId)
            .OrderBy(t => t.OverrideBezeichnung != null ? t.OverrideBezeichnung : t.Otium.Bezeichnung)
            .Select(t => new MinimalTermin(t.Id,
                t.OverrideBezeichnung != null ? t.OverrideBezeichnung : t.Otium.Bezeichnung,
                t.Tutor != null ? new PersonInfoMinimal(t.Tutor) : null, t.Ort))
            .ToListAsync();

        return alternatives
            .Prepend(new MinimalTermin(Guid.Empty, "Nicht eingeschrieben", null, "FEHLEND"));
    }

    /// <summary>
    ///     Moves a student from one termin to another.
    /// </summary>
    /// <param name="studentId">The id of the student to move</param>
    /// <param name="fromTerminId">
    ///     The id of the termin to move the student from. Use <see cref="Guid.Empty">Guid.Empty</see>
    ///     when the student is not enrolled.
    /// </param>
    /// <param name="toTerminId">The id of the termin to move the student to</param>
    /// <param name="enrollmentService">From DI</param>
    /// <param name="dbContext">From DI</param>
    /// <param name="userService">From DI</param>
    public async Task MoveStudentNow(Guid studentId, Guid fromTerminId, Guid toTerminId,
        EnrollmentService enrollmentService, AfraAppContext dbContext, UserService userService)
    {
        if (fromTerminId == toTerminId)
            return;

        var fromBlock = await dbContext.OtiaTermine
            .Where(t => t.Id == fromTerminId)
            .Select(t => t.Block)
            .FirstOrDefaultAsync();
        var toData = await dbContext.OtiaTermine
            .Where(t => t.Id == toTerminId)
            .Select(t => new
            {
                t.Block,
                Bezeichnung = t.OverrideBezeichnung != null ? t.OverrideBezeichnung : t.Otium.Bezeichnung
            })
            .FirstOrDefaultAsync();

        var blockId = fromBlock?.Id ?? toData?.Block.Id;
        if ((fromBlock is null && fromTerminId != Guid.Empty) || (toData is null && toTerminId != Guid.Empty))
            throw new HubException("One of the termin IDs provided was not found in the database.");
        if ((fromBlock is not null && toData is not null && fromBlock.Id != toData.Block.Id) ||
            blockId is null)
        {
            _logger.LogWarning(
                "Someone tried to move a student ({studentId}) from a termin ({fromTerminId}) to a termin ({toTerminId}) that is not in the same block",
                studentId, fromTerminId, toTerminId);
            throw new HubException("The termin is not in the same block as the target termin.");
        }

        if (toData is null && fromBlock is null)
        {
            _logger.LogWarning(
                "User {userId} tried to move student {studentId} from termin {fromTerminId} to termin {toTerminId}, but i dont seem to be able to find either one.",
                Context.UserIdentifier, studentId, fromTerminId, toTerminId);
            throw new HubException("Can't find either termin.");
        }

        if (Context.User is null
            || (fromBlock is not null && !_attendanceService.MaySupervise(Context.User, fromBlock))
            || (toData is not null && !_attendanceService.MaySupervise(Context.User, toData.Block))
            || (toData is null && fromBlock is null))
        {
            _logger.LogWarning(
                "User {userId} tried to move student {studentId} from termin {fromTerminId} to termin {toTerminId} without authority",
                Context.UserIdentifier, studentId, fromTerminId, toTerminId);
            throw new HubException("You do not have permission to move students in this block.");
        }

        try
        {
            await enrollmentService.ForceMoveNow(studentId, fromTerminId, toTerminId);
            await _attendanceService.SetAttendanceForStudentInBlockAsync(studentId, blockId.Value,
                OtiumAnwesenheitsStatus.Fehlend);
            if (toTerminId != Guid.Empty)
                await _attendanceService.SetStatusForTerminAsync(toTerminId, false);
            else
                await _attendanceService.SetStatusForMissingPersonsAsync(blockId.Value, false);

            var student = await userService.GetUserByIdAsync(studentId);
            await SendNotificationToAffected("Schüler:in verschoben", toData is not null
                    ? $"{student.Vorname} {student.Nachname} wurde zum Termin „{toData.Bezeichnung}“ verschoben."
                    : $"{student.Vorname} {student.Nachname} wurde ausgetragen.",
                blockId.Value, fromTerminId, toTerminId);
            await SendUpdateToAffected(blockId.Value, fromTerminId, toTerminId);
        }
        catch (KeyNotFoundException)
        {
            throw new HubException("One of the IDs provided was not found in the database.");
        }
        catch (InvalidOperationException)
        {
            _logger.LogWarning(
                "Someone tried to move a student ({studentId}) from a termin ({fromTerminId}) that is not currently running",
                studentId, fromTerminId);
            throw new HubException("The termin is not currently running");
        }
    }

    /// <summary>
    ///     Moves a student from one termin to another.
    /// </summary>
    /// <param name="studentId">The id of the user</param>
    /// <param name="toTerminId">The id of the termin to move the user to</param>
    /// <param name="enrollmentService">From DI</param>
    /// <param name="dbContext">From DI</param>
    /// <param name="userService">From DI</param>
    public async Task MoveStudent(Guid studentId, Guid toTerminId, EnrollmentService enrollmentService,
        AfraAppContext dbContext, UserService userService)
    {
        var toData = dbContext.OtiaTermine
            .Where(t => t.Id == toTerminId)
            .Include(t => t.Otium)
            .AsEnumerable()
            .Select(t => new
            {
                t.Block,
                Bezeichnung = t.OverrideBezeichnung != null ? t.OverrideBezeichnung : t.Otium.Bezeichnung
            })
            .FirstOrDefault();

        if (toData is null)
            throw new HubException("One of the termin IDs provided was not found in the database.");

        if (Context.User is null || !_attendanceService.MaySupervise(Context.User, toData.Block))
        {
            _logger.LogWarning(
                "User {userId} tried to move student {studentId} to termin {toTerminId} without authority",
                Context.UserIdentifier, studentId, toTerminId);
            throw new HubException("You do not have permission to move students in this block.");
        }

        try
        {
            var (fromTerminId, blockId) = await enrollmentService.ForceMove(studentId, toTerminId);
            await _attendanceService.SetStatusForTerminAsync(toTerminId, false);
            var student = await userService.GetUserByIdAsync(studentId);
            await SendUpdateToAffected(blockId, fromTerminId, toTerminId);
            await SendNotificationToAffected("Schüler:in verschoben",
                $"{student.Vorname} {student.Nachname} wurde zum Termin „{toData.Bezeichnung}“ verschoben.",
                blockId, fromTerminId, toTerminId);
        }
        catch (KeyNotFoundException)
        {
            throw new HubException("One of the IDs provided was not found in the database.");
        }
    }

    /// <summary>
    /// Unenrolls a student from a specific termin.
    /// </summary>
    public async Task ForceUnenroll(Guid studentId, Guid fromTerminId, EnrollmentService enrollmentService,
        UserService userService, AfraAppContext dbContext)
    {
        var block = await dbContext.OtiaTermine
            .Where(t => t.Id == fromTerminId)
            .Select(t => t.Block)
            .FirstOrDefaultAsync();
        if (block is null || Context.User is null || !_attendanceService.MaySupervise(Context.User, block))
            throw new HubException("You do not have permission to unenroll students in this block.");

        var user = await userService.GetUserByIdAsync(studentId);
        await enrollmentService.UnenrollAsync(fromTerminId, user, true);
        await _attendanceService.SetStatusForMissingPersonsAsync(block.Id, false);
        await SendUpdateToAffected(block.Id, fromTerminId);
        await SendNotificationToAffected("Schüler:in verschoben",
            $"{user.Vorname} {user.Nachname} wurde ausgetragen.",
            block.Id, fromTerminId, Guid.Empty);
    }

    private async Task UpdateAttendance(OtiumAnwesenheitsStatus status, Guid studentId, Guid blockId, Guid terminId)
    {
        await _attendanceService.SetAttendanceForStudentInBlockAsync(studentId, blockId, status);
        await Clients.Groups(TerminGroupName(terminId), BlockGroupName(blockId))
            .UpdateAttendance(new IAttendanceHubClient.AttendanceUpdate(studentId, terminId, blockId, status));
    }

    private async Task<List<IAttendanceHubClient.TerminInformation>> GetBlockAttendances(Guid blockId)
    {
        var (attendancesByTermin, missingPersons, missingPersonsChecked) =
            await _attendanceService.GetAttendanceForBlockAsync(blockId);

        List<IAttendanceHubClient.TerminInformation> updates = [];
        foreach (var (termin, anwesenheitByPerson) in attendancesByTermin)
        {
            var enrollments = anwesenheitByPerson.Select((entry, _) =>
                    new LehrerEinschreibung(new PersonInfoMinimal(entry.Key), entry.Value))
                .OrderBy(e => e.Student?.Vorname)
                .ThenBy(e => e.Student?.Nachname)
                .ToList();
            updates.Add(new IAttendanceHubClient.TerminInformation(termin.Id, termin.Bezeichnung, termin.Ort,
                enrollments, termin.SindAnwesenheitenKontrolliert));
        }

        updates = updates.OrderBy(e => e.Ort).ToList();

        var missingPersonsEnrollments = missingPersons.Select((entry, _) =>
                new LehrerEinschreibung(new PersonInfoMinimal(entry.Key), entry.Value))
            .OrderBy(e => e.Student?.Vorname)
            .ThenBy(e => e.Student?.Nachname)
            .ToList();
        updates.Insert(0, new IAttendanceHubClient.TerminInformation(Guid.Empty, "Nicht eingeschrieben", "FEHLEND",
            missingPersonsEnrollments, missingPersonsChecked));

        return updates;
    }

    private async Task<List<LehrerEinschreibung>> GetTerminAttendances(Guid terminId)
    {
        var enrollments = await _attendanceService.GetAttendanceForTerminAsync(terminId);
        return enrollments.Select(entry => new LehrerEinschreibung(new PersonInfoMinimal(entry.Key), entry.Value))
            .ToList();
    }

    private Task SendUpdateToAffected(Guid blockId, Guid terminId)
    {
        return SendUpdateToAffected(blockId, Guid.Empty, terminId);
    }

    private async Task SendNotificationToAffected(string subject, string message, Guid blockId, Guid fromTerminId,
        Guid toTerminId)
    {
        var notification =
            new IAttendanceHubClient.Notification(subject, message, IAttendanceHubClient.NotificationSeverity.Info);
        await Clients.Groups(BlockGroupName(blockId)).Notify(notification);
        if (fromTerminId != Guid.Empty)
            await Clients.Groups(TerminGroupName(fromTerminId)).Notify(notification);
        if (toTerminId != Guid.Empty)
            await Clients.Groups(TerminGroupName(toTerminId)).Notify(notification);
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

    internal static string TerminGroupName(Guid terminId)
    {
        return $"termin-{terminId}";
    }

    internal static string BlockGroupName(Guid blockId)
    {
        return $"block-{blockId}";
    }
}

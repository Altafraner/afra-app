using Afra_App.Data;
using Afra_App.Data.DTO;
using Afra_App.Data.DTO.Otium;
using Afra_App.Data.Otium;
using Afra_App.Data.Schuljahr;
using Afra_App.Services.Otium;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Quartz;

namespace Afra_App.Endpoints.Otium;

/// <summary>
///     A hub for managing attendance updates in the Otium application.
/// </summary>
public class AttendanceHub : Hub<IAttendanceHubClient>
{
    private readonly IAttendanceService _attendanceService;

    /// <summary>
    ///     Constructs a new instance of the <see cref="AttendanceHub" /> class.
    /// </summary>
    public AttendanceHub(IAttendanceService attendanceService)
    {
        _attendanceService = attendanceService;
    }

    /// <summary>
    ///     Subscribes a user to get updates for a specific termin.
    /// </summary>
    /// <param name="terminId">The <see cref="Guid" /> of the <see cref="Data.Otium.Termin" /> to subscribe to.</param>
    public async Task SubscribeToTermin(Guid terminId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, TerminGroupName(terminId));
        var updates = await GetTerminAttendances(terminId);
        await Clients.Caller.UpdateTerminAttendances(updates);
    }

    /// <summary>
    ///     Unsubscribes a user from updates for a specific termin.
    /// </summary>
    /// <param name="terminId">The <see cref="Guid" /> of the <see cref="Data.Otium.Termin" /> to unsubscribe from.</param>
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
        await Groups.AddToGroupAsync(Context.ConnectionId, BlockGroupName(blockId));
        var updates = await GetBlockAttendances(blockId);
        await Clients.Caller.UpdateBlockAttendances(updates);
        var scheduler = await schedulerFactory.GetScheduler();
        var jobKey = new JobKey($"MissingStudentNotification-{blockId}");

        var block = await context.Blocks.FindAsync(blockId);
        var metadata = blockHelper.Get(block!.SchemaId)!;

        var now = DateTime.Now;
        var today = DateOnly.FromDateTime(now);
        var time = TimeOnly.FromDateTime(now);

        if (block.SchultagKey != today ||
            !time.IsBetween(metadata.Interval.Start, metadata.Interval.Start.AddMinutes(25)))
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
    /// <param name="studentId">The id of the Students <see cref="Data.People.Person" /> entity.</param>
    /// <param name="status">The new status</param>
    /// <param name="dbContext">Injected via DI</param>
    public async Task SetAttendanceStatusInBlock(Guid blockId, Guid studentId, AnwesenheitsStatus status,
        AfraAppContext dbContext)
    {
        // Might return Guid.Empty if the user is not enrolled in any termin of the block
        var terminId = dbContext.OtiaEinschreibungen
            .Where(e => e.Termin.Block.Id == blockId)
            .Where(e => e.BetroffenePerson.Id == studentId)
            .Select(t => t.Termin.Id)
            .FirstOrDefault();

        await UpdateAttendance(status, studentId, blockId, terminId);
    }

    /// <summary>
    ///     Set the attendance status for a specific termin for a specific student.
    /// </summary>
    /// <param name="terminId">The id of the <see cref="Data.Otium.Termin" /></param>
    /// <param name="studentId">The id of the students <see cref="Data.People.Person" /> entity</param>
    /// <param name="status">The new status</param>
    /// <param name="dbContext">Injected from DI-Container</param>
    /// <exception cref="KeyNotFoundException"></exception>
    public async Task SetAttendanceStatusInTermin(Guid terminId, Guid studentId, AnwesenheitsStatus status,
        AfraAppContext dbContext)
    {
        var blockId = dbContext.OtiaTermine.Where(t => t.Id == terminId)
            .Select(t => t.Block.Id)
            .FirstOrDefault();

        if (blockId == Guid.Empty)
            throw new KeyNotFoundException($"Termin ID {terminId} not found");

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
    public async Task SetTerminStatus(Guid blockId, Guid terminId, bool status)
    {
        if (terminId == Guid.Empty)
            await _attendanceService.SetStatusForMissingPersonsAsync(blockId, status);
        else
            await _attendanceService.SetStatusForTerminAsync(terminId, status);

        await Clients.Groups(TerminGroupName(terminId), BlockGroupName(blockId))
            .UpdateTerminStatus(new IAttendanceHubClient.TerminStatusUpdate(terminId, status));
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
    public async Task MoveStudentNow(Guid studentId, Guid fromTerminId, Guid toTerminId,
        EnrollmentService enrollmentService, AfraAppContext dbContext)
    {
        if (fromTerminId == toTerminId)
            return;

        try
        {
            await enrollmentService.ForceMoveNow(studentId, fromTerminId, toTerminId);
            var blockId = await dbContext.OtiaTermine
                .Where(t => t.Id == toTerminId)
                .Select(t => t.Block.Id)
                .FirstOrDefaultAsync();

            await SendUpdateToAffected(blockId, fromTerminId, toTerminId);
        }
        catch (KeyNotFoundException)
        {
            throw new HubException("One of the IDs provided was not found in the database.");
        }
        catch (InvalidOperationException)
        {
            throw new HubException("The termin is not currently running");
        }
    }

    /// <summary>
    ///     Moves a student from one termin to another.
    /// </summary>
    /// <param name="studentId"></param>
    /// <param name="toTerminId"></param>
    /// <param name="enrollmentService"></param>
    /// <exception cref="HubException"></exception>
    public async Task MoveStudent(Guid studentId, Guid toTerminId, EnrollmentService enrollmentService)
    {
        try
        {
            var (fromTerminId, blockId) = await enrollmentService.ForceMove(studentId, toTerminId);
            await SendUpdateToAffected(blockId, fromTerminId, toTerminId);
        }
        catch (KeyNotFoundException)
        {
            throw new HubException("One of the IDs provided was not found in the database.");
        }
    }

    private async Task UpdateAttendance(AnwesenheitsStatus status, Guid studentId, Guid blockId, Guid terminId)
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
            updates.Add(new IAttendanceHubClient.TerminInformation(termin.Id, termin.Otium.Bezeichnung, termin.Ort,
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

    private async Task SendUpdateToAffected(Guid blockId, Guid fromTerminId, Guid toTerminId)
    {
        var blockUpdates = await GetBlockAttendances(blockId);
        await Clients.Group(BlockGroupName(blockId)).UpdateBlockAttendances(blockUpdates);

        var toTerminUpdates = blockUpdates.FirstOrDefault(t => t.TerminId == toTerminId);
        if (toTerminUpdates is not null)
            await Clients.Group(TerminGroupName(toTerminId))
                .UpdateTerminAttendances(toTerminUpdates.Einschreibungen);

        if (fromTerminId == Guid.Empty)
            return;
        var fromTerminUpdates = blockUpdates.FirstOrDefault(t => t.TerminId == fromTerminId);
        if (fromTerminUpdates is not null)
            await Clients.Group(TerminGroupName(fromTerminId))
                .UpdateTerminAttendances(fromTerminUpdates.Einschreibungen);
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

using System.Text;
using System.Web;
using Altafraner.AfraApp.Attendance.Domain.Contracts;
using Altafraner.AfraApp.Attendance.Domain.Dto;
using Altafraner.AfraApp.Backbone.EmergencyBackup.Services.Contracts;
using Altafraner.AfraApp.User.Domain.Models;
using Quartz;

namespace Altafraner.AfraApp.Attendance.Jobs;

/// <summary>
///     A job that uploads emergency backups of currently running Otium-Blocks.
/// </summary>
public class EmergencyUploadJob : IJob
{
    private readonly IAttendanceService _attendanceService;
    private readonly IEmergencyBackupService _backupService;
    private readonly ILogger<EmergencyUploadJob> _logger;
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    ///     Called from DI
    /// </summary>
    public EmergencyUploadJob(IEmergencyBackupService backupService,
        IAttendanceService attendanceService,
        ILogger<EmergencyUploadJob> logger,
        IServiceProvider serviceProvider)
    {
        _backupService = backupService;
        _attendanceService = attendanceService;
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    /// <inheritdoc />
    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            var allInformationProviders =
                _serviceProvider.GetKeyedServices<IAttendanceInformationProvider>(KeyedService.AnyKey);

            foreach (var provider in allInformationProviders)
            {
                var activeSlots = await provider.GetActiveSlots();
                foreach (var slot in activeSlots) await BackupSlot(provider, slot);
            }
        }
        catch (Exception e)
        {
            _logger.LogError("Error during emergency upload: {Message}", e.Message);
            throw new JobExecutionException(e)
            {
                RefireImmediately = false,
                UnscheduleAllTriggers = false,
                UnscheduleFiringTrigger = false
            };
        }
    }

    private async Task BackupSlot(IAttendanceInformationProvider provider, AttendanceSlot slot)
    {
        var termine = await provider.GetEnrollmentsForSlot(slot.SlotId);
        var attendances = await _attendanceService.GetAttendanceForSlotAsync(slot.Scope, slot.SlotId);

        var html =
            $$"""
              <!DOCTYPE html>
              <html lang="de">
                  <head>
                      <meta charset="UTF-8">
                      <meta name="viewport" content="width=device-width, initial-scale=1.0">
                      <title>Otium Notfall-Backup {{HttpUtility.HtmlEncode($"{DateTime.Now:yyyy-MM-dd HH:mm}")}}</title>
                      <style>
                          body {
                              font-family: Arial, sans-serif;
                              margin: 20px;
                          }
                          table {
                              width: 100%;
                              border-collapse: collapse;
                              margin-bottom: 20px;
                          }
                          th, td {
                              border: 1px solid #ddd;
                              padding: 8px;
                              text-align: left;
                          }
                      </style>
                  </head>
                  <body>
                      <h1>Aufsichts Notfall-Backup {{HttpUtility.HtmlEncode($"{DateTime.Now:yyyy-MM-dd HH:mm}")}}</h1>
                      <p>Slot: {{HttpUtility.HtmlEncode(slot.Bezeichnung)}}</p>
                      <h2>Termine</h2>
                      {{termine.Select(t => $"<h3>{HttpUtility.HtmlEncode(t.Location)} {HttpUtility.HtmlEncode(t.Name)}</h3>"
                                            + GenerateHtmlTable(t.Enrollments)).Aggregate(new StringBuilder(), (current, next) => current.Append(next))}}
                  </body>
              </html>
              """;
        await _backupService.SaveHtmlAsync(
            $"Otium {DateTime.Now:yyyy-MM-dd} {slot.Bezeichnung}",
            html);
        return;

        string GenerateHtmlTable(IEnumerable<Person> personen)
        {
            return personen
                       .Select(person =>
                           $"<tr><td>{HttpUtility.HtmlEncode(person.LastName)}, {HttpUtility.HtmlEncode(person.FirstName)}</td><td>{HttpUtility.HtmlEncode(attendances.GetValueOrDefault(person, IAttendanceService.DefaultAttendanceStatus))}</td></tr>")
                       .Aggregate("<table><tr><th>Name</th><th>Status</th></tr>", (current, row) => current + row) +
                   "</table>";
        }
    }
}

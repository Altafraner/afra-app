using Afra_App.Backbone.EmergencyBackup.Services.Contracts;
using Afra_App.Otium.Domain.Models;
using Afra_App.Otium.Services;
using Afra_App.Schuljahr.Services;
using Afra_App.User.Domain.Models;
using Quartz;

namespace Afra_App.Otium.Jobs;

/// <summary>
/// A job that uploads emergency backups of currently running Otium-Blocks.
/// </summary>
public class EmergencyUploadJob : IJob
{
    private readonly IAttendanceService _attendanceService;
    private readonly IEmergencyBackupService _backupService;
    private readonly BlockHelper _blockHelper;
    private readonly ILogger<EmergencyUploadJob> _logger;
    private readonly SchuljahrService _schuljahrService;

    /// <summary>
    /// Called from DI
    /// </summary>
    public EmergencyUploadJob(IEmergencyBackupService backupService, IAttendanceService attendanceService,
        SchuljahrService schuljahrService, BlockHelper blockHelper, ILogger<EmergencyUploadJob> logger)
    {
        _backupService = backupService;
        _attendanceService = attendanceService;
        _schuljahrService = schuljahrService;
        _blockHelper = blockHelper;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            var block = await _schuljahrService.GetCurrentBlockAsync();
            if (block == null) return;

            var (termine, missingPersons, _) = await _attendanceService.GetAttendanceForBlockAsync(block.Id);

            var html =
                $$"""
                  <!DOCTYPE html>
                  <html lang="de">
                      <head>
                          <meta charset="UTF-8">
                          <meta name="viewport" content="width=device-width, initial-scale=1.0">
                          <title>Otium Notfall-Backup {{DateTime.Now:yyyy-MM-dd HH:mm}}</title>
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
                          <h1>Otium Notfall-Backup {{DateTime.Now:yyyy-MM-dd HH:mm}}</h1>
                          <p>Block: {{_blockHelper.Get(block.SchemaId)!.Bezeichnung}}</p>
                          <h2>Termine</h2>
                          <h3>Fehlende</h3>
                          {{GenerateHtmlTable(missingPersons)}}
                          {{termine.Select(t => $"<h3>{t.Key.Ort} {t.Key.Otium.Bezeichnung}</h3>" + GenerateHtmlTable(t.Value))
                                  .Aggregate("", (current, next) => current + next)}}
                      </body>
                  </html>
                  """;
            await _backupService.SaveHtmlAsync(
                $"Otium {DateTime.Now:yyyy-MM-dd} {_blockHelper.Get(block.SchemaId)!.Bezeichnung}", html);
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

    private static string GenerateHtmlTable(Dictionary<Person, OtiumAnwesenheitsStatus> attendances)
    {
        return attendances
            .OrderBy(a => a.Key.Nachname)
            .ThenBy(a => a.Key.Vorname)
            .Select(attendance =>
                $"<tr><td>{attendance.Key.Nachname}, {attendance.Key.Vorname}</td><td>{attendance.Value}</td></tr>")
            .Aggregate("<table><tr><th>Name</th><th>Status</th></tr>", (current, row) => current + row) + "</table>";
    }
}

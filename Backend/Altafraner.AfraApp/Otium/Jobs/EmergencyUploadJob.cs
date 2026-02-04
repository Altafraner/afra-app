using System.Text;
using System.Web;
using Altafraner.AfraApp.Backbone.EmergencyBackup.Services.Contracts;
using Altafraner.AfraApp.Otium.Domain.Contracts.Services;
using Altafraner.AfraApp.Otium.Domain.Models;
using Altafraner.AfraApp.Otium.Services;
using Altafraner.AfraApp.Schuljahr.Services;
using Altafraner.AfraApp.User.Domain.Models;
using Quartz;

namespace Altafraner.AfraApp.Otium.Jobs;

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
    public EmergencyUploadJob(
        IEmergencyBackupService backupService,
        IAttendanceService attendanceService,
        SchuljahrService schuljahrService,
        BlockHelper blockHelper,
        ILogger<EmergencyUploadJob> logger
    )
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
            if (block == null)
                return;

            var (termine, missingPersons, _) = await _attendanceService.GetAttendanceForBlockAsync(
                block.Id
            );

            var html = $$"""
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
                          <h1>Otium Notfall-Backup {{HttpUtility.HtmlEncode($"{DateTime.Now:yyyy-MM-dd HH:mm}")}}</h1>
                          <p>Block: {{HttpUtility.HtmlEncode(_blockHelper.Get(block.SchemaId)!.Bezeichnung)}}</p>
                          <h2>Termine</h2>
                          <h3>Fehlende</h3>
                          {{GenerateHtmlTable(missingPersons)}}
                          {{termine.Select(t => $"<h3>{HttpUtility.HtmlEncode(t.Key.Ort)} {HttpUtility.HtmlEncode(t.Key.Bezeichnung)}</h3>"
                                                + GenerateHtmlTable(t.Value)).Aggregate(new StringBuilder(), (current, next) => current.Append(next))}}
                      </body>
                  </html>
                  """;
            await _backupService.SaveHtmlAsync(
                $"Otium {DateTime.Now:yyyy-MM-dd} {_blockHelper.Get(block.SchemaId)!.Bezeichnung}",
                html
            );
        }
        catch (Exception e)
        {
            _logger.LogError("Error during emergency upload: {Message}", e.Message);
            throw new JobExecutionException(e)
            {
                RefireImmediately = false,
                UnscheduleAllTriggers = false,
                UnscheduleFiringTrigger = false,
            };
        }
    }

    private static string GenerateHtmlTable(Dictionary<Person, OtiumAnwesenheitsStatus> attendances)
    {
        return attendances
                .OrderBy(a => a.Key.LastName)
                .ThenBy(a => a.Key.FirstName)
                .Select(attendance =>
                    $"<tr><td>{HttpUtility.HtmlEncode(attendance.Key.LastName)}, {HttpUtility.HtmlEncode(attendance.Key.FirstName)}</td><td>{HttpUtility.HtmlEncode(attendance.Value)}</td></tr>"
                )
                .Aggregate(
                    "<table><tr><th>Name</th><th>Status</th></tr>",
                    (current, row) => current + row
                ) + "</table>";
    }
}

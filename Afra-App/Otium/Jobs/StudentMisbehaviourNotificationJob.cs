using System.Text;
using Afra_App.Backbone.Email.Services.Contracts;
using Afra_App.Backbone.Scheduler.Templates;
using Afra_App.Backbone.Utilities;
using Afra_App.Otium.Configuration;
using Afra_App.Otium.Domain.Models;
using Afra_App.Otium.Services;
using Afra_App.Schuljahr.Domain.Models;
using Afra_App.Schuljahr.Services;
using Afra_App.User.Domain.Models;
using Afra_App.User.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Quartz;

namespace Afra_App.Otium.Jobs;

/// <summary>
///     A job that notifies mentors about student misbehaviour.
/// </summary>
internal sealed class StudentMisbehaviourNotificationJob : RetryJob
{
    private readonly AfraAppContext _dbContext;
    private readonly IEmailOutbox _emailOutbox;
    private readonly ILogger<StudentMisbehaviourNotificationJob> _logger;
    private readonly IOptions<OtiumConfiguration> _otiumConfiguration;
    private readonly RulesValidationService _rulesValidationService;
    private readonly SchuljahrService _schuljahrService;
    private readonly UserService _userService;

    /// <summary>
    ///     Called from DI
    /// </summary>
    public StudentMisbehaviourNotificationJob(ILogger<StudentMisbehaviourNotificationJob> logger,
        IEmailOutbox emailOutbox, SchuljahrService schuljahrService, UserService userService,
        IOptions<OtiumConfiguration> otiumConfiguration, AfraAppContext dbContext,
        RulesValidationService rulesValidationService) : base(logger)
    {
        _logger = logger;
        _emailOutbox = emailOutbox;
        _schuljahrService = schuljahrService;
        _userService = userService;
        _otiumConfiguration = otiumConfiguration;
        _dbContext = dbContext;
        _rulesValidationService = rulesValidationService;
    }

    protected override int MaxRetryCount => 3;

    /// <exception cref="ArgumentOutOfRangeException"></exception>
    /// <inheritdoc />
    protected override async Task ExecuteAsync(IJobExecutionContext context, int _)
    {
        if (!_otiumConfiguration.Value.StudentMisbehaviourNotification.Enabled) return;

        var now = DateTime.Now;
        var hasRun = context.JobDetail.JobDataMap.TryGetDateTime("last_run", out var lastRun);
        if (TimeOnly.FromDateTime(now) < _otiumConfiguration.Value.StudentMisbehaviourNotification.Time.AddMinutes(-5))
        {
            _logger.LogWarning(
                "Student Misbehaviour job was scheduled before the default reminder time. Skipping execution.");
            return;
        }

        if (hasRun && lastRun.Date == now.Date)
        {
            _logger.LogInformation("Student Misbehaviour job has already run today. Skipping execution.");
            return;
        }

        _logger.LogInformation("Running student misbehaviour job at {Time}", now);

        await DoWork();
        context.JobDetail.JobDataMap.Put("last_run", now);
        _logger.LogInformation("Student misbehaviour job completed successfully.");
    }

    private async Task DoWork()
    {
        var today = DateOnly.FromDateTime(DateTime.Now);
        var schultag = await _schuljahrService.GetSchultagAsync(today);
        var lastDayWithBlocks = await _schuljahrService.GetLastDayWithBlocksAsync(today) == today;

        if (schultag is null || schultag.Blocks.Count == 0)
        {
            _logger.LogInformation("No blocks found for today ({Date})", today);
            return;
        }

        var todaysEnrollments = await _dbContext.OtiaEinschreibungen
            .Where(e => schultag.Blocks.Contains(e.Termin.Block))
            .Include(e => e.BetroffenePerson)
            .Include(e => e.Termin)
            .ThenInclude(t => t.Block)
            .Include(e => e.Termin)
            .ThenInclude(t => t.Otium)
            .ThenInclude(o => o.Kategorie)
            .GroupBy(e => e.BetroffenePerson.Id)
            .ToDictionaryAsync(e => e.Key, e => e.ToList());
        var students = await _userService.GetUsersWithRoleAsync(Rolle.Mittelstufe);

        List<Schultag> schultageInWeek = [];
        Dictionary<Guid, List<OtiumEinschreibung>> weeksEnrollments = [];

        if (lastDayWithBlocks)
        {
            var startOfWeek = today.GetStartOfWeek();
            var endOfWeek = startOfWeek.AddDays(7);
            schultageInWeek = await _dbContext.Schultage
                .Include(s => s.Blocks).Where(s => s.Datum >= startOfWeek && s.Datum < endOfWeek)
                .ToListAsync();
            weeksEnrollments = await _dbContext.OtiaEinschreibungen
                .Where(e => schultageInWeek.Contains(e.Termin.Block.Schultag))
                .Include(e => e.BetroffenePerson)
                .Include(e => e.Termin)
                .ThenInclude(t => t.Block)
                .Include(e => e.Termin)
                .ThenInclude(t => t.Otium)
                .ThenInclude(o => o.Kategorie)
                .GroupBy(e => e.BetroffenePerson.Id)
                .ToDictionaryAsync(e => e.Key, e => e.ToList());
        }

        foreach (var student in students)
        {
            var studentsEnrollments = todaysEnrollments.GetValueOrDefault(student.Id, []);
            List<string> messages = [];

            messages.AddRange(
                await _rulesValidationService.GetMessagesForEnrollmentsAsync(student, studentsEnrollments));
            messages.AddRange(
                await _rulesValidationService.GetMessagesForDayAsync(student, schultag, studentsEnrollments));
            if (lastDayWithBlocks)
            {
                var studentsEnrollmentsInWeek = weeksEnrollments.GetValueOrDefault(student.Id, []);
                messages.AddRange(await _rulesValidationService.GetMessagesForWeekAsync(student, schultageInWeek,
                    studentsEnrollmentsInWeek));
            }

            if (messages.Count == 0) continue;

            // Send E-Mail
            var contentBuilder = new StringBuilder();
            contentBuilder.AppendLine("Die Afra-App hat im Bezug auf Ihren Mentee folgendes festgestellt:");
            foreach (var message in messages)
                contentBuilder.AppendLine($"  - {message}");

            var subject = $"{student.Vorname} {student.Nachname}: Information zum Otium";
            var body = contentBuilder.ToString();

            var mentoren = await _userService.GetMentorsAsync(student);
            foreach (var mentor in mentoren)
                await _emailOutbox.ScheduleNotificationAsync(mentor, subject, body, TimeSpan.FromMinutes(10));
        }
    }
}

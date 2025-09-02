using Afra_App.Otium.Domain.Contracts.Services;
using Afra_App.Otium.Domain.Models;
using Afra_App.Schuljahr.Domain.Models;
using Afra_App.User.Domain.Models;

namespace Afra_App.Otium.Services;

/// <summary>
///     A service to validate rules for enrollments.
/// </summary>
public class RulesValidationService
{
    private readonly IRulesFactory _rulesFactory;

    ///
    public RulesValidationService(IRulesFactory rulesFactory)
    {
        _rulesFactory = rulesFactory;
    }

    /// <summary>
    ///     Validates all independent rules for the given user and enrollments and returns a list of messages for any
    ///     violations.
    /// </summary>
    public async Task<List<string>> GetMessagesForEnrollmentsAsync(Person user,
        List<OtiumEinschreibung> einschreibungen)
    {
        List<string> messages = [];
        var independentRules = _rulesFactory.GetIndependentRules();
        foreach (var enrollment in einschreibungen)
            foreach (var rule in independentRules)
            {
                var result = await rule.IsValidAsync(user, enrollment);
                if (!result.IsValid)
                    messages.AddRange(result.Messages);
            }

        return messages;
    }

    /// <summary>
    ///     Validates all week rules for the given user, school days and enrollments and returns a list of messages for any
    ///     violations.
    /// </summary>
    public async Task<List<string>> GetMessagesForWeekAsync(Person user, List<Schultag> schultage,
        List<OtiumEinschreibung> einschreibungen)
    {
        List<string> messages = [];
        var weekRules = _rulesFactory.GetWeekRules();
        foreach (var rule in weekRules)
        {
            var result = await rule.IsValidAsync(user, schultage, einschreibungen);
            if (!result.IsValid)
                messages.AddRange(result.Messages);
        }

        return messages;
    }

    /// <summary>
    ///     Validates all block rules for the given user, school day and enrollments and returns a list of messages for any
    ///     violations.
    /// </summary>
    public async Task<List<string>> GetMessagesForDayAsync(Person user, Schultag schultag,
        List<OtiumEinschreibung> einschreibungen)
    {
        List<string> messages = [];
        var orderedBlocks = schultag.Blocks.OrderBy(b => b.SchemaId).ToList();
        var blockRules = _rulesFactory.GetBlockRules();
        foreach (var rule in blockRules)
            foreach (var block in orderedBlocks)
            {
                var result = await rule.IsValidAsync(user, block, einschreibungen.Where(e => e.Termin.Block == block));
                if (!result.IsValid)
                    messages.AddRange(result.Messages);
            }

        return messages;
    }
}

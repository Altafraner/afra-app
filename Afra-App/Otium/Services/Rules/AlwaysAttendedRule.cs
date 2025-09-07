using System.ComponentModel;
using Afra_App.Otium.Domain.Contracts.Rules;
using Afra_App.Otium.Domain.Contracts.Services;
using Afra_App.Otium.Domain.Models;
using Afra_App.Schuljahr.Domain.Models;
using Afra_App.User.Domain.Models;

namespace Afra_App.Otium.Services.Rules;

/// <summary>
///     Checks that the person always attended
/// </summary>
public class AlwaysAttendedRule : IBlockRule
{
    private readonly IAttendanceService _attendanceService;
    private readonly BlockHelper _blockHelper;

    ///
    public AlwaysAttendedRule(BlockHelper blockHelper, IAttendanceService attendanceService)
    {
        _blockHelper = blockHelper;
        _attendanceService = attendanceService;
    }

    /// <inheritdoc />
    public async ValueTask<RuleStatus> IsValidAsync(Person person, Block block,
        IEnumerable<OtiumEinschreibung> einschreibungen)
    {
        var now = DateTime.Now;
        var today = DateOnly.FromDateTime(now);
        var time = TimeOnly.FromDateTime(now);

        var blockSchema = _blockHelper.Get(block.SchemaId)!;
        if (block.SchultagKey > today || (block.SchultagKey == today && blockSchema.Interval.End >= time) ||
            (!blockSchema.Verpflichtend && !einschreibungen.Any()))
            return RuleStatus.Valid;

        var attendance = await _attendanceService.GetAttendanceForStudentInBlockAsync(block.Id, person.Id);
        return attendance switch
        {
            OtiumAnwesenheitsStatus.Anwesend => RuleStatus.Valid,
            OtiumAnwesenheitsStatus.Entschuldigt => RuleStatus.Valid with { IgnoreOtherRules = true },
            OtiumAnwesenheitsStatus.Fehlend => RuleStatus.Invalid(
                $"Unentschuldigtes Fehlen im Block „{blockSchema.Bezeichnung}“"),
            _ => throw new InvalidEnumArgumentException("Unrecognized attendance status")
        };
    }
}

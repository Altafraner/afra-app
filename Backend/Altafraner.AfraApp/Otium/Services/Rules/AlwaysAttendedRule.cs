using System.ComponentModel;
using Altafraner.AfraApp.Attendance.Domain.Contracts;
using Altafraner.AfraApp.Attendance.Domain.Models;
using Altafraner.AfraApp.Otium.Domain.Contracts.Rules;
using Altafraner.AfraApp.Otium.Domain.Models;
using Altafraner.AfraApp.Schuljahr.Domain.Models;
using Altafraner.AfraApp.User.Domain.Models;

namespace Altafraner.AfraApp.Otium.Services.Rules;

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
        var blockSchema = _blockHelper.Get(block.SchemaId)!;
        if (_blockHelper.GetBlockStatus(block) is BlockHelper.BlockStatus.Running or BlockHelper.BlockStatus.Pending ||
            (!blockSchema.Verpflichtend && !einschreibungen.Any()))
            return RuleStatus.Valid;

        var attendance =
            await _attendanceService.GetAttendanceForStudentInSlotAsync(OtiumAttendanceInformationProvider.ScopeValue,
                block.Id,
                person.Id);
        return attendance switch
        {
            AttendanceState.Anwesend => RuleStatus.Valid,
            AttendanceState.Entschuldigt => RuleStatus.Valid with { IgnoreOtherRules = true },
            AttendanceState.Fehlend => RuleStatus.Invalid(
                $"Unentschuldigtes Fehlen im Block „{blockSchema.Bezeichnung}“"),
            _ => throw new InvalidEnumArgumentException("Unrecognized attendance status")
        };
    }
}

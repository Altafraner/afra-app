using System.ComponentModel;
using Altafraner.AfraApp.Attendance.Domain.Dto;
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
    private readonly BlockHelper _blockHelper;

    ///
    public AlwaysAttendedRule(BlockHelper blockHelper)
    {
        _blockHelper = blockHelper;
    }

    /// <inheritdoc />
    public ValueTask<RuleStatus> IsValidAsync(Person person,
        Block block,
        IEnumerable<OtiumEinschreibung> einschreibungen,
        AttendanceInformation attendance)
    {
        var blockSchema = _blockHelper.Get(block.SchemaId)!;
        if (_blockHelper.GetBlockStatus(block) is BlockHelper.BlockStatus.Running or BlockHelper.BlockStatus.Pending ||
            (!BlockHelper.IsRequiredToAttendBlock(blockSchema, person) && !einschreibungen.Any()))
            return new ValueTask<RuleStatus>(RuleStatus.Valid);

        return attendance.State switch
        {
            AttendanceState.Anwesend => new ValueTask<RuleStatus>(RuleStatus.Valid),
            AttendanceState.Entschuldigt =>
                new ValueTask<RuleStatus>(RuleStatus.Valid with { IgnoreOtherRules = true }),
            AttendanceState.Fehlend => new ValueTask<RuleStatus>(RuleStatus.Invalid(
                $"Unentschuldigtes Fehlen im Block „{blockSchema.Bezeichnung}“")),
            _ => throw new InvalidEnumArgumentException("Unrecognized attendance status")
        };
    }
}

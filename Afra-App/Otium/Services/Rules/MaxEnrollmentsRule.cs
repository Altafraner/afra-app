using Afra_App.Otium.Domain.Contracts.Rules;
using Afra_App.Otium.Domain.Models;
using Afra_App.User.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Afra_App.Otium.Services.Rules;

/// <summary>
///     Checks if the termin is not full.
/// </summary>
public class MaxEnrollmentsRule : IIndependentRule
{
    private readonly AfraAppContext _dbContext;

    ///
    public MaxEnrollmentsRule(AfraAppContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <inheritdoc />
    public async ValueTask<RuleStatus> MayEnrollAsync(Person person, OtiumTermin termin)
    {
        var countEnrolled = await _dbContext.OtiaEinschreibungen.AsNoTracking()
            .CountAsync(e => e.Termin == termin);
        return termin.MaxEinschreibungen is null || countEnrolled < termin.MaxEinschreibungen
            ? RuleStatus.Valid
            : RuleStatus.Invalid("Der Termin ist bereits voll.");
    }

    /// <inheritdoc />
    public ValueTask<RuleStatus> IsValidAsync(Person person, OtiumEinschreibung enrollment)
    {
        return new ValueTask<RuleStatus>(RuleStatus.Valid);
    }

    /// <inheritdoc />
    public ValueTask<RuleStatus> MayUnenrollAsync(Person person, OtiumTermin termin)
    {
        return new ValueTask<RuleStatus>(RuleStatus.Valid);
    }
}

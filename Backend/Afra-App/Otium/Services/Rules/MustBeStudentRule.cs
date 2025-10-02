using Altafraner.AfraApp.Otium.Domain.Contracts.Rules;
using Altafraner.AfraApp.Otium.Domain.Models;
using Altafraner.AfraApp.User.Domain.Models;

namespace Altafraner.AfraApp.Otium.Services.Rules;

/// <summary>
///     Checks if the person is a student (not a tutor).
/// </summary>
public class MustBeStudentRule : IIndependentRule
{
    /// <inheritdoc />
    public ValueTask<RuleStatus> IsValidAsync(Person person, OtiumEinschreibung enrollment)
    {
        return new ValueTask<RuleStatus>(Rule(person));
    }

    /// <inheritdoc />
    public ValueTask<RuleStatus> MayEnrollAsync(Person person, OtiumTermin termin)
    {
        return new ValueTask<RuleStatus>(Rule(person));
    }

    /// <inheritdoc />
    public ValueTask<RuleStatus> MayUnenrollAsync(Person person, OtiumEinschreibung einschreibung)
    {
        return new ValueTask<RuleStatus>(Rule(person));
    }

    private static RuleStatus Rule(Person person)
    {
        return IsTutor(person)
            ? RuleStatus.Invalid("Nur Schüler:innen können sich einschreiben.")
            : RuleStatus.Valid;
    }

    private static bool IsTutor(Person person)
    {
        return person.Rolle == Rolle.Tutor;
    }
}

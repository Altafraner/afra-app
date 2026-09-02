using System.Diagnostics.CodeAnalysis;
using Altafraner.AfraApp.Profundum.Domain.Models;

namespace Altafraner.AfraApp.Profundum.Domain.DTO;

/// <summary>
///     A dto representing a concrete date+time occurrence (Termin) of a Profundum Slot.
/// </summary>
public record DTOProfundumTermin
{
    ///
    [SetsRequiredMembers]
    public DTOProfundumTermin(ProfundumTermin dbTermin)
    {
        Day = dbTermin.Day;
        StartTime = dbTermin.StartTime;
        EndTime = dbTermin.EndTime;
    }

    ///
    public DTOProfundumTermin()
    {
    }

    /// <inheritdoc cref="ProfundumTermin.Day"/>
    public required DateOnly Day { get; set; }

    /// <inheritdoc cref="ProfundumTermin.StartTime"/>
    public required TimeOnly StartTime { get; set; }

    /// <inheritdoc cref="ProfundumTermin.EndTime"/>
    public required TimeOnly EndTime { get; set; }
}

/// <summary>
///     A request to create or update a Profundum Termin for a given Slot.
/// </summary>
public record DTOProfundumTerminCreation
{
    /// <inheritdoc cref="ProfundumTermin.Day"/>
    public required DateOnly Day { get; set; }

    /// <inheritdoc cref="ProfundumTermin.StartTime"/>
    public required TimeOnly StartTime { get; set; }

    /// <inheritdoc cref="ProfundumTermin.EndTime"/>
    public required TimeOnly EndTime { get; set; }
}

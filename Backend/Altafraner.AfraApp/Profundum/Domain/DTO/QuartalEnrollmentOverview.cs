using Altafraner.AfraApp.Profundum.Domain.Models;

namespace Altafraner.AfraApp.Profundum.Domain.DTO;

/// <summary>
///     A quick dto for transmitting enrollments to all profunda in a quartal
/// </summary>
public record QuartalEnrollmentOverview
{
    /// <summary>
    ///     Creates a dto from db models
    /// </summary>
    public QuartalEnrollmentOverview(ProfundumSlot quartal, IEnumerable<ProfundumInstanz> profunda)
    {
        Label = quartal.ToString();
        Profunda = profunda.Select(p => new ProfundumEnrollmentOverview(p));
    }

    /// <summary>
    ///     the quartals label
    /// </summary>
    public string Label { get; set; }

    /// <summary>
    ///     all profunda in the quartal
    /// </summary>
    public IEnumerable<ProfundumEnrollmentOverview> Profunda { get; set; }
}

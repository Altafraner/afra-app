using Altafraner.AfraApp.Profundum.Domain.Models;
using Altafraner.AfraApp.User.Domain.DTO;

namespace Altafraner.AfraApp.Profundum.Domain.DTO;

/// <summary>
///     A quick dto for transmitting enrollments into a profundum
/// </summary>
public record ProfundumEnrollmentOverview
{
    /// <summary>
    ///     Creates a dto from the db model
    /// </summary>
    public ProfundumEnrollmentOverview(ProfundumInstanz instanz)
    {
        Id = instanz.Id;
        Label = instanz.Profundum.Bezeichnung;
        Students = instanz.Einschreibungen.Select(e => new PersonInfoMinimal(e.BetroffenePerson));
    }

    /// <summary>
    ///     The profundum instances id
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    ///     The profundums label
    /// </summary>
    public string Label { get; set; }

    /// <summary>
    ///     The students enrolled to the profundum
    /// </summary>
    public IEnumerable<PersonInfoMinimal> Students { get; set; }
}

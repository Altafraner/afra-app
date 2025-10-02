using System.ComponentModel.DataAnnotations;
using Altafraner.AfraApp.User.Domain.Models;

namespace Altafraner.AfraApp.Otium.Domain.Models;

/// <summary>
///     An abstract class representing the basic structure of an instance for an Otium. Here to ensure consistency between
///     recurrent and single instances.
/// </summary>
public abstract class OtiumInstanz
{
    /// <summary>
    ///     A reference to the Otium this instance is for.
    /// </summary>
    public required OtiumDefinition Otium { get; set; }

    /// <summary>
    ///     A reference to the tutor of the Otium. Could be a student or a teacher.
    /// </summary>
    public required Person? Tutor { get; set; }

    /// <summary>
    ///     Maximum number of concurrent enrollments for the Otium. If null, no limit is set.
    /// </summary>
    public int? MaxEinschreibungen { get; set; } = null;

    /// <summary>
    ///     The location for the Otium.
    /// </summary>
    [MaxLength(20)]
    public required string Ort { get; set; }
}

using System.ComponentModel.DataAnnotations;
using Altafraner.AfraApp.Otium.Domain.Models;
using Altafraner.AfraApp.Profundum.Domain.Models;
using Altafraner.Backbone.EmailSchedulingModule.Contracts;

namespace Altafraner.AfraApp.User.Domain.Models;

/// <summary>
///     A record representing a person using the application.
/// </summary>
/// <remarks>Usually provided by an external directory service and cached for performance and convenience.</remarks>
public class Person : IEmailRecipient
{
    /// <summary>
    ///     The unique identifier of the person.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    ///     The first name of the person.
    /// </summary>
    [MaxLength(50)]
    public required string FirstName { get; set; }

    /// <summary>
    ///     The last name of the person.
    /// </summary>
    [MaxLength(50)]
    public required string LastName { get; set; }

    /// <summary>
    ///     The email address of the person. Used for communication.
    /// </summary>
    [EmailAddress]
    [MaxLength(150)]
    public required string Email { get; set; }

    /// <summary>
    ///     The mentors of the person. Only used if the person is a student.
    /// </summary>
    public ICollection<Person> Mentors { get; set; } = new List<Person>();

    /// <summary>
    ///     A collection of the mentees of the person. Only used if the person is a teacher.
    /// </summary>
    public ICollection<Person> Mentees { get; set; } = new List<Person>();

    /// <summary>
    ///     The role of the person.
    /// </summary>
    public required Rolle Rolle { get; set; }

    /// <summary>
    ///     A group the person belongs to, e.g. a class.
    /// </summary>
    [MaxLength(100)]
    public string? Gruppe { get; set; }

    /// <summary>
    ///     A list of all global permissions the person has.
    /// </summary>
    public ICollection<GlobalPermission> GlobalPermissions { get; set; } = new List<GlobalPermission>();

    /// <summary>
    ///     The ObjectGuid of the person in the LDAP directory.
    /// </summary>
    public Guid? LdapObjectId { get; set; }

    /// <summary>
    ///     The time the person was last synchronized with the LDAP directory.
    /// </summary>
    public DateTime? LdapSyncTime { get; set; }

    /// <summary>
    ///     The time the first LDAP sync failed for this person. Gets reset when the sync is successful again.
    /// </summary>
    public DateTime? LdapSyncFailureTime { get; set; }

    /// <summary>
    ///     A list of all Otia the person is responsible for.
    /// </summary>
    public ICollection<OtiumDefinition> VerwalteteOtia { get; set; } = new List<OtiumDefinition>();

    /// <summary>
    ///     A list of all Otia the person is enrolled in.
    /// </summary>
    public ICollection<OtiumEinschreibung> OtiaEinschreibungen { get; set; } = new List<OtiumEinschreibung>();

    ///
    public ICollection<ProfundumEinschreibung> ProfundaEinschreibungen { get; set; } = [];

    ///
    public ICollection<ProfundumBelegWunsch> ProfundaBelegwuensche { get; set; } = [];

    /// <inheritdoc />
    public override string ToString()
    {
        return $"{FirstName} {LastName}";
    }
}

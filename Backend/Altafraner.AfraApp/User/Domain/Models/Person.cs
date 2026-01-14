using System.ComponentModel.DataAnnotations;
using Altafraner.AfraApp.Otium.Domain.Models;
using Altafraner.AfraApp.Profundum.Domain.Models;
using Altafraner.Backbone.EmailSchedulingModule;

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
    public List<Person> Mentors { get; set; } = null!;

    /// <summary>
    ///     A collection of the mentees of the person. Only used if the person is a teacher.
    /// </summary>
    public List<Person> Mentees { get; set; } = null!;

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
    public List<GlobalPermission> GlobalPermissions { get; set; } = [];

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
    public List<OtiumDefinition> VerwalteteOtia { get; set; } = null!;

    /// <summary>
    ///     A list of all Otia the person is enrolled in.
    /// </summary>
    public List<OtiumEinschreibung> OtiaEinschreibungen { get; set; } = null!;

    ///
    public List<ProfundumEinschreibung> ProfundaEinschreibungen { get; set; } = null!;

    ///
    public List<ProfundumBelegWunsch> ProfundaBelegwuensche { get; set; } = null!;

    ///
    public List<ProfundumInstanz> BetreuteProfunda { get; set; } = null!;

    /// <inheritdoc />
    public override string ToString()
    {
        return $"{FirstName} {LastName}";
    }
}

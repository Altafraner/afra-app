using Altafraner.AfraApp.User.Domain.Models;
using Altafraner.Backbone.Utils;

namespace Altafraner.AfraApp.Profundum.Domain.Models;

/// <summary>
///     An outstanding invitation for a mutual team-partner pairing (see <see cref="ProfundumPartnerWunsch" />). The
///     <see cref="Token" /> - not <see cref="Id" /> - is the bearer secret shared out-of-band (e.g. read aloud or
///     copy-pasted) between the two students.
/// </summary>
public class ProfundumPartnerEinladung : IHasTimestamps
{
    /// <summary>
    ///     Internal primary key. Not shared with students - see <see cref="Token" />.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    ///     The human-shareable invite token (lower kebab-case, e.g. "apfel-baum-schnee"), generated from the
    ///     configured Partnerwahl wordlist. This is the bearer secret shared out-of-band between the two students.
    /// </summary>
    public required string Token { get; set; }

    /// <summary>
    ///     The Profundum-Definition (topic) this invite is for.
    /// </summary>
    public required ProfundumDefinition ProfundumDefinition { get; set; }

    /// <summary>
    ///     the id of <see cref="ProfundumDefinition" />
    /// </summary>
    protected internal Guid ProfundumDefinitionId { get; set; }

    /// <summary>
    ///     The submission window this invite belongs to. Scoping to the Einwahlzeitraum keeps a stale invite from a
    ///     prior semester from resurrecting if the same topic runs again later.
    /// </summary>
    public required ProfundumEinwahlZeitraum EinwahlZeitraum { get; set; }

    /// <summary>
    ///     the id of <see cref="EinwahlZeitraum" />
    /// </summary>
    protected internal Guid EinwahlZeitraumId { get; set; }

    /// <summary>
    ///     The student who created this invite.
    /// </summary>
    public required Person InitiatorPerson { get; set; }

    /// <summary>
    ///     the id of <see cref="InitiatorPerson" />
    /// </summary>
    protected internal Guid InitiatorPersonId { get; set; }

    /// <inheritdoc/>
    public DateTime CreatedAt { get; set; }

    /// <inheritdoc/>
    public DateTime LastModified { get; set; }
}

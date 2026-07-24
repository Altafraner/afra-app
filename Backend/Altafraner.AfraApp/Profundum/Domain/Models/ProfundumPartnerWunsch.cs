using Altafraner.AfraApp.User.Domain.Models;
using Altafraner.Backbone.Utils;

namespace Altafraner.AfraApp.Profundum.Domain.Models;

/// <summary>
///     A confirmed mutual team-partner pairing for a Profundum-Definition, created once a second student redeems a
///     <see cref="ProfundumPartnerEinladung" />. The matching solver forces both students onto the same Instanz (or
///     neither) for this Definition - see <c>PartnerPairingRule</c>.
/// </summary>
public class ProfundumPartnerWunsch : IHasTimestamps
{
    /// <summary>
    ///     A unique identifier for this pairing.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    ///     The Profundum-Definition (topic) this pairing is for.
    /// </summary>
    public required ProfundumDefinition ProfundumDefinition { get; set; }

    /// <summary>
    ///     the id of <see cref="ProfundumDefinition" />
    /// </summary>
    protected internal Guid ProfundumDefinitionId { get; set; }

    /// <summary>
    ///     The submission window this pairing belongs to.
    /// </summary>
    public required ProfundumEinwahlZeitraum EinwahlZeitraum { get; set; }

    /// <summary>
    ///     the id of <see cref="EinwahlZeitraum" />
    /// </summary>
    protected internal Guid EinwahlZeitraumId { get; set; }

    /// <summary>
    ///     One of the two paired students. Unordered with <see cref="PersonB" /> - which one is "A" vs. "B" has no
    ///     meaning.
    /// </summary>
    public required Person PersonA { get; set; }

    /// <summary>
    ///     the id of <see cref="PersonA" />
    /// </summary>
    protected internal Guid PersonAId { get; set; }

    /// <summary>
    ///     The other paired student.
    /// </summary>
    public required Person PersonB { get; set; }

    /// <summary>
    ///     the id of <see cref="PersonB" />
    /// </summary>
    protected internal Guid PersonBId { get; set; }

    /// <inheritdoc/>
    public DateTime CreatedAt { get; set; }

    /// <inheritdoc/>
    public DateTime LastModified { get; set; }
}

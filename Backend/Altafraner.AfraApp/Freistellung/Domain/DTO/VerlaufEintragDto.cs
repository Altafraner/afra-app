using System.Text.Json.Serialization;
using Altafraner.AfraApp.Freistellung.Domain.Models;
using Altafraner.AfraApp.User.Domain.DTO;

namespace Altafraner.AfraApp.Freistellung.Domain.DTO;

/// <summary>
///     A single entry in a leave request's status history.
/// </summary>
public record VerlaufEintragDto
{
    /// <summary>
    ///     Constructs a DTO from a domain model.
    /// </summary>
    public VerlaufEintragDto(FreistellungsVerlaufEintrag eintrag)
    {
        Zeitpunkt = eintrag.Zeitpunkt;
        Person = eintrag.Person is null ? null : new PersonInfoMinimal(eintrag.Person);
        NeuerStatus = eintrag.NeuerStatus;
        Kommentar = eintrag.Kommentar;
    }

    /// <inheritdoc cref="FreistellungsVerlaufEintrag.Zeitpunkt" />
    public DateTime Zeitpunkt { get; init; }

    /// <summary>
    ///     The person who caused this transition, or <c>null</c> if it happened automatically.
    /// </summary>
    public PersonInfoMinimal? Person { get; init; }

    /// <inheritdoc cref="FreistellungsVerlaufEintrag.NeuerStatus" />
    [JsonConverter(typeof(JsonStringEnumConverter<FreistellungsStatus>))]
    public FreistellungsStatus NeuerStatus { get; init; }

    /// <inheritdoc cref="FreistellungsVerlaufEintrag.Kommentar" />
    public string? Kommentar { get; init; }
}

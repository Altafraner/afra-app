using System.Text.Json.Serialization;
using Altafraner.AfraApp.Freistellung.Domain.Models;
using Altafraner.AfraApp.User.Domain.DTO;

namespace Altafraner.AfraApp.Freistellung.Domain.DTO;

/// <summary>
///     A representation of a teacher's decision on a leave request.
/// </summary>
public record LehrerEntscheidungDto
{
    /// <summary>
    ///     Constructs a DTO from a domain model.
    /// </summary>
    public LehrerEntscheidungDto(Models.LehrerEntscheidung entscheidung)
    {
        Id = entscheidung.Id;
        Lehrer = new PersonInfoMinimal(entscheidung.Lehrer);
        Status = entscheidung.Status;
        Kommentar = entscheidung.Kommentar;
        EntschiedenAm = entscheidung.EntschiedenAm;
    }

    /// <inheritdoc cref="Models.LehrerEntscheidung.Id" />
    public Guid Id { get; init; }

    /// <summary>
    ///     The teacher who made this decision.
    /// </summary>
    public PersonInfoMinimal Lehrer { get; init; }

    /// <inheritdoc cref="Models.LehrerEntscheidung.Status" />
    [JsonConverter(typeof(JsonStringEnumConverter<EntscheidungsStatus>))]
    public EntscheidungsStatus Status { get; init; }

    /// <inheritdoc cref="Models.LehrerEntscheidung.Kommentar" />
    public string? Kommentar { get; init; }

    /// <inheritdoc cref="Models.LehrerEntscheidung.EntschiedenAm" />
    public DateTime? EntschiedenAm { get; init; }
}

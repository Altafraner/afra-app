using System.Text.Json.Serialization;
using Altafraner.AfraApp.Freistellung.Domain.Models;
using Altafraner.AfraApp.User.Domain.DTO;

namespace Altafraner.AfraApp.Freistellung.Domain.DTO;

/// <summary>
///     A summary of a <see cref="Models.Freistellungsantrag" />.
/// </summary>
public record FreistellungsantragDto
{
    /// <summary>
    ///     Constructs a DTO from a domain model.
    /// </summary>
    public FreistellungsantragDto(Models.Freistellungsantrag antrag)
    {
        Id = antrag.Id;
        DatumVon = antrag.DatumVon;
        DatumBis = antrag.DatumBis;
        Grund = antrag.Grund;
        Status = antrag.Status;
        ErstelltAm = antrag.ErstelltAm;
        Student = new PersonInfoMinimal(antrag.Student);
        Entscheidungen = antrag.Entscheidungen
            .Select(e => new LehrerEntscheidungDto(e))
            .ToList();
    }

    /// <inheritdoc cref="Models.Freistellungsantrag.Id" />
    public Guid Id { get; init; }

    /// <inheritdoc cref="Models.Freistellungsantrag.DatumVon" />
    public DateOnly DatumVon { get; init; }

    /// <inheritdoc cref="Models.Freistellungsantrag.DatumBis" />
    public DateOnly DatumBis { get; init; }

    /// <inheritdoc cref="Models.Freistellungsantrag.Grund" />
    public string Grund { get; init; }

    /// <inheritdoc cref="Models.Freistellungsantrag.Status" />
    [JsonConverter(typeof(JsonStringEnumConverter<FreistellungsStatus>))]
    public FreistellungsStatus Status { get; init; }

    /// <inheritdoc cref="Models.Freistellungsantrag.ErstelltAm" />
    public DateTime ErstelltAm { get; init; }

    /// <summary>
    ///     The student who submitted this request.
    /// </summary>
    public PersonInfoMinimal Student { get; init; }

    /// <summary>
    ///     The teacher decisions associated with this request.
    /// </summary>
    public List<LehrerEntscheidungDto> Entscheidungen { get; init; }
}

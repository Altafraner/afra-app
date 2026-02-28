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
    public FreistellungsantragDto(Models.Freistellungsantrag antrag, FreistellungsStatistikDto statistik)
    {
        Id = antrag.Id;
        Grund = antrag.Grund;
        Von = antrag.Von;
        Bis = antrag.Bis;
        Beschreibung = antrag.Beschreibung;
        Status = antrag.Status;
        ErstelltAm = antrag.ErstelltAm;
        Student = new PersonInfoMinimal(antrag.Student);
        ElternbestaetigungErforderlich = antrag.ElternbestaetigungErforderlich;
        ElternbestaetigungVorhanden = antrag.ElternbestaetigungVorhanden;
        Statistik = statistik;
        BetroffeneStunden = antrag.BetroffeneStunden
            .OrderBy(s => s.Datum)
            .ThenBy(s => s.Block)
            .Select(s => new BetroffeneStundeDto(s))
            .ToList();
        Entscheidungen = antrag.Entscheidungen
            .Select(e => new LehrerEntscheidungDto(e))
            .ToList();
        Verlauf = antrag.Verlauf
            .OrderBy(v => v.Zeitpunkt)
            .Select(v => new VerlaufEintragDto(v))
            .ToList();

        // Convenience accessors for the two Verlauf comments the UI needs to surface prominently
        // while they are still actionable — resolved once the request has moved on.
        ElternbestaetigungHinweis = Status == FreistellungsStatus.WartetAufEltern
            ? Verlauf.LastOrDefault(v => v.NeuerStatus == FreistellungsStatus.WartetAufEltern)?.Kommentar
            : null;
        SchulleiterKommentar = Status == FreistellungsStatus.Abgelehnt
            ? Verlauf.LastOrDefault(v => v.NeuerStatus == FreistellungsStatus.Abgelehnt)?.Kommentar
            : null;
    }

    /// <inheritdoc cref="Models.Freistellungsantrag.Id" />
    public Guid Id { get; init; }

    /// <inheritdoc cref="Models.Freistellungsantrag.Grund" />
    public string Grund { get; init; }

    /// <inheritdoc cref="Models.Freistellungsantrag.Von" />
    public DateTime Von { get; init; }

    /// <inheritdoc cref="Models.Freistellungsantrag.Bis" />
    public DateTime Bis { get; init; }

    /// <inheritdoc cref="Models.Freistellungsantrag.Beschreibung" />
    public string Beschreibung { get; init; }

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
    ///     The individual lessons the student will miss during the leave period.
    /// </summary>
    public List<BetroffeneStundeDto> BetroffeneStunden { get; init; }

    /// <summary>
    ///     The decisions associated with this request (both teachers and mentors).
    /// </summary>
    public List<LehrerEntscheidungDto> Entscheidungen { get; init; }

    /// <summary>
    ///     The full, ordered status history of this request.
    /// </summary>
    public List<VerlaufEintragDto> Verlauf { get; init; }

    /// <inheritdoc cref="Models.Freistellungsantrag.ElternbestaetigungErforderlich" />
    public bool? ElternbestaetigungErforderlich { get; init; }

    /// <inheritdoc cref="Models.Freistellungsantrag.ElternbestaetigungVorhanden" />
    public bool ElternbestaetigungVorhanden { get; init; }

    /// <summary>
    ///     The Sekretariat's hint about what is still missing, while the request is still waiting
    ///     on the student to act (<c>null</c> once resolved).
    /// </summary>
    public string? ElternbestaetigungHinweis { get; init; }

    /// <summary>
    ///     The Schulleiter's rejection comment, while the request is still in the rejected state
    ///     (<c>null</c> once reversed).
    /// </summary>
    public string? SchulleiterKommentar { get; init; }

    /// <summary>
    ///     The student's leave-request tally for the current Schuljahr.
    /// </summary>
    public FreistellungsStatistikDto Statistik { get; init; }
}

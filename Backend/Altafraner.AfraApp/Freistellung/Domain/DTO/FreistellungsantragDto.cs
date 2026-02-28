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
        ElternbestaetigungHinweis = antrag.ElternbestaetigungHinweis;
        InCevexEingetragen = antrag.InCevexEingetragen;
        SchulleiterKommentar = antrag.SchulleiterKommentar;
        Statistik = statistik;
        BetroffeneStunden = antrag.BetroffeneStunden
            .OrderBy(s => s.Datum)
            .ThenBy(s => s.Block)
            .Select(s => new BetroffeneStundeDto(s))
            .ToList();
        Entscheidungen = antrag.Entscheidungen
            .Select(e => new LehrerEntscheidungDto(e))
            .ToList();
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

    /// <inheritdoc cref="Models.Freistellungsantrag.ElternbestaetigungErforderlich" />
    public bool? ElternbestaetigungErforderlich { get; init; }

    /// <inheritdoc cref="Models.Freistellungsantrag.ElternbestaetigungVorhanden" />
    public bool ElternbestaetigungVorhanden { get; init; }

    /// <inheritdoc cref="Models.Freistellungsantrag.ElternbestaetigungHinweis" />
    public string? ElternbestaetigungHinweis { get; init; }

    /// <inheritdoc cref="Models.Freistellungsantrag.InCevexEingetragen" />
    public bool InCevexEingetragen { get; init; }

    /// <inheritdoc cref="Models.Freistellungsantrag.SchulleiterKommentar" />
    public string? SchulleiterKommentar { get; init; }

    /// <summary>
    ///     The student's leave-request tally for the current Schuljahr.
    /// </summary>
    public FreistellungsStatistikDto Statistik { get; init; }
}

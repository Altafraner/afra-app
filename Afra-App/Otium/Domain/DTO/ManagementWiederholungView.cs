using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using Afra_App.Otium.Domain.Models;
using Afra_App.Otium.Domain.Models.Schuljahr;
using Afra_App.User.Domain.DTO;

namespace Afra_App.Otium.Domain.DTO;

/// <summary>
///     A DTO for the view of a Wiederholung in the management ui
/// </summary>
public record ManagementWiederholungView
{
    /// <summary>
    ///     Construct an empty ManagementWiederholungView
    /// </summary>
    public ManagementWiederholungView()
    {
    }

    /// <summary>
    ///     Construct a ManagementWiederholungView from a Database Wiederholung
    /// </summary>
    [SetsRequiredMembers]
    public ManagementWiederholungView(Wiederholung dbWiederholung)
    {
        Id = dbWiederholung.Id;
        OtiumId = dbWiederholung.Otium.Id;
        Tutor = dbWiederholung.Tutor is not null ? new PersonInfoMinimal(dbWiederholung.Tutor) : null;
        Ort = dbWiederholung.Ort;
        Wochentag = dbWiederholung.Wochentag;
        Wochentyp = dbWiederholung.Wochentyp;
        StartDate = dbWiederholung.StartDate;
        EndDate = dbWiederholung.EndDate;
        Block = dbWiederholung.Block;
    }

    /// <summary>
    ///     The Id the wiederholung
    /// </summary>
    public required Guid Id { get; set; }

    /// <summary>
    ///     The Id of the otium the wiederholung belongs to
    /// </summary>
    public required Guid OtiumId { get; set; }

    /// <summary>
    ///     The Information on the tutor of the Otium. Could be a student or a teacher.
    /// </summary>
    public required PersonInfoMinimal? Tutor { get; set; }

    /// <summary>
    ///     The location for the Otium.
    /// </summary>
    public required string Ort { get; set; }

    /// <summary>
    ///     The number of the Block the Wiederholung is on.
    /// </summary>
    public required char Block { get; set; }

    /// <summary>
    ///     The Day of the Week that Termine of the Wiederholung are scheduled
    /// </summary>
    [JsonConverter(typeof(JsonNumberEnumConverter<DayOfWeek>))]
    public required DayOfWeek Wochentag { get; set; }

    /// <summary>
    ///     The Type of Week that Termine of the Wiederholung are scheduled
    /// </summary>
    public required Wochentyp Wochentyp { get; set; }

    /// <summary>
    ///     The date of the first Termin
    /// </summary>
    public DateOnly? StartDate { get; set; }

    /// <summary>
    ///     The date of the Last Termin
    /// </summary>
    public DateOnly? EndDate { get; set; }
}
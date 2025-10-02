using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using Altafraner.AfraApp.Otium.Domain.Models;
using Altafraner.AfraApp.Schuljahr.Domain.Models;
using Altafraner.AfraApp.User.Domain.DTO;

namespace Altafraner.AfraApp.Otium.Domain.DTO;

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
    public ManagementWiederholungView(OtiumWiederholung dbWiederholung, string block)
    {
        Id = dbWiederholung.Id;
        OtiumId = dbWiederholung.Otium.Id;
        Tutor = dbWiederholung.Tutor is not null ? new PersonInfoMinimal(dbWiederholung.Tutor) : null;
        Ort = dbWiederholung.Ort;
        Wochentag = dbWiederholung.Wochentag;
        Wochentyp = dbWiederholung.Wochentyp;
        StartDate = dbWiederholung.StartDate;
        EndDate = dbWiederholung.EndDate;
        BlockSchemaId = dbWiederholung.Block;
        Block = block;
        MaxEinschreibungen = dbWiederholung.MaxEinschreibungen;
    }

    /// <summary>
    ///     The ID of the wiederholung
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
    ///     The maximum number of enrollments.
    /// </summary>
    public required int? MaxEinschreibungen { get; set; }

    /// <summary>
    ///     The name of the Block the Wiederholung is in.
    /// </summary>
    public required string Block { get; set; }

    /// <summary>
    ///     The schemaID of the Block the Wiederholung is in.
    /// </summary>
    public required char BlockSchemaId { get; set; }

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

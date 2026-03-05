using System.Text.Json.Serialization;

namespace Altafraner.AfraApp.Freistellung.Domain.Models;

/// <summary>
///     The status of a <see cref="Freistellungsantrag" />
/// </summary>
public enum FreistellungsStatus
{
    /// <summary>
    ///     The request has been submitted and is waiting for teacher and mentor decisions.
    /// </summary>
    [JsonStringEnumMemberName("Gestellt")] Gestellt,

    /// <summary>
    ///     All teachers and mentors have approved the request. Waiting for Sekretariat confirmation.
    /// </summary>
    [JsonStringEnumMemberName("AlleLehrerGenehmigt")]
    AlleLehrerGenehmigt,

    /// <summary>
    ///     At least one teacher or mentor has denied the request.
    /// </summary>
    [JsonStringEnumMemberName("Abgelehnt")] Abgelehnt,

    /// <summary>
    ///     The Sekretariat has confirmed the request. Waiting for Schulleiter approval.
    /// </summary>
    [JsonStringEnumMemberName("Bestaetigt")] Bestaetigt,

    /// <summary>
    ///     The Schulleiter has given final approval for the request.
    /// </summary>
    [JsonStringEnumMemberName("SchulleiterBestaetigt")]
    SchulleiterBestaetigt
}

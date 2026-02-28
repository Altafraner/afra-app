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
    ///     All teachers and mentors have made a decision (approval or rejection does not matter —
    ///     only the Schulleiter can reject a request). Waiting for the Sekretariat to decide whether
    ///     a Elternbestätigung (parental confirmation) is required.
    /// </summary>
    [JsonStringEnumMemberName("BeiSekretariat")] BeiSekretariat,

    /// <summary>
    ///     The Sekretariat has determined that a Elternbestätigung is required but not yet present.
    ///     Waiting for the student to submit it.
    /// </summary>
    [JsonStringEnumMemberName("ElternbestaetigungAusstehend")]
    ElternbestaetigungAusstehend,

    /// <summary>
    ///     The student has indicated that the missing Elternbestätigung has been provided.
    ///     Waiting for the Sekretariat to confirm this and forward the request to the Schulleiter.
    /// </summary>
    [JsonStringEnumMemberName("ElternbestaetigungEingereicht")]
    ElternbestaetigungEingereicht,

    /// <summary>
    ///     The Sekretariat has forwarded the request. Waiting for the Schulleiter's final decision.
    /// </summary>
    [JsonStringEnumMemberName("BeimSchulleiter")] BeimSchulleiter,

    /// <summary>
    ///     The Schulleiter has given final approval for the request. The Sekretariat still needs to
    ///     enter the leave into Cevex (see <see cref="Freistellungsantrag.InCevexEingetragen" />).
    /// </summary>
    [JsonStringEnumMemberName("SchulleiterBestaetigt")]
    SchulleiterBestaetigt,

    /// <summary>
    ///     The Schulleiter has rejected the request. Only the Schulleiter may set this status,
    ///     and only the Schulleiter can revert it (e.g. if the rejection was a mistake).
    /// </summary>
    [JsonStringEnumMemberName("Abgelehnt")] Abgelehnt
}

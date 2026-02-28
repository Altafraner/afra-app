using System.Text.Json.Serialization;

namespace Altafraner.AfraApp.Freistellung.Domain.Models;

/// <summary>
///     The status of a <see cref="Freistellungsantrag" />. Every value here is a distinct, fully
///     self-describing phase of the workflow — there is no auxiliary flag on
///     <see cref="Freistellungsantrag" /> whose value is needed to know what state a request is
///     actually in.
/// </summary>
public enum FreistellungsStatus
{
    /// <summary>
    ///     The request has been submitted and is waiting for teacher and mentor decisions.
    /// </summary>
    [JsonStringEnumMemberName("Eingereicht")] Eingereicht,

    /// <summary>
    ///     All teachers and mentors have given their assessment (approval or objection does not
    ///     matter — only the Schulleiter can reject a request). Waiting for the Sekretariat to
    ///     decide whether a Elternbestätigung (parental confirmation) is required.
    /// </summary>
    [JsonStringEnumMemberName("BeiSekretariat")] BeiSekretariat,

    /// <summary>
    ///     The Sekretariat has determined that a Elternbestätigung is required but not yet present.
    ///     Waiting for the student to submit it.
    /// </summary>
    [JsonStringEnumMemberName("WartetAufEltern")] WartetAufEltern,

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
    ///     The Schulleiter has rejected the request. Only the Schulleiter may set this status,
    ///     and only the Schulleiter can revert it (e.g. if the rejection was a mistake).
    /// </summary>
    [JsonStringEnumMemberName("Abgelehnt")] Abgelehnt,

    /// <summary>
    ///     The Schulleiter has given final approval. The Sekretariat still needs to enter the leave
    ///     into Cevex before the request is <see cref="Abgeschlossen" />.
    /// </summary>
    [JsonStringEnumMemberName("Genehmigt")] Genehmigt,

    /// <summary>
    ///     The Schulleiter approved the request and the Sekretariat has entered it into Cevex.
    ///     Terminal state.
    /// </summary>
    [JsonStringEnumMemberName("Abgeschlossen")] Abgeschlossen
}

using System.Text.Json.Serialization;

namespace Altafraner.AfraApp.Freistellung.Domain.Models;

/// <summary>
///     The decision status of a <see cref="LehrerEntscheidung" />
/// </summary>
public enum EntscheidungsStatus
{
    /// <summary>
    ///     The teacher has not yet made a decision.
    /// </summary>
    [JsonStringEnumMemberName("Ausstehend")] Ausstehend,

    /// <summary>
    ///     The teacher has approved the request.
    /// </summary>
    [JsonStringEnumMemberName("Genehmigt")] Genehmigt,

    /// <summary>
    ///     The teacher has denied the request.
    /// </summary>
    [JsonStringEnumMemberName("Abgelehnt")] Abgelehnt
}

using System.Text.Json.Serialization;

namespace Altafraner.AfraApp.Profundum.Domain.Models;

/// <summary>
///     A preference level for wishes
/// </summary>
public enum ProfundumBelegWunschStufe
{
    /// <summary>
    ///     high preference
    /// </summary>
    [JsonStringEnumMemberName("ErstWunsch")]
    ErstWunsch = 1,

    /// <summary>
    ///     medium preference
    /// </summary>
    [JsonStringEnumMemberName("Zweitwunsch")]
    ZweitWunsch = 2,

    /// <summary>
    ///     low preference
    /// </summary>
    [JsonStringEnumMemberName("Drittwunsch")]
    DrittWunsch = 3,
}

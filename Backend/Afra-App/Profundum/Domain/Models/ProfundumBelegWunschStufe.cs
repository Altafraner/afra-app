using System.Text.Json.Serialization;

///
public enum ProfundumBelegWunschStufe
{
    ///
    [JsonStringEnumMemberName("ErstWunsch")]
    ErstWunsch = 1,
    ///
    [JsonStringEnumMemberName("Zweitwunsch")]
    ZweitWunsch = 2,
    ///
    [JsonStringEnumMemberName("Drittwunsch")]
    DrittWunsch = 3,
}


using System.Text.Json.Serialization;

///
public enum BelegWunschStufe
{
    ///
    [JsonStringEnumMemberName("ErstWunsch")]
    ErstWunsch = 1,
    ///
    [JsonStringEnumMemberName("Zweitwunsch")]
    Zweitwunsch = 2,
    ///
    [JsonStringEnumMemberName("Drittwunsch")]
    DrittWunsch = 3,
}


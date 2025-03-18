namespace Afra_App.Data.DTO.Otium;

public record LehrerUebersicht(IEnumerable<LehrerTerminPreview> termine, IEnumerable<MenteePreview> mentees);

public record LehrerTerminPreview(Guid Id, string Otium, string Ort, int? Auslastung, DateOnly Datum, byte block);

public record MenteePreview(
    PersonInfoMinimal mentee,
    MenteePreviewStatus letzteWoche,
    MenteePreviewStatus dieseWoche,
    MenteePreviewStatus nächsteWoche);

public enum MenteePreviewStatus
{
    Okay,
    Auffaellig
}
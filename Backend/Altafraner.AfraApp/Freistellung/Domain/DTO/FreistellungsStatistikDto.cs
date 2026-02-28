namespace Altafraner.AfraApp.Freistellung.Domain.DTO;

/// <summary>
///     A student's leave-request tally for the current Schuljahr, counting only requests that
///     have received final Schulleiter approval.
/// </summary>
/// <param name="AnzahlAntraegeSchuljahr">The number of approved leave requests this Schuljahr.</param>
/// <param name="AnzahlStundenSchuljahr">The number of missed lesson-hours this Schuljahr.</param>
public record FreistellungsStatistikDto(int AnzahlAntraegeSchuljahr, int AnzahlStundenSchuljahr);

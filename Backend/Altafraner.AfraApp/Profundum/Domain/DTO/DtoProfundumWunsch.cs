namespace Altafraner.AfraApp.Profundum.Domain.DTO;

/// <summary>
///     A dto that represents a students enrollment wish
/// </summary>
/// <param name="Id">The id of the wished ProfundumDefinition</param>
/// <param name="Rang">the rank (1 = most preferred) this wish has</param>
/// <param name="UnprocessedRang">The rank as the student entered it before processing</param>
public record struct DtoProfundumWunsch(Guid Id, int Rang, int UnprocessedRang);

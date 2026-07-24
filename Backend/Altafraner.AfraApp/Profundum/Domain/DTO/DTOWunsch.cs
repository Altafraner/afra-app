namespace Altafraner.AfraApp.Profundum.Domain.DTO;

/// <summary>
///     A dto that represents a students enrollment wish
/// </summary>
/// <param name="Id">The id of the wished ProfundumDefinition</param>
/// <param name="SlotId">the ids of the slots any Instanz of this Profundum occupies</param>
/// <param name="Rang">the rank (1 = most preferred) this wish has</param>
public record struct DTOWunsch(Guid Id, IEnumerable<Guid> SlotId, int Rang);

namespace Altafraner.AfraApp.Profundum.Domain.DTO;

/// <summary>
///     A dto that represents a students enrollment wish
/// </summary>
/// <param name="Id">The wish's id</param>
/// <param name="SlotId">the slots ids</param>
/// <param name="Rang">the rank (1-3) this wish has</param>
public record struct DTOWunsch(Guid Id, IEnumerable<Guid> SlotId, int Rang);

using Altafraner.AfraApp.User.Domain.DTO;

namespace Altafraner.AfraApp.Otium.Domain.DTO;

/// <summary>
/// A DTO that represents the minimal information on a termin
/// </summary>
/// <param name="Id">The id of the termin</param>
/// <param name="Otium">The name of the Otium the termin is for</param>
/// <param name="Tutor">Information about the tutor tutoring the termin</param>
/// <param name="Ort">The location where the termin is happening</param>
public record MinimalTermin(Guid Id, string Otium, PersonInfoMinimal? Tutor, string Ort);

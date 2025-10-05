namespace Altafraner.AfraApp.Otium.Domain.DTO.Katalog;

/// <summary>
/// A DTO representing a day for the katalog.
/// </summary>
/// <param name="Termine">Previews for all termine on the day</param>
/// <param name="Hinweise">A list with messages to help the user know, what he must choose to comply with regulations.</param>
public record Tag(IAsyncEnumerable<TerminPreview> Termine, IEnumerable<string> Hinweise);

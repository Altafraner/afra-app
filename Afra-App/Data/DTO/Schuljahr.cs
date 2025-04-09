using Afra_App.Data.Schuljahr;

namespace Afra_App.Data.DTO;

/// <summary>
///     A DTO for sending schooldays to the client.
/// </summary>
/// <param name="Standard">The schoolday that should be displayed by default</param>
/// <param name="Schultage">A list of all schooldays</param>
public record Schuljahr(Schultag Standard, IEnumerable<Schultag> Schultage);

/// <summary>
/// A DTO for sending schooldays to the client.
/// </summary>
public record Schultag(DateOnly Datum, Wochentyp Wochentyp, IEnumerable<sbyte> Blocks);
using Altafraner.AfraApp.Schuljahr.Domain.Models;

namespace Altafraner.AfraApp.Schuljahr.Domain.DTO;

/// <summary>
///     A DTO for sending schooldays to the client.
/// </summary>
/// <param name="Standard">The schoolday that should be displayed by default</param>
/// <param name="Schultage">A list of all schooldays</param>
public record Schuljahr(Schultag? Standard, IEnumerable<Schultag> Schultage);

/// <summary>
///     A DTO for sending schooldays to the client.
/// </summary>
public record Schultag(DateOnly Datum, Wochentyp Wochentyp, IEnumerable<BlockSchema> Blocks);

/// <summary>
///     A DTO for creating a new school day.
/// </summary>
public record SchultagCreation(DateOnly Datum, Wochentyp Wochentyp, IEnumerable<char> Blocks);

/// <summary>
///     A DTO for sending the schema of a block to the client.
/// </summary>
public record BlockSchema(char SchemaId, string Bezeichnung);

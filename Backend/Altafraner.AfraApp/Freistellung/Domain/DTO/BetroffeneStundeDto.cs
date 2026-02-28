using Altafraner.AfraApp.Freistellung.Domain.Models;
using Altafraner.AfraApp.User.Domain.DTO;

namespace Altafraner.AfraApp.Freistellung.Domain.DTO;

/// <summary>
///     A representation of a single missed lesson within a leave request.
/// </summary>
public record BetroffeneStundeDto
{
    /// <summary>
    ///     Constructs a DTO from a domain model.
    /// </summary>
    public BetroffeneStundeDto(Models.BetroffeneStunde stunde)
    {
        Id = stunde.Id;
        Datum = stunde.Datum;
        Block = stunde.Block;
        Fach = stunde.Fach;
        Lehrer = new PersonInfoMinimal(stunde.Lehrer);
    }

    /// <inheritdoc cref="Models.BetroffeneStunde.Id" />
    public Guid Id { get; init; }

    /// <inheritdoc cref="Models.BetroffeneStunde.Datum" />
    public DateOnly Datum { get; init; }

    /// <inheritdoc cref="Models.BetroffeneStunde.Block" />
    public int Block { get; init; }

    /// <inheritdoc cref="Models.BetroffeneStunde.Fach" />
    public string Fach { get; init; }

    /// <summary>
    ///     The teacher who teaches this lesson.
    /// </summary>
    public PersonInfoMinimal Lehrer { get; init; }
}

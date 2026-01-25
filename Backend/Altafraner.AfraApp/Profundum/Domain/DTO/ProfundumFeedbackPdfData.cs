using System.Collections.Immutable;
using Altafraner.AfraApp.User.Domain.DTO;

namespace Altafraner.AfraApp.Profundum.Domain.DTO;

internal class ProfundumFeedbackPdfData
{
    public required MetaData Meta { get; init; }
    public required PersonInfoMinimal Person { get; init; }
    public required IEnumerable<Profundum> Profunda { get; init; }
    public required ImmutableSortedDictionary<string, Dictionary<string, int[]>> Feedback { get; init; }

    internal record MetaData(string Datum, int Schuljahr);

    internal record Profundum(string Label, IEnumerable<PersonInfoMinimal> Verantwortliche);
}

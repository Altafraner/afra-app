using System.Collections.Immutable;
using Altafraner.AfraApp.User.Domain.DTO;

namespace Altafraner.AfraApp.Profundum.Domain.DTO;

internal class ProfundumFeedbackPdfData
{
    public required MetaData Meta { get; init; }
    public required PersonInfoMinimal Person { get; init; }

    // ReSharper disable once InconsistentNaming
    public required PersonInfoMinimal? GM { get; init; }
    public required PersonInfoMinimal? Schulleiter { get; init; }
    public required IEnumerable<Profundum> Profunda { get; init; }
    public required ImmutableSortedDictionary<string, Dictionary<string, int[]>> FeedbackAllgemein { get; init; }
    public required ImmutableSortedDictionary<string, Dictionary<string, int[]>> FeedbackFachlich { get; init; }

    internal record MetaData(string Datum, int Schuljahr);

    internal record Profundum(string Label, IEnumerable<PersonInfoMinimal> Verantwortliche);
}

using System.Diagnostics.CodeAnalysis;
using Altafraner.AfraApp.Profundum.Domain.Models;

namespace Altafraner.AfraApp.Profundum.Domain.DTO;

/// <summary>
/// A DTO for a fachbereich
/// </summary>
public class DtoProfundumFachbereich
{
    [SetsRequiredMembers]
    internal DtoProfundumFachbereich(ProfundumFachbereich dbEntity)
    {
        Id = dbEntity.Id;
        Label = dbEntity.Label;
    }

    ///
    public DtoProfundumFachbereich()
    {
    }

    /// <summary>
    /// The unique id of the fachbereich
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// The human-readable label of the fachbereich.
    /// </summary>
    public required string Label { get; set; }
}

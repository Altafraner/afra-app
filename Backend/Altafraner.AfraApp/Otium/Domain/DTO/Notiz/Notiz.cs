using System.Diagnostics.CodeAnalysis;
using Altafraner.AfraApp.Otium.Domain.Models;
using Altafraner.AfraApp.User.Domain.DTO;

namespace Altafraner.AfraApp.Otium.Domain.DTO.Notiz;

/// <summary>
///     A DTO representing an attendance note
/// </summary>
public record Notiz
{
    /// <summary>
    ///     The time the note was created
    /// </summary>
    public required DateTime Created { get; set; }

    /// <summary>
    ///     The time the note was changed
    /// </summary>
    public required DateTime Changed { get; set; }

    /// <summary>
    ///     The notes content
    /// </summary>
    public required string Content { get; set; }

    /// <summary>
    ///     The person who created the note
    /// </summary>
    public required PersonInfoMinimal Creator { get; set; }

    ///
    public Notiz()
    {
    }

    [SetsRequiredMembers]
    internal Notiz(OtiumAnwesenheitsNotiz notiz)
    {
        Created = notiz.CreatedAt;
        Changed = notiz.LastModified;
        Content = notiz.Content;
        Creator = new PersonInfoMinimal(notiz.Author);
    }
}

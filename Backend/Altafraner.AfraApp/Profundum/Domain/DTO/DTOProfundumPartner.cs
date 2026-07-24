using System.Diagnostics.CodeAnalysis;
using Altafraner.AfraApp.Profundum.Domain.Models;
using Altafraner.AfraApp.User.Domain.DTO;

namespace Altafraner.AfraApp.Profundum.Domain.DTO;

/// <summary>
///     An outstanding team-partner invite the current student created.
/// </summary>
public record DTOProfundumPartnerEinladung
{
    ///
    [SetsRequiredMembers]
    public DTOProfundumPartnerEinladung(ProfundumPartnerEinladung dbEinladung)
    {
        Token = dbEinladung.Token;
        ProfundumDefinitionId = dbEinladung.ProfundumDefinitionId;
        Bezeichnung = dbEinladung.ProfundumDefinition.Bezeichnung;
        CreatedAt = dbEinladung.CreatedAt;
    }

    /// <inheritdoc cref="ProfundumPartnerEinladung.Token"/>
    public required string Token { get; set; }

    /// <inheritdoc cref="ProfundumPartnerEinladung.ProfundumDefinition"/>
    public required Guid ProfundumDefinitionId { get; set; }

    /// <inheritdoc cref="ProfundumDefinition.Bezeichnung"/>
    public required string Bezeichnung { get; set; }

    /// <inheritdoc cref="ProfundumPartnerEinladung.CreatedAt"/>
    public DateTime CreatedAt { get; set; }
}

/// <summary>
///     A confirmed team-partner pairing, as seen by one of the two paired students.
/// </summary>
public record DTOProfundumPartnerWunsch
{
    ///
    [SetsRequiredMembers]
    public DTOProfundumPartnerWunsch(ProfundumPartnerWunsch dbWunsch, Guid selfId)
    {
        Id = dbWunsch.Id;
        ProfundumDefinitionId = dbWunsch.ProfundumDefinitionId;
        Bezeichnung = dbWunsch.ProfundumDefinition.Bezeichnung;
        Partner = new PersonInfoMinimal(dbWunsch.PersonAId == selfId ? dbWunsch.PersonB : dbWunsch.PersonA);
    }

    /// <inheritdoc cref="ProfundumPartnerWunsch.Id"/>
    public required Guid Id { get; set; }

    /// <inheritdoc cref="ProfundumPartnerWunsch.ProfundumDefinition"/>
    public required Guid ProfundumDefinitionId { get; set; }

    /// <inheritdoc cref="ProfundumDefinition.Bezeichnung"/>
    public required string Bezeichnung { get; set; }

    /// <summary>
    ///     The other paired student.
    /// </summary>
    public required PersonInfoMinimal Partner { get; set; }
}

/// <summary>
///     A confirmed team-partner pairing, as seen by staff.
/// </summary>
public record DTOProfundumPartnerWunschStaff
{
    ///
    [SetsRequiredMembers]
    public DTOProfundumPartnerWunschStaff(ProfundumPartnerWunsch dbWunsch)
    {
        Id = dbWunsch.Id;
        ProfundumDefinitionId = dbWunsch.ProfundumDefinitionId;
        Bezeichnung = dbWunsch.ProfundumDefinition.Bezeichnung;
        PersonA = new PersonInfoMinimal(dbWunsch.PersonA);
        PersonB = new PersonInfoMinimal(dbWunsch.PersonB);
    }

    /// <inheritdoc cref="ProfundumPartnerWunsch.Id"/>
    public required Guid Id { get; set; }

    /// <inheritdoc cref="ProfundumPartnerWunsch.ProfundumDefinition"/>
    public required Guid ProfundumDefinitionId { get; set; }

    /// <inheritdoc cref="ProfundumDefinition.Bezeichnung"/>
    public required string Bezeichnung { get; set; }

    /// <inheritdoc cref="ProfundumPartnerWunsch.PersonA"/>
    public required PersonInfoMinimal PersonA { get; set; }

    /// <inheritdoc cref="ProfundumPartnerWunsch.PersonB"/>
    public required PersonInfoMinimal PersonB { get; set; }
}

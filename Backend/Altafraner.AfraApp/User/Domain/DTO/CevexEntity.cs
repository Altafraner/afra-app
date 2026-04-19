using Altafraner.AfraApp.Attendance.AbsenceProviders.Cevex;

namespace Altafraner.AfraApp.User.Domain.DTO;

/// <summary>
///     A dto for exposing cevex entities
/// </summary>
public record struct CevexEntity
{
    internal CevexEntity(CevexUser c)
    {
        Id = c.Guid;
        LastName = c.Lastname;
        FirstName = c.Firstname;
    }

    ///
    public CevexEntity()
    {
    }

    /// <summary>
    ///     The cevex id
    /// </summary>
    public string? Id { get; set; }

    /// <summary>
    ///     The cevex last name
    /// </summary>
    public string? LastName { get; set; }

    /// <summary>
    ///     the cevex first name
    /// </summary>
    public string? FirstName { get; set; }
}

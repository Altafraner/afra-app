namespace Altafraner.AfraApp.User.Domain.DTO;

/// <summary>
///     A request to change a users cevex id
/// </summary>
public struct CevexChangeRequest
{
    /// <summary>
    ///     The id of the user to change the cevex id for
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    ///     The cevex id to set
    /// </summary>
    public string CevexId { get; set; }
}

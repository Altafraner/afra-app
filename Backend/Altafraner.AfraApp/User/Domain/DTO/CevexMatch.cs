namespace Altafraner.AfraApp.User.Domain.DTO;

/// <summary>
///     Correlates a user to a cevex entity
/// </summary>
public struct CevexMatch
{
    /// <summary>
    ///     The internal user
    /// </summary>
    public PersonInfoMinimal User { get; set; }

    /// <summary>
    ///     The cevex entity
    /// </summary>
    public CevexEntity? Cevex { get; set; }
}

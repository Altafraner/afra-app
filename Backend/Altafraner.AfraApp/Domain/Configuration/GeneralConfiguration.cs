using Altafraner.AfraApp.User.Domain.DTO;

namespace Altafraner.AfraApp.Domain.Configuration;

/// <summary>
///     General information about the app
/// </summary>
public class GeneralConfiguration
{
    /// <summary>
    ///     The current principal
    /// </summary>
    public PersonInfoMinimal Schulleiter { get; init; }
}

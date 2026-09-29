namespace Altafraner.AfraApp.Files;

/// <summary>
///     Contains configuration for the files services
/// </summary>
public class FilesConfiguration
{
    /// <summary>
    ///     The root path of files stored by the files service
    /// </summary>
    public required string RootPath { get; set; }

    /// <summary>
    ///     Validates the config
    /// </summary>
    public static bool Validate(FilesConfiguration config)
    {
        var di = new DirectoryInfo(config.RootPath);
        return !di.Exists ? throw new Exception("Cannot write to files path") : true;
    }
}

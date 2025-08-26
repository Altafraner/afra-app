namespace Afra_App.Backbone.EmergencyBackup.Services.Contracts;

/// <summary>
///     Specifies methods for saving HTML content as part of an emergency backup service.
/// </summary>
public interface IEmergencyBackupService
{
    /// <summary>
    /// Saves the provided HTML content with the specified name to the backup service.
    /// </summary>
    /// <param name="name">The name of the file to save</param>
    /// <param name="content">The files contents</param>
    public Task SaveHtmlAsync(string name, string content);
}

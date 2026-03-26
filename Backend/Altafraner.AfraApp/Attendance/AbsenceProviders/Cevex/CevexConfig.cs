namespace Altafraner.AfraApp.Attendance.AbsenceProviders.Cevex;

/// <summary>
///     Configuration for the cevex absence provider
/// </summary>
public class CevexConfig
{
    /// <summary>
    ///     The path to the cevex export file
    /// </summary>
    public required string FilePath { get; set; }

    /// <summary>
    ///     a list of e-mail addresses that should be notified on a failed sync
    /// </summary>
    public string[] SyncNotificationRecipients { get; set; } = [];
}

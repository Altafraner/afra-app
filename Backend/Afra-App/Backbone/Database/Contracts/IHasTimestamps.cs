namespace Altafraner.AfraApp.Backbone.Database.Contracts;

/// <summary>
///     An interface implemented by Database records to keep created and updated times
/// </summary>
public interface IHasTimestamps
{
    /// <summary>
    ///     The time of creation of the db record (UTC)
    /// </summary>
    DateTime CreatedAt { get; set; }

    /// <summary>
    ///     The last time the other fields were modified (UTC)
    /// </summary>
    DateTime LastModified { get; set; }
}

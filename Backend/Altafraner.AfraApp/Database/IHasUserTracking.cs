namespace Altafraner.AfraApp;

/// <summary>
///     An interface implemented by Database records to keep the user ids responsible for changes
/// </summary>
public interface IHasUserTracking
{
    /// <summary>
    ///     The <see cref="User.Domain.Models.Person.Id"/> of the user responsible for creating this tuple
    /// </summary>
    Guid? CreatedById { get; set; }

    /// <summary>
    ///     The <see cref="User.Domain.Models.Person.Id"/> of the user responsible for last modifying this tuple
    /// </summary>
    Guid? LastModifiedById { get; set; }
}

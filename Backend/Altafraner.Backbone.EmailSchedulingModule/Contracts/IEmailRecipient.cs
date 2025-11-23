using System.ComponentModel.DataAnnotations;

namespace Altafraner.Backbone.EmailSchedulingModule;

/// <summary>
///     A recipient for an e-mail
/// </summary>
public interface IEmailRecipient
{
    /// <summary>
    ///     The recipients unique id
    /// </summary>
    Guid Id { get; }

    /// <summary>
    ///     The recipients first name
    /// </summary>
    string FirstName { get; }

    /// <summary>
    ///     The recipients last name
    /// </summary>
    string LastName { get; }

    /// <summary>
    ///     The recipients E-Mail-Address
    /// </summary>
    [EmailAddress]
    string Email { get; }
}

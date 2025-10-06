using System.ComponentModel.DataAnnotations;

namespace Altafraner.Backbone.EmailSchedulingModule;

public interface IEmailRecipient
{
    public Guid Id { get; }

    public string FirstName { get; }
    public string LastName { get; }

    [EmailAddress] public string Email { get; }
}

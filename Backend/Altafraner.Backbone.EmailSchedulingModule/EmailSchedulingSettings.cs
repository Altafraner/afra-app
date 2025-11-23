using Microsoft.EntityFrameworkCore;

namespace Altafraner.Backbone.EmailSchedulingModule;

/// <summary>
///     Settings-Builder for scheduling e-mail notifications
/// </summary>
/// <typeparam name="TPerson">The data-type representing a user in the current context</typeparam>
public class EmailSchedulingSettings<TPerson> where TPerson : class, IEmailRecipient
{
    internal Type? DbContextType { get; set; }

    /// <summary>
    ///     Registers the DbContextStore to the settings
    /// </summary>
    public EmailSchedulingSettings<TPerson> WithDbContextStore<T>()
        where T : DbContext, IScheduledEmailContext<TPerson>
    {
        DbContextType = typeof(T);
        return this;
    }
}

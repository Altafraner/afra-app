using Altafraner.Backbone.EmailSchedulingModule.Models;
using Microsoft.EntityFrameworkCore;

namespace Altafraner.Backbone.EmailSchedulingModule;

/// <summary>
///     Describes how a db context must be designed to include relevant fields for e-mail notifications
/// </summary>
/// <typeparam name="TPerson"></typeparam>
public interface IScheduledEmailContext<TPerson>
    where TPerson : class, IEmailRecipient
{
    /// <summary>
    ///     E-Mail-Notifications scheduled for later delivery
    /// </summary>
    DbSet<ScheduledEmail<TPerson>> ScheduledEmails { get; }
}

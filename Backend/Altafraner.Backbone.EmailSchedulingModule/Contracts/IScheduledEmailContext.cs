using Altafraner.Backbone.EmailSchedulingModule.Models;
using Microsoft.EntityFrameworkCore;

namespace Altafraner.Backbone.EmailSchedulingModule.Contracts;

public interface IScheduledEmailContext<TPerson> where TPerson : class, IEmailRecipient
{
    public DbSet<ScheduledEmail<TPerson>> ScheduledEmails { get; set; }
}

using Altafraner.Backbone.EmailSchedulingModule.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Altafraner.Backbone.EmailSchedulingModule;

public class EmailSchedulingSettings<TPerson> where TPerson : class, IEmailRecipient
{
    internal Type? DbContextType { get; set; }

    public EmailSchedulingSettings<TPerson> WithDbContextStore<T>()
        where T : DbContext, IScheduledEmailContext<TPerson>
    {
        DbContextType = typeof(T);
        return this;
    }
}

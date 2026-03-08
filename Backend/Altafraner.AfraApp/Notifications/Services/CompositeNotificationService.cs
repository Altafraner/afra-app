using Altafraner.AfraApp.User.Domain.Models;
using Altafraner.Backbone.EmailSchedulingModule;
using Altafraner.Backbone.WebNotifications.Services;
using Microsoft.EntityFrameworkCore;

namespace Altafraner.AfraApp.Notifications.Services;


/// <summary>
///     Wraps the <see cref="IEmailNotificationService" /> and <see cref="IInAppNotificationService{TPerson}" /> to deliver
///     notifications via both channels.  Email delivery respects the per-user
///     <c>ReceiveEmailNotifications</c> preference.
/// </summary>
internal class CompositeNotificationService : INotificationService
{
    private readonly AfraAppContext _db;
    private readonly IEmailNotificationService _email;
    private readonly IInAppNotificationService<Person> _inApp;

    /// <summary>
    ///     Constructs a new <see cref="CompositeNotificationService" />.
    /// </summary>
    public CompositeNotificationService(
        IEmailNotificationService email,
        IInAppNotificationService<Person> inApp,
        AfraAppContext db)
    {
        _email = email;
        _inApp = inApp;
        _db = db;
    }

    /// <inheritdoc />
    public async Task ScheduleNotificationAsync(Guid recipientId, string subject, string body, TimeSpan deadline)
    {
        Console.WriteLine("composite");

        await _inApp.SendInAppNotificationAsync(recipientId, subject, body);

        var wantsEmail = await _db.Personen
            .Where(p => p.Id == recipientId)
            .Select(p => p.ReceiveEmailNotifications)
            .FirstOrDefaultAsync();

        if (wantsEmail)
            await _email.ScheduleNotificationAsync(recipientId, subject, body, deadline);
    }
}

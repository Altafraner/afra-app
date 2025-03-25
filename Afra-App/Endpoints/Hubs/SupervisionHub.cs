using Afra_App.Authentication;
using Afra_App.Data;
using Afra_App.Data.People;
using Afra_App.Services.Otium;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Afra_App.Endpoints.Hubs;

/// <summary>
/// A hub for handling supervision of otia.
/// </summary>
[Authorize]
public class SupervisionHub : Hub<SupervisionHubClient>
{
    private readonly AttendanceService _attendance;
    private readonly AfraAppContext _context;

    /// <inheritdoc />
    public SupervisionHub(AttendanceService attendance, AfraAppContext context)
    {
        _attendance = attendance;
        _context = context;
    }

    /// <inheritdoc />
    public override async Task OnConnectedAsync()
    {
        var user = await Context.GetPersonAsync(_context);
        if (user.Rolle != Rolle.Tutor)
        {
            Context.Abort();
            return;
        }

        await base.OnConnectedAsync();
    }
}
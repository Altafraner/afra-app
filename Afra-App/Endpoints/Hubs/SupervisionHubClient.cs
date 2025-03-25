using Afra_App.Data.DTO.Otium.Supervision;

namespace Afra_App.Endpoints.Hubs;

public abstract class SupervisionHubClient
{
    public abstract Task UpdateAll(IAsyncEnumerable<Attendance> attendances);
    public abstract Task Update(Attendance attendance);
}
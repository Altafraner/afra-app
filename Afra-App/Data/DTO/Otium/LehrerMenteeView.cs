using Afra_App.Data.DTO.Otium.Dashboard;

namespace Afra_App.Data.DTO.Otium;

public record LehrerMenteeView(IAsyncEnumerable<Tag> Termine, PersonInfoMinimal Mentee);
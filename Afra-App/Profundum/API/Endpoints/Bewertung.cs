using Afra_App;
using Afra_App.Backbone.Authentication;
using Afra_App.User.Services;
using Afra_App.Profundum.Domain.DTO;

/// <summary>
///    Contains endpoints for managing Profunda Bewertungen.
/// </summary>
public static class Bewertung
{
    /// <summary>
    ///     Maps the Profundum Bewertung endpoints to the given <see cref="IEndpointRouteBuilder" />.
    /// </summary>
    public static void MapBewertungEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/bewertung");

        group.MapGet("/me", async (
            ProfundumsBewertungService bewertungService,
            UserAccessor userAccessor) =>
        {
            var user = await userAccessor.GetUserAsync();
            var bewertungen = await bewertungService.GetBewertungenAsync(user);
            return Results.Ok(bewertungen);
        })
        .RequireAuthorization(AuthorizationPolicies.StudentOnly);

        group.MapGet("/{personId}", async (
            Guid personId,
            ProfundumsBewertungService bewertungService,
            AfraAppContext dbContext) =>
        {
            var person = await dbContext.Personen.FindAsync(personId);
            if (person == null)
                return Results.NotFound("Person nicht gefunden");

            var bewertungen = await bewertungService.GetBewertungenAsync(person);
            return Results.Ok(bewertungen);
        })
        .RequireAuthorization(AuthorizationPolicies.TutorOnly);

        group.MapPost("/", async (
            ProfundumsBewertungService bewertungService,
            AfraAppContext dbContext,
            List<DTOProfundumBewertung> bewertungenDto) =>
        {
            var bewertungen = new List<ProfundumBewertung>();

            foreach (var dto in bewertungenDto)
            {
                var kriterium = await dbContext.ProfundumsBewertungKriterien.FindAsync(dto.KriteriumId);
                var instanz = await dbContext.ProfundaInstanzen.FindAsync(dto.InstanzId);
                var student = await dbContext.Personen.FindAsync(dto.InstanzId);

                if (kriterium == null || instanz == null || student == null)
                    return Results.BadRequest("Kriterium, Instanz oder betroffene Person nicht gefunden");

                bewertungen.Add(new ProfundumBewertung
                {
                    Id = Guid.NewGuid(),
                    Kriterium = kriterium,
                    Instanz = instanz,
                    BetroffenePerson = student,
                    Grad = dto.Grad
                });
            }

            try
            {
                await bewertungService.AddBewertungenAsync(null!, bewertungen);
            }
            catch (ProfundumsBewertungException e)
            {
                return Results.BadRequest(e.Message);
            }

            return Results.Ok("Feedback gespeichert");
        })
        .RequireAuthorization(AuthorizationPolicies.TutorOnly);
    }
}

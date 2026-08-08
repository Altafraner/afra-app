using Altafraner.AfraApp.Otium.API;
using Altafraner.AfraApp.User.Domain.DTO;
using Altafraner.AfraApp.User.Services;
using Microsoft.EntityFrameworkCore;

namespace Altafraner.AfraApp.User.API.Endpoints;

internal static class Dev
{
    public static void MapDevEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/dev/users",
                async (AfraAppContext dbContext) =>
                {
                    return await dbContext.Personen
                        .GroupBy(e => e.Gruppe ?? e.Rolle.ToString())
                        .ToDictionaryAsync(e => e.Key,
                            e => e
                                .OrderBy(e2 => e2.FirstName)
                                .ThenBy(e2 => e2.LastName)
                                .Select(e2 => new PersonInfoMinimal(e2)));
                })
            .AllowAnonymous();

        app.MapPost("/api/dev/users",
            async (UserSigninService signinService, ValueWrapper<Guid> userId) =>
            {
                await signinService.SignInAsync(userId.Value, false);
            });
    }
}

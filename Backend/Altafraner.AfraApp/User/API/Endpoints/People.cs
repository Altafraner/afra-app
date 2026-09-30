using System.Net.Mime;
using Altafraner.AfraApp.Attendance.AbsenceProviders.Cevex;
using Altafraner.AfraApp.Attendance.Configuration;
using Altafraner.AfraApp.Backbone.Auth;
using Altafraner.AfraApp.User.Domain.DTO;
using Altafraner.AfraApp.User.Domain.Models;
using Altafraner.AfraApp.User.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using SkiaSharp;

namespace Altafraner.AfraApp.User.API.Endpoints;

/// <summary>
/// A class containing extension methods for the people endpoint.
/// </summary>
internal static class People
{
    /// <summary>
    /// Maps endpoints for getting people.
    /// </summary>
    public static void MapPeopleEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/people", GetPeople)
            .WithName("GetPeople")
            .RequireAuthorization(AuthorizationPolicies.TeacherOrAdmin);
        app.MapGet("/api/people/{id:guid}/mentor", GetPersonMentors)
            .WithName("GetPersonMentors")
            .RequireAuthorization(AuthorizationPolicies.TeacherOrAdmin);
        app.MapGet("/api/klassen", GetKlassen)
            .RequireAuthorization();
        app.MapDelete("/api/people/{userId:guid}", DeletePerson)
            .RequireAuthorization(AuthorizationPolicies.AdminOnly);
        app.MapGet("/api/people/{userId:guid}/avatar", GetAvatar)
            .RequireAuthorization(AuthorizationPolicies.TeacherOrAdmin);
        app.MapPost("/api/people/{userId:guid}/avatar", SetAvatar)
            .RequireAuthorization(AuthorizationPolicies.AdminOnly);
        app.MapDelete("/api/people/{userId:guid}/avatar", DeleteAvatar)
            .RequireAuthorization(AuthorizationPolicies.AdminOnly);

        var attendanceConfiguration = app.ServiceProvider.GetService<IOptions<AttendanceConfiguration>>();
        // ReSharper disable once InvertIf
        if (!string.IsNullOrWhiteSpace(attendanceConfiguration?.Value.Cevex?.FilePath))
        {
            app.MapGet("/api/people/cevex", GetCevex)
                .RequireAuthorization(AuthorizationPolicies.AdminOnly);
            app.MapPost("/api/people/cevex", SetCevex)
                .RequireAuthorization(AuthorizationPolicies.AdminOnly);
        }
    }

    private static async Task<NoContent> DeleteAvatar(Guid userId,
        AvatarService avatarService)
    {
        await avatarService.DeleteImageAsync(userId);
        return TypedResults.NoContent();
    }

    private static async Task<Results<BadRequest<string>, NoContent>> SetAvatar(HttpContext context,
        Guid userId,
        AvatarService avatarService,
        CancellationToken token)
    {
        context.Request.EnableBuffering();
        const int boundaryLengthLimit = 70;
        var contentType = context.Request.ContentType;
        if (contentType is null) return TypedResults.BadRequest("Missing Content-Type header");

        if (!(!string.IsNullOrEmpty(contentType)
              && contentType.Contains("multipart/", StringComparison.OrdinalIgnoreCase)))
            return TypedResults.BadRequest("Invalid Content-Type");

        var boundary = HeaderUtilities
            .RemoveQuotes(MediaTypeHeaderValue.Parse(contentType).Boundary)
            .Value;

        if (string.IsNullOrWhiteSpace(boundary)) return TypedResults.BadRequest("Missing content-type boundary.");

        if (boundary.Length > boundaryLengthLimit)
            return TypedResults.BadRequest(
                $"Multipart boundary length limit {boundaryLengthLimit} exceeded.");

        var reader = new MultipartReader(boundary, context.Request.Body);
        var section = await reader.ReadNextSectionAsync(token);

        if (section is null) return TypedResults.BadRequest("No section of multipart message found");

        var hasContentDispositionHeader =
            ContentDispositionHeaderValue.TryParse(
                section.ContentDisposition,
                out var contentDisposition);

        if (!hasContentDispositionHeader || contentDisposition is null ||
            !contentDisposition.IsFileDisposition())
            return TypedResults.BadRequest("Content disposition not properly defined for file upload");

        await avatarService.SaveImageAsync(userId, section.Body);
        return TypedResults.NoContent();
    }

    private static Results<FileStreamHttpResult, BadRequest, NotFound> GetAvatar(Guid userId,
        AvatarService avatarService,
        string size)
    {
        if (size == "original")
        {
            var stream = avatarService.GetOriginalAvatar(userId);
            if (stream is null) return TypedResults.NotFound();

            // Try to recover the mime type. Will return octet-stream if not possible, which will probably result in the browser discarding the image.
            var codec = SKCodec.Create(stream);
            var mimeType = GetMimeType(codec.EncodedFormat);
            stream.Seek(0, SeekOrigin.Begin);

            return TypedResults.Stream(stream, mimeType);
        }

        if (!int.TryParse(size, out var width)) return TypedResults.BadRequest();

        var downsizedStream = avatarService.GetScaledImage(userId, width);
        if (downsizedStream is null) return TypedResults.NotFound();
        return TypedResults.Stream(downsizedStream, AvatarService.ScaledMimeType);
    }

    private static string GetMimeType(SKEncodedImageFormat format)
    {
        return format switch
        {
            SKEncodedImageFormat.Png => MediaTypeNames.Image.Png,
            SKEncodedImageFormat.Jpeg => MediaTypeNames.Image.Jpeg,
            SKEncodedImageFormat.Gif => MediaTypeNames.Image.Gif,
            SKEncodedImageFormat.Webp => MediaTypeNames.Image.Webp,
            SKEncodedImageFormat.Avif => MediaTypeNames.Image.Avif,
            SKEncodedImageFormat.Heif => "image/heif",
            SKEncodedImageFormat.Bmp => MediaTypeNames.Image.Bmp,
            _ => MediaTypeNames.Application.Octet
        };
    }

    private static Ok<IAsyncEnumerable<PersonInfoMinimal>> GetPeople(AfraAppContext dbContext,
        HttpContext httpContext)
    {
        var people = dbContext.Personen
            .Where(p => !p.Deleted)
            .OrderBy(p => p.LastName)
            .ThenBy(p => p.FirstName)
            .Select(p => new PersonInfoMinimal(p))
            .AsAsyncEnumerable();

        return TypedResults.Ok(people);
    }

    private static async Task<IResult> GetPersonMentors(AfraAppContext dbContext, UserService userService, Guid id)
    {
        try
        {
            var student = await userService.GetUserByIdAsync(id);
            var mentors = await userService.GetMentorsAsync(student);
            return Results.Ok(mentors.Select(s => new PersonInfoMinimal(s)));
        }
        catch (KeyNotFoundException)
        {
            return Results.NotFound();
        }
        catch (InvalidOperationException)
        {
            return Results.Ok(new List<PersonInfoMinimal>());
        }
    }

    private static IResult GetKlassen(AfraAppContext dbContext, UserService userService)
    {
        return Results.Ok(userService.GetKlassenstufen());
    }

    private static async Task<IResult> GetCevex(AfraAppContext dbContext,
        CevexDataParser cevexParser)
    {
        var cevexData = (await cevexParser.ReadFile()).ToArray();
        await cevexParser.GetMatches();
        var cevexDict = cevexData.ToDictionary(data => data.Guid);
        var people = await dbContext.Personen
            .AsNoTracking()
            .Where(p => (p.Rolle == Rolle.Mittelstufe || p.Rolle == Rolle.Oberstufe) && !p.Deleted)
            .OrderByDescending(p => p.CevexId == null)
            .ThenBy(p => p.LastName)
            .ThenBy(p => p.FirstName)
            .AsAsyncEnumerable()
            .Select(person => new CevexMatch
            {
                Cevex = person.CevexId is not null && cevexDict.TryGetValue(person.CevexId, out var cevexUser)
                    ? new CevexEntity(cevexUser)
                    : null,
                User = new PersonInfoMinimal(person)
            })
            .ToArrayAsync();
        var usedCevexIds = people
            .Where(p => p.Cevex is not null)
            .Select(p => p.Cevex!.Value.Id!)
            .ToHashSet();
        var missingCevexIds = cevexData.Select(c => c.Guid).ToHashSet();
        missingCevexIds.ExceptWith(usedCevexIds);

        return Results.Ok(new
        {
            Available = missingCevexIds.Select(c => new CevexEntity(cevexDict[c])),
            Matches = people
        });
    }

    private static async Task<IResult> SetCevex(CevexChangeRequest request,
        CevexDataParser cevexParser,
        UserService userService,
        AfraAppContext dbContext)
    {
        if (request.CevexId == "00000-0000000000-AAAAAAA")
            try
            {
                var user = await userService.GetUserByIdAsync(request.UserId);
                user.CevexId = request.CevexId;
                user.CevexIdManuallyEntered = true;
                user.CevexSyncFailureTime = DateTime.UtcNow;
                dbContext.Update(user);
                await dbContext.SaveChangesAsync();
                return Results.NoContent();
            }
            catch (InvalidOperationException)
            {
                return Results.NotFound();
            }

        var cevexData = (await cevexParser.ReadFile()).ToArray();
        var cevexDict = cevexData.ToDictionary(data => data.Guid);
        var usedIds = await dbContext.Personen
            .Where(p => (p.Rolle == Rolle.Mittelstufe || p.Rolle == Rolle.Oberstufe) && p.CevexId != null)
            .Select(p => p.CevexId!)
            .ToHashSetAsync();
        if (usedIds.Contains(request.CevexId)) return Results.Conflict();
        if (!cevexDict.ContainsKey(request.CevexId)) return Results.NotFound();
        try
        {
            var user = await userService.GetUserByIdAsync(request.UserId);
            user.CevexId = request.CevexId;
            user.CevexIdManuallyEntered = true;
            user.CevexSyncFailureTime = null;
            dbContext.Update(user);
            await dbContext.SaveChangesAsync();
            return Results.NoContent();
        }
        catch (InvalidOperationException)
        {
            return Results.NotFound();
        }
    }

    private static async Task<Results<NoContent, Conflict>> DeletePerson(Guid userId, UserService userService)
    {
        var user = await userService.GetUserByIdAsync(userId);
        try
        {
            await userService.SoftDelete(user);
        }
        catch (InvalidOperationException)
        {
            return TypedResults.Conflict();
        }

        return TypedResults.NoContent();
    }
}

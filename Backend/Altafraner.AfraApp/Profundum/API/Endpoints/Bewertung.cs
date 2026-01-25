using System.Net.Mime;
using Altafraner.AfraApp.Backbone.Authorization;
using Altafraner.AfraApp.Profundum.Domain.DTO;
using Altafraner.AfraApp.Profundum.Services;
using Altafraner.AfraApp.User.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Altafraner.AfraApp.Profundum.API.Endpoints;

/// <summary>
///     The Bewertung endpoints for Profundum management.
/// </summary>
public static class Bewertung
{
    /// <summary>
    ///     Maps the Bewertung endpoints
    /// </summary>
    /// <param name="app"></param>
    public static void MapBewertungEndpoints(this IEndpointRouteBuilder app)
    {
        var bewertung = app.MapGroup("/bewertung")
            .RequireAuthorization(AuthorizationPolicies.TutorOnly);

        var anker = bewertung.MapGroup("/anker")
            .RequireAuthorization(AuthorizationPolicies.ProfundumsVerantwortlich);

        anker.MapGet("/", GetAllAnker);
        anker.MapPost("/", AddAnkerAsync);
        anker.MapDelete("/{id:guid}", DeleteAnkerAsync);
        anker.MapPut("/{id:guid}", UpdateAnkerAsync);

        var kategorie = bewertung.MapGroup("/kategorie")
            .RequireAuthorization(AuthorizationPolicies.ProfundumsVerantwortlich);
        kategorie.MapPost("/", AddKategorieAsync);
        kategorie.MapDelete("/{id:guid}", DeleteKategorieAsync);
        kategorie.MapPut("/{id:guid}", UpdateKategorieAsync);

        bewertung.MapGet("/{profundumId:guid}", GetAnkerForProfundum);
        bewertung.MapGet("/{profundumId:guid}/{studentId:guid}", GetBewertungAsync);
        bewertung.MapPut("/{profundumId:guid}/{studentId:guid}", UpdateBewertungAsync);

        bewertung.MapGet("/control/status", GetStatusAsync)
            .RequireAuthorization(AuthorizationPolicies.ProfundumsVerantwortlich);

        bewertung.MapGet("/{userId:guid}.pdf",
            async (FeedbackPrintoutService ProfundumManagementService, Guid userId) =>
            {
                var fileContents = await ProfundumManagementService.GenerateFileForPerson(userId);
                return TypedResults.File(fileContents, MediaTypeNames.Application.Pdf);
            });
    }

    private static async Task<Results<Ok<Anker>, NotFound<HttpValidationProblemDetails>>> AddAnkerAsync(
        AnkerChangeRequest request,
        FeedbackAnkerService ankerService)
    {
        try
        {
            var entry = await ankerService.AddAnker(request.Label, request.KategorieId);
            return TypedResults.Ok(new Anker(entry));
        }
        catch (ArgumentException)
        {
            return TypedResults.NotFound(new HttpValidationProblemDetails(
                new Dictionary<string, string[]>
                {
                    { nameof(request.KategorieId), ["Kategorie not found"] }
                }));
        }
    }

    private static async Task<Results<NoContent, BadRequest<HttpValidationProblemDetails>>> UpdateAnkerAsync(Guid id,
        AnkerChangeRequest request,
        FeedbackAnkerService ankerService)
    {
        try
        {
            await ankerService.UpdateAnker(id, request.Label, request.KategorieId);
            return TypedResults.NoContent();
        }
        catch (ArgumentException e)
        {
            return TypedResults.BadRequest(new HttpValidationProblemDetails(new Dictionary<string, string[]>
            {
                { e.ParamName ?? "unknown", [e.Message] }
            }));
        }
    }

    private static async Task<Results<NoContent, NotFound>> DeleteAnkerAsync(Guid id, FeedbackAnkerService ankerService)
    {
        try
        {
            await ankerService.RemoveAnker(id);
            return TypedResults.NoContent();
        }
        catch (ArgumentException)
        {
            return TypedResults.NotFound();
        }
    }

    private static async Task<Results<Ok<FeedbackCategory>, NotFound<string>>> AddKategorieAsync(
        FeedbackKategorieChangeRequest request,
        FeedbackKategorienService kategorienService)
    {
        try
        {
            var entry = await kategorienService.AddKategorie(request.Label, request.Kategorien);
            return TypedResults.Ok(new FeedbackCategory(entry));
        }
        catch (ArgumentException)
        {
            return TypedResults.NotFound("Kategorie not found");
        }
    }

    private static async Task<Results<NoContent, BadRequest<HttpValidationProblemDetails>>> UpdateKategorieAsync(
        Guid id,
        FeedbackKategorieChangeRequest request,
        FeedbackKategorienService kategorienService)
    {
        try
        {
            await kategorienService.UpdateKategorie(id, request.Label, request.Kategorien);
            return TypedResults.NoContent();
        }
        catch (ArgumentException e)
        {
            return TypedResults.BadRequest(new HttpValidationProblemDetails(new Dictionary<string, string[]>
            {
                { e.ParamName ?? "unknown", [e.Message] }
            }));
        }
    }

    private static async Task<Results<NoContent, NotFound>> DeleteKategorieAsync(Guid id,
        FeedbackKategorienService kategorienService)
    {
        try
        {
            await kategorienService.RemoveKategorie(id);
            return TypedResults.NoContent();
        }
        catch (ArgumentException)
        {
            return TypedResults.NotFound();
        }
    }

    private static async Task<Ok<AnkerOverview>> GetAllAnker(FeedbackAnkerService ankerService,
        FeedbackKategorienService kategorienService)
    {
        var anker = await ankerService.GetAnkerByCategories();
        var kategorien = await kategorienService.GetAllCategories();

        var dto = new AnkerOverview
        {
            AnkerByKategorie = anker
                .ToDictionary(a => a.Key.Id, a => a.Value.Select(e => new Anker(e))),
            Kategorien = kategorien.Select(k => new FeedbackCategory(k))
        };

        return TypedResults.Ok(dto);
    }

    private static async Task<Results<Ok<AnkerOverview>, ForbidHttpResult>> GetAnkerForProfundum(Guid profundumId,
        FeedbackAnkerService ankerService,
        FeedbackKategorienService kategorienService,
        FeedbackService feedbackService,
        UserAccessor userAccessor)
    {
        var user = await userAccessor.GetUserAsync();
        if (!await feedbackService.MayProvideFeedbackForProfundumAsync(user, profundumId))
            return TypedResults.Forbid();

        var anker = await ankerService.GetAnkerByCategories(profundumId);
        var kategorien = await kategorienService.GetAllCategories();

        var dto = new AnkerOverview
        {
            AnkerByKategorie = anker
                .ToDictionary(a => a.Key.Id, a => a.Value.Select(e => new Anker(e))),
            Kategorien = kategorien.Where(k => anker.ContainsKey(k)).Select(k => new FeedbackCategory(k))
        };

        return TypedResults.Ok(dto);
    }

    private static async Task<Results<Ok<Dictionary<Guid, int?>>, ForbidHttpResult>> GetBewertungAsync(Guid studentId,
        Guid profundumId,
        FeedbackService feedbackService,
        UserAccessor userAccessor)
    {
        var user = await userAccessor.GetUserAsync();
        if (!await feedbackService.MayProvideFeedbackForProfundumAsync(user, profundumId))
            return TypedResults.Forbid();

        var feedback = await feedbackService.GetFeedback(studentId, profundumId);
        return TypedResults.Ok(feedback.ToDictionary(f => f.Key.Id, f => f.Value));
    }

    private static async Task<Results<NoContent, ForbidHttpResult, BadRequest<HttpValidationProblemDetails>>>
        UpdateBewertungAsync(Guid studentId,
        Guid profundumId,
        Dictionary<Guid, int?> bewertungen,
        FeedbackService feedbackService,
        UserAccessor userAccessor)
    {
        var user = await userAccessor.GetUserAsync();
        if (!await feedbackService.MayProvideFeedbackForProfundumAsync(user, profundumId))
            return TypedResults.Forbid();
        try
        {
            await feedbackService.UpdateFeedback(studentId,
                profundumId,
                bewertungen.Where(b => b.Value.HasValue)
                    .ToDictionary(f => f.Key, f => f.Value!.Value));

            return TypedResults.NoContent();
        }
        catch (ArgumentException e)
        {
            return TypedResults.BadRequest(new HttpValidationProblemDetails(new Dictionary<string, string[]>
            {
                { e.ParamName ?? "unknown", [e.Message] }
            }));
        }
    }

    private static async Task<Ok<Dictionary<Guid, IEnumerable<FeedbackOverview>>>>
        GetStatusAsync(
            FeedbackService feedbackService,
            ProfundumManagementService managementService)
    {
        var slots = await managementService.GetSlotsAsync();
        var allInstances = await managementService.GetInstanzenAsync();

        var bewertungsStatus = await feedbackService.GetFeedbackStatus().ToArrayAsync();

        var dict = new Dictionary<Guid, IEnumerable<FeedbackOverview>>();

        foreach (var slot in slots)
        {
            var instancesInSlot = allInstances.Where(i => i.Slots.Contains(slot.Id));
            var data = instancesInSlot
                .Where(i => i.MaxEinschreibungen != 0)
                .Select(i => new FeedbackOverview
                {
                    Instanz = i,
                    Slot = slot,
                    Status = bewertungsStatus.Single(e => e.instanz.Id == i.Id).status
                });
            dict.Add(slot.Id, data);
        }

        return TypedResults.Ok(dict);
    }
}

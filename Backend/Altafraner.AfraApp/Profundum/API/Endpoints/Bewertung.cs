using System.Net.Mime;
using Altafraner.AfraApp.Backbone.Authorization;
using Altafraner.AfraApp.Profundum.Domain.DTO;
using Altafraner.AfraApp.Profundum.Domain.Models;
using Altafraner.AfraApp.Profundum.Services;
using Altafraner.AfraApp.User.Domain.DTO;
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
            .RequireAuthorization();

        var anker = bewertung.MapGroup("/anker")
            .RequireAuthorization(AuthorizationPolicies.ProfundumsVerantwortlich);

        var disclose = bewertung.MapGroup("/disclose");

        anker.MapGet("/", GetAllAnker);
        anker.MapPost("/", AddAnkerAsync);
        anker.MapDelete("/{id:guid}", DeleteAnkerAsync);
        anker.MapPut("/{id:guid}", UpdateAnkerAsync);

        var kategorie = bewertung.MapGroup("/kategorie")
            .RequireAuthorization(AuthorizationPolicies.ProfundumsVerantwortlich);
        kategorie.MapPost("/", AddKategorieAsync);
        kategorie.MapDelete("/{id:guid}", DeleteKategorieAsync);
        kategorie.MapPut("/{id:guid}", UpdateKategorieAsync);

        bewertung.MapGet("/{profundumId:guid}", GetAnkerForProfundum)
            .RequireAuthorization(AuthorizationPolicies.TutorOnly);
        bewertung.MapGet("/{instanzId:guid}/{slotId:guid}/{studentId:guid}", GetBewertungAsync)
            .RequireAuthorization(AuthorizationPolicies.TutorOnly);
        bewertung.MapPut("/{instanzId:guid}/{slotId:guid}/{studentId:guid}", UpdateBewertungAsync)
            .RequireAuthorization(AuthorizationPolicies.TutorOnly);

        bewertung.MapGet("/control/status", GetStatusAsync)
            .RequireAuthorization(AuthorizationPolicies.ProfundumsVerantwortlich);

        bewertung.MapGet("/{userId:guid}.pdf",
                async (FeedbackPrintoutService profundumManagementService,
                    Guid userId,
                    UserService userService,
                    int schuljahr,
                    bool halbjahr,
                    DateOnly ausgabedatum) =>
            {
                var user = await userService.GetUserByIdAsync(userId);
                var fileContents =
                    await profundumManagementService.GenerateFileForPerson(user, schuljahr, halbjahr, ausgabedatum);
                return TypedResults.File(fileContents, MediaTypeNames.Application.Pdf);
            })
            .RequireAuthorization(AuthorizationPolicies.ProfundumsVerantwortlich);

        bewertung.MapGet("/batch.zip",
                async (FeedbackPrintoutService profundumManagementService,
                    int schuljahr,
                    bool halbjahr,
                    DateOnly ausgabedatum,
                    bool doublesided,
                    bool byClass = false,
                    bool byGm = false,
                    bool single = false) =>
                {
                    FeedbackPrintoutService.BatchingModes mode = 0;
                    if (byClass) mode |= FeedbackPrintoutService.BatchingModes.ByClass;
                    if (byGm) mode |= FeedbackPrintoutService.BatchingModes.ByGm;
                    if (single) mode |= FeedbackPrintoutService.BatchingModes.Single;
                    if (single && (byClass || byGm))
                        return (Results<FileContentHttpResult, BadRequest<string>>)TypedResults.BadRequest(
                            "If single is set, no other batching method may be selected.");
                    var fileContents =
                        await profundumManagementService.GenerateFileBatched(mode,
                            schuljahr,
                            halbjahr,
                            ausgabedatum,
                            doublesided);
                    return TypedResults.File(fileContents, MediaTypeNames.Application.Zip);
                })
            .RequireAuthorization(AuthorizationPolicies.ProfundumsVerantwortlich);

        disclose.MapGet("/", DiscloseForCurrentUser)
            .RequireAuthorization(AuthorizationPolicies.StudentOnly);
        disclose.MapGet("/{studentId:guid}", DiscloseForMentor)
            .RequireAuthorization(AuthorizationPolicies.TutorOnly);
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
        FeedbackCategoryChangeRequest request,
        FeedbackKategorienService kategorienService)
    {
        try
        {
            var entry = await kategorienService.AddKategorie(request.Label, request.Kategorien, request.IsFachlich);
            return TypedResults.Ok(new FeedbackCategory(entry));
        }
        catch (ArgumentException)
        {
            return TypedResults.NotFound("Kategorie not found");
        }
    }

    private static async Task<Results<NoContent, BadRequest<HttpValidationProblemDetails>>> UpdateKategorieAsync(
        Guid id,
        FeedbackCategoryChangeRequest request,
        FeedbackKategorienService kategorienService)
    {
        try
        {
            await kategorienService.UpdateKategorie(id, request.Label, request.Kategorien, request.IsFachlich);
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
        Guid instanzId,
        Guid slotId,
        FeedbackService feedbackService,
        UserAccessor userAccessor)
    {
        var user = await userAccessor.GetUserAsync();
        if (!await feedbackService.MayProvideFeedbackForProfundumAsync(user, instanzId))
            return TypedResults.Forbid();

        var feedback = await feedbackService.GetFeedback(studentId, instanzId, slotId);
        return TypedResults.Ok(feedback.ToDictionary(f => f.Key.Id, f => f.Value));
    }

    private static async Task<Results<NoContent, ForbidHttpResult, BadRequest<HttpValidationProblemDetails>>>
        UpdateBewertungAsync(Guid studentId,
            Guid instanzId,
            Guid slotId,
        Dictionary<Guid, int?> bewertungen,
        FeedbackService feedbackService,
        UserAccessor userAccessor)
    {
        var user = await userAccessor.GetUserAsync();
        if (!await feedbackService.MayProvideFeedbackForProfundumAsync(user, instanzId))
            return TypedResults.Forbid();
        try
        {
            await feedbackService.UpdateFeedback(studentId,
                instanzId,
                slotId,
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
                    Status = bewertungsStatus.Single(e => e.instanz.Id == i.Id && e.slot.Id == slot.Id).status
                });
            dict.Add(slot.Id, data);
        }

        return TypedResults.Ok(dict);
    }

    private static async Task<Results<Ok<MenteeFeedback>, ForbidHttpResult>> DiscloseForMentor(
        Guid studentId,
        UserService userService,
        UserAccessor userAccessor,
        FeedbackService feedbackService,
        ProfundumManagementService managementService)
    {
        var user = await userAccessor.GetUserAsync();
        var mentees = await userService.GetMenteesAsync(user);
        var mentee = mentees.FirstOrDefault(e => e.Id == studentId);
        if (mentee is null) return TypedResults.Forbid();

        var disclosure = await DiscloseForSpecifiedUser(studentId, feedbackService, managementService);
        return TypedResults.Ok(new MenteeFeedback(new PersonInfoMinimal(mentee), disclosure));
    }

    private static async Task<Ok<StudentFeedbackHierarchie>> DiscloseForCurrentUser(
        UserAccessor userAccessor,
        FeedbackService feedbackService,
        ProfundumManagementService managementService)
    {
        var user = await userAccessor.GetUserAsync();
        return TypedResults.Ok(await DiscloseForSpecifiedUser(user.Id, feedbackService, managementService));
    }

    private static async Task<StudentFeedbackHierarchie> DiscloseForSpecifiedUser(
        Guid userId,
        FeedbackService feedbackService,
        ProfundumManagementService managementService)
    {
        var allSlots = await managementService.GetSlotsAsync();
        var domainFeedback = await feedbackService.GetFeedback(userId, allSlots.Select(s => s.Id)).ToArrayAsync();
        var enrollmentInfos = domainFeedback.Select(f => (
                f.Slot,
                f.Instanz.Profundum.Bezeichnung
            ))
            .ToDictionary(e => e.Slot.Id, e => new FeedbackEnrollmentInfo(new DTOProfundumSlot(e.Slot), e.Bezeichnung));

        var slotComparer = new ProfundumSlotComparer();
        var orderedSlots = domainFeedback.Select(f => f.Slot)
            .Distinct()
            .OrderBy(s => s, slotComparer)
            .ToArray();

        var categories = domainFeedback
            .SelectMany(f => f.Feedback.Keys.Select(a => a.Kategorie))
            .DistinctBy(c => c.Id)
            .OrderByDescending(c => c.IsFachlich)
            .ThenBy(c => c.Label)
            .ToArray();

        var kategorieGroups = new List<FeedbackKategorieGroup>();
        foreach (var kat in categories)
        {
            var anchorsInKat = domainFeedback
                .SelectMany(f => f.Feedback.Keys)
                .Where(a => a.Kategorie.Id == kat.Id)
                .DistinctBy(a => a.Id)
                .OrderBy(a => a.Label)
                .ToArray();

            var anchorGroups = new List<FeedbackAnkerGroup>();
            foreach (var anchor in anchorsInKat)
            {
                var ratings = new Dictionary<Guid, int>();
                foreach (var slot in orderedSlots)
                {
                    var slotFeedback = domainFeedback.FirstOrDefault(f => f.Slot.Id == slot.Id);
                    var rating = slotFeedback.Feedback.FirstOrDefault(e => e.Key.Id == anchor.Id).Value;
                    if (rating is not null) ratings[slot.Id] = rating.Value;
                }

                if (ratings.Count > 0) anchorGroups.Add(new FeedbackAnkerGroup(anchor.Id, anchor.Label, ratings));
            }

            if (anchorGroups.Count > 0)
                kategorieGroups.Add(new FeedbackKategorieGroup(kat.Id, kat.Label, kat.IsFachlich, anchorGroups));
        }

        return new StudentFeedbackHierarchie(enrollmentInfos, kategorieGroups);
    }
}

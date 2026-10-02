namespace Altafraner.AfraApp.Backbone.Caching;

internal static class CachingExtensions
{
    private const int MaxImgCachingLengthSeconds = 24 * 60 * 60;

    public static RouteHandlerBuilder AddImageCachingHeaders(this RouteHandlerBuilder builder)
    {
        return builder.AddEndpointFilter(async (context, next) =>
        {
            var method = context.HttpContext.Request.Method;
            if (!HttpMethods.IsGet(method) && !HttpMethods.IsHead(method)) return await next(context);

            var res = await next(context);

            if (context.HttpContext.Response.StatusCode == StatusCodes.Status200OK)
                context.HttpContext.Response.Headers.CacheControl = $"private,max-age={MaxImgCachingLengthSeconds}";

            return res;
        });
    }
}

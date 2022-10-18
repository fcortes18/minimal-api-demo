using MinimalAPI.Attributes;

namespace MinimalAPI.Middleware
{
    public class ApiKeyMiddleware
    {
        private readonly RequestDelegate _next;
        private const string APIKEYNAME = "ApiKey";

        public ApiKeyMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.GetEndpoint()?.Metadata.GetMetadata<ApiKeyAttribute>() is ApiKeyAttribute attribute)
            {
                if (!context.Request.Headers.TryGetValue(APIKEYNAME, out var extractedApiKey))
                {
                    context.Response.StatusCode = 401;
                    context.Response.ContentType = "text/plain";
                    await context.Response.WriteAsync("Api Key was not provided.");
                    return;
                }

                var appSettings = context.RequestServices.GetRequiredService<IConfiguration>();
                var apiKey = appSettings.GetValue<string>(APIKEYNAME);
                if (!apiKey.Equals(extractedApiKey))
                {
                    context.Response.StatusCode = 401;
                    context.Response.ContentType = "text/plain";
                    await context.Response.WriteAsync("Unauthorized client.");
                    return;
                }
            }

            await _next(context);
        }
    }
}

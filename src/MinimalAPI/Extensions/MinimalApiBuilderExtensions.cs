using MinimalAPI.Attributes;
using MinimalAPI.DataSource.Tables;

namespace MinimalAPI.Extensions
{
    public static class MinimalApiBuilderExtensions
    {
        public static TBuilder RequireApiKey<TBuilder>(this TBuilder builder) where TBuilder : IEndpointConventionBuilder
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.WithMetadata(new ApiKeyAttribute());
            return builder;
        }

        public static IEndpointConventionBuilder GetEndpointProduces(this RouteHandlerBuilder builder, Type? responseType = null)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status200OK, responseType)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);

            return builder;
        }
    }
}

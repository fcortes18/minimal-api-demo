using MinimalAPI.Attributes;

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
    }
}

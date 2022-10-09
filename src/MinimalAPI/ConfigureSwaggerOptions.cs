using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;
using System.Text;

namespace MinimalAPI
{
    public class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
    {
        private readonly IApiVersionDescriptionProvider _provider;

        public ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider) => _provider = provider;

        public void Configure(SwaggerGenOptions options)
        {
            foreach (var description in _provider.ApiVersionDescriptions)
            {
                options.SwaggerDoc(description.GroupName, 
                    CreateVersionInfo(description));
            }
        }

        private static OpenApiInfo CreateVersionInfo(ApiVersionDescription description)
        {
            var text = new StringBuilder("Minimal API demo.");
            var info = new OpenApiInfo()
            {
                Title = Assembly.GetEntryAssembly()?.GetName().Name ?? "Minimal API",
                Version = description.ApiVersion.ToString()
            };

            if (description.IsDeprecated)
            {
                text.Append(" This API version has been deprecated. Please use one of the new APIs available from the explorer.");
            }

            info.Description = text.ToString();

            return info;
        }

    }
}

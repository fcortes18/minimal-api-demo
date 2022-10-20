using MinimalAPI.Auth;
using MinimalAPI.DataSource;
using MinimalAPI.Middleware;
using MinimalAPI.Swagger;

namespace MinimalAPI
{
    public static class ApiModuleSetup
    {
        public static void AddApiServicesSetup(this IServiceCollection services, ConfigurationManager configuration)
        {
            services.AddEndpointsApiExplorer();
            services.AddSwaggerModule();
            services.AddAuthModule(configuration);
            services.AddDataSourceModule();
        }

        public static void AddApiAppSetup(this WebApplication app)
        {
            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseCors("AllowAll");
            app.UseMiddleware<ApiKeyMiddleware>();
        }
    }
}

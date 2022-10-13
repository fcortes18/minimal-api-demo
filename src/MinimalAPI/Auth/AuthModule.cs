using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace MinimalAPI.Auth
{
    public static class AuthModule
    {
        public static void AddAuthModule(this IServiceCollection services, ConfigurationManager configuration)
        {
            // configure strongly typed TokenOptions object
            var tokenOptionsSection = configuration.GetSection(nameof(TokenOptions));
            var tokenOptions = tokenOptionsSection.Get<TokenOptions>();
            services.Configure<TokenOptions>(tokenOptionsSection);
            services.AddAuthorization();
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = tokenOptions?.Issuer,
                    ValidAudience = tokenOptions?.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenOptions?.SecurityKey ?? string.Empty))
                };
            });

            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", a => a.AllowAnyHeader().AllowAnyOrigin().AllowAnyMethod());
            }
            );

            services.AddSingleton<ITokenHelper, JwtTokenHelper>();
        }
    }
}

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

namespace MusicStreamingService.Infrastructure.Authentication;

public static class AuthInjection
{
    public static IServiceCollection ConfigureAuth<T>(
        this IServiceCollection services,
        IWebHostEnvironment environment,
        IConfiguration configuration) where T: IClaimConvertable
    {
        var config = configuration.GetSection(nameof(JwtConfiguration)).Get<JwtConfiguration>();
        var jwtService = new JwtService<T>(config!);

        services.Configure<JwtConfiguration>(configuration.GetSection(nameof(JwtConfiguration)));
        services.AddScoped<IJwtService<T>, JwtService<T>>();

        if (environment.IsDevelopment() && !configuration.GetSection("AuthEnabled").Get<bool>())
        {
            services
                .AddAuthentication("Dev")
                .AddScheme<AuthenticationSchemeOptions, DevAuthHandler>("Dev", _ => { });

            services.AddOpenApi();
        }
        else
        {
            services
                .AddAuthentication(options => 
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = jwtService.GetTokenValidationParameters();
                });
            
            services.AddOpenApi(options =>
            {
                options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
            });
        }

        services.AddAuthentication();
        services.AddAuthorization();

        return services;
    }
}

public sealed class BearerSecuritySchemeTransformer(IAuthenticationSchemeProvider authenticationSchemeProvider) : IOpenApiDocumentTransformer
{
    public async Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        var authenticationSchemes = await authenticationSchemeProvider.GetAllSchemesAsync();
        if (authenticationSchemes.Any(authScheme => authScheme.Name == JwtBearerDefaults.AuthenticationScheme))
        {
            var requirements = new Dictionary<string, OpenApiSecurityScheme>
            {
                [JwtBearerDefaults.AuthenticationScheme] = new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = JwtBearerDefaults.AuthenticationScheme, 
                    In = ParameterLocation.Header,
                    BearerFormat = "JWT"
                }
            };
            document.Components ??= new OpenApiComponents();
            document.Components.SecuritySchemes = requirements;

            foreach (var operation in document.Paths.Values.SelectMany(path => path.Operations))
            {
                operation.Value.Security.Add(new OpenApiSecurityRequirement
                {
                    [new OpenApiSecurityScheme { Reference = new OpenApiReference { Id = JwtBearerDefaults.AuthenticationScheme, Type = ReferenceType.SecurityScheme } }] = Array.Empty<string>()
                });
            }
        }
    }
}
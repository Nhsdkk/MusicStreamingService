using Microsoft.AspNetCore.Authentication.JwtBearer;
using MusicStreamingService.Data;
using MusicStreamingService.Data.Entities;
using MusicStreamingService.Infrastructure.Authentication;
using MusicStreamingService.Infrastructure.Password;
using Scalar.AspNetCore;

namespace MusicStreamingService.Setup;

public static class Setup
{
    public static WebApplication Configure(this WebApplicationBuilder builder)
    {
        var services = builder.Services;
        var configuration = builder.Configuration;
        configuration.AddEnvironmentVariables();

        services.AddControllers();

        services
            .ConfigureMusicStreamingDbContext(configuration)
            .AddMediator(
                options: options =>
                    options.ServiceLifetime = ServiceLifetime.Scoped)
            .ConfigurePasswordService(configuration)
            .ConfigureAuth<UserClaims>(builder.Environment, configuration);

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.Services.CreateScope().ApplyMigrations<MusicStreamingContext>();
            app.MapOpenApi();
            app.MapScalarApiReference(opts =>
                opts.AddPreferredSecuritySchemes(JwtBearerDefaults.AuthenticationScheme));
        }

        app.MapControllers();

        app.UseAuthentication();
        app.UseAuthorization();


        return app;
    }
}
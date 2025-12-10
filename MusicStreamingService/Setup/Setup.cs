using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using MusicStreamingService.Auth;
using MusicStreamingService.Data;
using MusicStreamingService.Data.Entities;
using MusicStreamingService.Infrastructure.Authentication;
using MusicStreamingService.Infrastructure.ObjectStorage;
using MusicStreamingService.Infrastructure.Password;
using Scalar.AspNetCore;
using SharpGrip.FluentValidation.AutoValidation.Mvc.Extensions;

namespace MusicStreamingService.Setup;

public static class Setup
{
    public static WebApplication Configure(this WebApplicationBuilder builder)
    {
        var services = builder.Services;
        var configuration = builder.Configuration;
        configuration.AddEnvironmentVariables();
        
        services
            .AddControllers()
            .AddJsonOptions(opts =>
            {
                opts.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });
        
        services.ConfigureHttpJsonOptions(opts =>
        {
            opts.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });
        
        services.AddValidatorsFromAssembly(Assembly.GetCallingAssembly());
        services.AddFluentValidationAutoValidation();

        services.AddScoped<IClaimValidator<UserClaims>, ClaimValidator>();
        
        services
            .ConfigureMusicStreamingDbContext(configuration)
            .AddMediator(
                options: options =>
                    options.ServiceLifetime = ServiceLifetime.Scoped)
            .ConfigurePasswordService(configuration)
            .ConfigureObjectStorageServices(configuration)
            .ConfigureAuth(builder.Environment, configuration);

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
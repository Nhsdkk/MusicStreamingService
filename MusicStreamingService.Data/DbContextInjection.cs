using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MusicStreamingService.Data.Interceptors;

namespace MusicStreamingService.Data;

public static class DbContextInjection
{
    public static IServiceCollection ConfigureMusicStreamingDbContext(this IServiceCollection services, IConfiguration configuration)
    {
        var config = configuration.GetRequiredSection(nameof(PsqlConfiguration)).Get<PsqlConfiguration>();
        services.AddSingleton<AuditLogSaveChangesInterceptor>();
        services.AddNpgsql<MusicStreamingContext>(config!.GetConnectionString());

        return services;
    }
}
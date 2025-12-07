using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Minio;

namespace MusicStreamingService.Infrastructure.ObjectStorage;

public static class ObjectStorageInjection
{
    public static IServiceCollection ConfigureObjectStorageServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var config = configuration.GetSection(nameof(MinioConfiguration)).Get<MinioConfiguration>();

        services.Configure<MinioConfiguration>(configuration.GetSection(nameof(MinioConfiguration)));
        services.AddMinio(client => client
            .WithEndpoint(config!.Endpoint)
            .WithCredentials(config.AccessKey, config.SecretKey)
            .WithSSL(config.UseSsl)
            .Build());
        
        services
            .AddScoped<ISongStorageService, SongStorageService>()
            .AddScoped<IAlbumStorageService, AlbumStorageService>();
        
        return services;
    }
}
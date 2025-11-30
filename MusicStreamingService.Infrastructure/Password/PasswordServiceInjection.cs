using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MusicStreamingService.Infrastructure.Password;

public static class PasswordServiceInjection
{
    public static IServiceCollection ConfigurePasswordService(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<PasswordServiceConfig>(configuration.GetSection(nameof(PasswordServiceConfig)));
        services.AddScoped<IPasswordService, PasswordService>();
        return services;
    }
}
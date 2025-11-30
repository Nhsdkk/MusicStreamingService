using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MusicStreamingService.Data;

public static class Migrator
{
    public static IServiceScope ApplyMigrations<TContext>(this IServiceScope serviceScope)
        where TContext : DbContext
    {
        using var context = serviceScope.ServiceProvider.GetService<TContext>();
        context!.Database.Migrate();
        return serviceScope;
    }
}
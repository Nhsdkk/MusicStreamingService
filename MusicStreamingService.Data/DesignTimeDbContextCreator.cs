using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using MusicStreamingService.Data.Interceptors;

namespace MusicStreamingService.Data;

// TODO: does not work because of env 
public class DesignTimeDbContextCreator : IDesignTimeDbContextFactory<MusicStreamingContext>
{
    public MusicStreamingContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        var url = configuration.GetSection("DesignDbConnectionString").Get<string>();
        var options = new DbContextOptionsBuilder<MusicStreamingContext>()
            .UseNpgsql(url);
        
        return new MusicStreamingContext(options.Options, new AuditLogSaveChangesInterceptor());
    }
}
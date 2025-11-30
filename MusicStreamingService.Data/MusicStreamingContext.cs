using Microsoft.EntityFrameworkCore;
using MusicStreamingService.Data.CompiledModels;
using MusicStreamingService.Data.Entities;
using MusicStreamingService.Data.Entities.Configurations;

namespace MusicStreamingService.Data;

public sealed class MusicStreamingContext : DbContext
{
    public DbSet<UserEntity> Users { get; set; }
    
    public DbSet<RegionEntity> Regions { get; set; }

    public MusicStreamingContext(DbContextOptions<MusicStreamingContext> options) : base(options) { }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseModel(MusicStreamingContextModel.Instance);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new UserEntityConfiguration());
        modelBuilder.ApplyConfiguration(new RegionEntityConfiguration());
    }
}
using Microsoft.EntityFrameworkCore;
using MusicStreamingService.Data.CompiledModels;
using MusicStreamingService.Data.Entities;
using MusicStreamingService.Data.Entities.Configurations;

namespace MusicStreamingService.Data;

public sealed class MusicStreamingContext : DbContext
{
    public DbSet<UserEntity> Users { get; set; }
    
    public DbSet<RegionEntity> Regions { get; set; }
    
    public DbSet<AlbumEntity> Albums { get; set; }
    
    public DbSet<AlbumFavoriteEntity> AlbumFavorites { get; set; }
    
    public DbSet<AlbumSongEntity> AlbumSongs { get; set; }
    
    public DbSet<AllowedDistributionEntity> AllowedDistribution { get; set; }
    
    public DbSet<GenreEntity> Genres { get; set; }
    
    public DbSet<SongArtistEntity> SongArtists { get; set; }
    
    public DbSet<SongEntity> Songs { get; set; }
    
    public DbSet<SongFavoriteEntity> SongFavorites { get; set; }
    
    public DbSet<SongGenreEntity> SongGenres { get; set; }

    public MusicStreamingContext(DbContextOptions<MusicStreamingContext> options) : base(options) { }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseModel(MusicStreamingContextModel.Instance);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new UserEntityConfiguration());
        modelBuilder.ApplyConfiguration(new RegionEntityConfiguration());
        modelBuilder.ApplyConfiguration(new AlbumEntityConfiguration());
        modelBuilder.ApplyConfiguration(new AlbumFavoriteEntityConfiguration());
        modelBuilder.ApplyConfiguration(new AlbumSongEntityConfiguration());
        modelBuilder.ApplyConfiguration(new AllowedDistributionEntityConfiguration());
        modelBuilder.ApplyConfiguration(new GenreEntityConfiguration());
        modelBuilder.ApplyConfiguration(new SongArtistEntityConfiguration());
        modelBuilder.ApplyConfiguration(new SongEntityConfiguration());
        modelBuilder.ApplyConfiguration(new SongFavoriteEntityConfiguration());
        modelBuilder.ApplyConfiguration(new SongGenreEntityConfiguration());
    }
}
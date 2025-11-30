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
    
    public DbSet<SubscriberEntity> Subscribers { get; set; }
    
    public DbSet<SubscriptionEntity> Subscriptions { get; set; }
    
    public DbSet<PaymentEntity> Payments { get; set; }
    
    public DbSet<PlaylistEntity> Playlists { get; set; }
    
    public DbSet<PlaylistSongEntity> PlaylistSongs { get; set; }
    
    public DbSet<PlaylistFavoriteEntity> PlaylistFavorites { get; set; }
    
    public DbSet<StreamingEventEntity> StreamingEvents { get; set; }
    
    public DbSet<DeviceEntity> Devices { get; set; }

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
        modelBuilder.ApplyConfiguration(new PaymentEntityConfiguration());
        modelBuilder.ApplyConfiguration(new SubscriberEntityConfiguration());
        modelBuilder.ApplyConfiguration(new SubscriptionEntityConfiguration());
        modelBuilder.ApplyConfiguration(new PlaylistEntityConfiguration());
        modelBuilder.ApplyConfiguration(new PlaylistFavoriteEntityConfiguration());
        modelBuilder.ApplyConfiguration(new PlaylistSongEntityConfiguration());
        modelBuilder.ApplyConfiguration(new StreamingEventEntityConfiguration());
        modelBuilder.ApplyConfiguration(new DeviceEntityConfiguration());
    }
}
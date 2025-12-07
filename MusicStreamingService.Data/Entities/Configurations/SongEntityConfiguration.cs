using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MusicStreamingService.Data.Constraints;
using MusicStreamingService.Data.Entities.Configurations.Base;

namespace MusicStreamingService.Data.Entities.Configurations;

internal sealed class SongEntityConfiguration : BaseUpdatableEntityConfiguration<SongEntity>
{
    protected override void OnConfigure(EntityTypeBuilder<SongEntity> builder)
    {
        builder.Property(x => x.Title).IsRequired().HasMaxLength(SongEntityConstraints.TitleMaxLength);
        builder.Property(x => x.Likes).HasDefaultValue(0).ValueGeneratedNever();
        builder.Property(x => x.DurationMs).IsRequired();
        builder.Property(x => x.Explicit).IsRequired();
        builder.Property(x => x.S3MediaFileName).IsRequired();
        
        builder
            .HasMany(x => x.Artists)
            .WithOne(x => x.Song)
            .HasForeignKey(x => x.SongId);
        builder
            .HasMany(x => x.AllowedRegions)
            .WithMany()
            .UsingEntity<AllowedDistributionEntity>();
        builder
            .HasMany(x => x.Albums)
            .WithOne(x => x.Song)
            .HasForeignKey(x => x.SongId);
        builder
            .HasMany(x => x.Genres)
            .WithMany(x => x.Songs)
            .UsingEntity<SongGenreEntity>();
        builder
            .HasMany(x => x.LikedByUsers)
            .WithMany(x => x.FavoriteSongs)
            .UsingEntity<SongFavoriteEntity>();
    }
}
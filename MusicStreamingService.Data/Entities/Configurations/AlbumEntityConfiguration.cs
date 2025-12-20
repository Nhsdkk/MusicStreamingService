using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MusicStreamingService.Data.Constraints;
using MusicStreamingService.Data.Entities.Configurations.Base;

namespace MusicStreamingService.Data.Entities.Configurations;

internal sealed class AlbumEntityConfiguration : BaseUpdatableEntityConfiguration<AlbumEntity>
{
    protected override void OnConfigure(EntityTypeBuilder<AlbumEntity> builder)
    {
        builder.Property(x => x.Title).IsRequired().HasMaxLength(AlbumEntityConstraints.MaxTitleLength);
        builder.Property(x => x.ArtistId).IsRequired();
        builder.Property(x => x.ReleaseDate).IsRequired();
        builder.Property(x => x.S3ArtworkFilename).IsRequired();
        
        builder
            .HasOne(x => x.Artist)
            .WithMany()
            .HasForeignKey(x => x.ArtistId);
        builder
            .HasMany(x => x.Songs)
            .WithOne(x => x.Album)
            .HasForeignKey(x => x.AlbumId);
        
        builder.Property(x => x.Likes).HasDefaultValue(0).ValueGeneratedNever();
        
        builder
            .HasIndex(x => x.Title)
            .HasMethod("GIST")
            .HasOperators("gist_trgm_ops")
            .IsUnique(false);
        builder.HasIndex(x => x.ReleaseDate).IsUnique(false);
    }
}
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MusicStreamingService.Data.Entities.Configurations;

internal sealed class AlbumSongEntityConfiguration : IEntityTypeConfiguration<AlbumSongEntity>
{
    public void Configure(EntityTypeBuilder<AlbumSongEntity> builder)
    {
        builder.Property(x => x.Title).IsRequired();
        builder.Property(x => x.Position).IsRequired();

        builder.HasKey(x => new { x.SongId, x.AlbumId });
        
        builder
            .HasOne(x => x.Song)
            .WithMany(x => x.Albums)
            .HasForeignKey(x => x.SongId);
        builder
            .HasOne(x => x.Album)
            .WithMany(x => x.Songs)
            .HasForeignKey(x => x.AlbumId);
    }
}
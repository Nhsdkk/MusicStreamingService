using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MusicStreamingService.Data.Entities.Configurations;

internal sealed class PlaylistSongEntityConfiguration : IEntityTypeConfiguration<PlaylistSongEntity>
{
    public void Configure(EntityTypeBuilder<PlaylistSongEntity> builder)
    {
        builder.Property(x => x.AddedAt)
            .HasDefaultValueSql("now()");
        
        builder.HasKey(x => new { x.SongId, x.PlaylistId });

        builder
            .HasOne(x => x.Song)
            .WithMany()
            .HasForeignKey(x => x.SongId);
        builder
            .HasOne(x => x.Playlist)
            .WithMany(x => x.Songs)
            .HasForeignKey(x => x.PlaylistId);
    }
}
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MusicStreamingService.Data.Entities.Configurations;

internal sealed class SongArtistEntityConfiguration : IEntityTypeConfiguration<SongArtistEntity>
{
    public void Configure(EntityTypeBuilder<SongArtistEntity> builder)
    {
        builder.Property(x => x.MainArtist).IsRequired();
        
        builder.HasKey(x => new { x.SongId, x.ArtistId });
        
        builder
            .HasOne(x => x.Artist)
            .WithMany()
            .HasForeignKey(x => x.ArtistId);
        builder
            .HasOne(x => x.Song)
            .WithMany()
            .HasForeignKey(x => x.SongId);
    }
}
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MusicStreamingService.Data.Entities.Configurations;

internal sealed class SongGenreEntityConfiguration : IEntityTypeConfiguration<SongGenreEntity>
{
    public void Configure(EntityTypeBuilder<SongGenreEntity> builder)
    {
        builder.HasKey(x => new { x.SongId, x.GenreId });

        builder
            .HasOne(x => x.Song)
            .WithMany()
            .HasForeignKey(x => x.SongId);
        builder
            .HasOne(x => x.Genre)
            .WithMany()
            .HasForeignKey(x => x.GenreId);
    }
}
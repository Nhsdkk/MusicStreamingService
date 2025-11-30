using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MusicStreamingService.Data.Entities.Configurations;

internal sealed class SongFavoriteEntityConfiguration : IEntityTypeConfiguration<SongFavoriteEntity>
{
    public void Configure(EntityTypeBuilder<SongFavoriteEntity> builder)
    {
        builder.HasKey(x => new { x.SongId, x.UserId });

        builder
            .HasOne(x => x.Song)
            .WithMany()
            .HasForeignKey(x => x.SongId);
        builder
            .HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId);
    }
}
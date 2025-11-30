using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MusicStreamingService.Data.Entities.Configurations;

internal sealed class PlaylistFavoriteEntityConfiguration : IEntityTypeConfiguration<PlaylistFavoriteEntity>
{
    public void Configure(EntityTypeBuilder<PlaylistFavoriteEntity> builder)
    {
        builder.HasKey(x => new { x.PlaylistId, x.UserId });

        builder
            .HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId);
        builder
            .HasOne(x => x.Playlist)
            .WithMany()
            .HasForeignKey(x => x.PlaylistId);
    }
}
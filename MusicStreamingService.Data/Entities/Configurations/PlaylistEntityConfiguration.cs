using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MusicStreamingService.Data.Constraints;
using MusicStreamingService.Data.Entities.Configurations.Base;

namespace MusicStreamingService.Data.Entities.Configurations;

internal sealed class PlaylistEntityConfiguration : BaseUpdatableEntityConfiguration<PlaylistEntity>
{
    protected override void OnConfigure(EntityTypeBuilder<PlaylistEntity> builder)
    {
        builder.Property(x => x.CreatorId).IsRequired();
        builder.Property(x => x.Title).IsRequired().HasMaxLength(PlaylistEntityConstraints.TitleMaxLength);
        builder.Property(x => x.Likes).HasDefaultValue(0).ValueGeneratedNever();
        builder.Property(x => x.AccessType).HasConversion<PlaylistAccessTypeConverter>().IsRequired();

        builder
            .HasOne(x => x.Creator)
            .WithMany()
            .HasForeignKey(x => x.CreatorId);
        builder
            .HasMany(x => x.Songs)
            .WithOne(x => x.Playlist)
            .HasForeignKey(x => x.PlaylistId);
    }
}
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MusicStreamingService.Data.Converters;
using MusicStreamingService.Data.Entities.Configurations.Base;

namespace MusicStreamingService.Data.Entities.Configurations;

internal sealed class PlaylistImportStagingEntityConfiguration : BaseUpdatableEntityConfiguration<PlaylistImportStagingEntity>
{
    protected override void OnConfigure(EntityTypeBuilder<PlaylistImportStagingEntity> builder)
    {
        builder.Property(x => x.ImportTaskId).IsRequired();
        builder.Property(x => x.BatchId).IsRequired();
        builder.Property(x => x.AlbumName).IsRequired();
        builder.Property(x => x.ArtistName).IsRequired();
        builder.Property(x => x.SongTitle).IsRequired();
        builder.Property(x => x.ReleaseDate).IsRequired();
        builder.Property(x => x.Status).IsRequired().HasConversion<EnumStringConverter<StagingStatus>>();

        builder
            .HasOne(x => x.Playlist)
            .WithMany()
            .HasForeignKey(x => x.PlaylistId);
        builder
            .HasOne(x => x.ImportTask)
            .WithMany(x => x.StagingEntries)
            .HasForeignKey(x => x.ImportTaskId);
    }
}
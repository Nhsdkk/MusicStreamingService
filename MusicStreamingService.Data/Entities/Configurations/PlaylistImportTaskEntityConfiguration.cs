using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MusicStreamingService.Data.Converters;
using MusicStreamingService.Data.Entities.Configurations.Base;

namespace MusicStreamingService.Data.Entities.Configurations;

internal sealed class PlaylistImportTaskEntityConfiguration : BaseUpdatableEntityConfiguration<PlaylistImportTaskEntity>
{
    protected override void OnConfigure(EntityTypeBuilder<PlaylistImportTaskEntity> builder)
    {
        builder.Property(x => x.CreatorId).IsRequired();
        builder.Property(x => x.S3FileName).IsRequired();
        builder.Property(x => x.Status).IsRequired().HasConversion<EnumStringConverter<PlaylistImportTaskStatus>>();;
        builder.Property(x => x.TotalEntries).IsRequired();
        builder.Property(x => x.ProcessedEntries).HasDefaultValue(0);

        builder
            .HasOne(x => x.Creator)
            .WithMany()
            .HasForeignKey(x => x.CreatorId);
        builder
            .HasMany(x => x.StagingEntries)
            .WithOne()
            .HasForeignKey(x => x.ImportTaskId);
    }
}
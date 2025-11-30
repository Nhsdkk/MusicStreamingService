using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MusicStreamingService.Data.Entities.Configurations.Base;

namespace MusicStreamingService.Data.Entities.Configurations;

internal sealed class StreamingEventEntityConfiguration : BaseIdEntityConfiguration<StreamingEventEntity>
{
    protected override void OnConfigure(EntityTypeBuilder<StreamingEventEntity> builder)
    {
        builder.Property(x => x.SongId).IsRequired();
        builder.Property(x => x.DeviceId).IsRequired();
        builder.Property(x => x.EventType).HasConversion<StreamingEventTypeConverter>().IsRequired();
        builder.Property(x => x.PositionMs).IsRequired();
        builder.Property(x => x.TimePlayedSinceLastRequestMs).IsRequired();

        builder
            .HasOne(x => x.Device)
            .WithMany(x => x.StreamingEvents)
            .HasForeignKey(x => x.DeviceId);
        builder
            .HasOne(x => x.Song)
            .WithMany()
            .HasForeignKey(x => x.SongId);
    }
}
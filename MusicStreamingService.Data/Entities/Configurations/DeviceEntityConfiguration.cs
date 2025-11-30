using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MusicStreamingService.Data.Constraints;
using MusicStreamingService.Data.Entities.Configurations.Base;

namespace MusicStreamingService.Data.Entities.Configurations;

internal sealed class DeviceEntityConfiguration : BaseIdEntityConfiguration<DeviceEntity>
{
    protected override void OnConfigure(EntityTypeBuilder<DeviceEntity> builder)
    {
        builder.Property(x => x.OwnerId).IsRequired();
        builder.Property(x => x.Title).IsRequired().HasMaxLength(DeviceEntityConstraints.TitleMaxLength);

        builder
            .HasOne(x => x.Owner)
            .WithMany(x => x.Devices)
            .HasForeignKey(x => x.OwnerId);
        builder
            .HasMany(x => x.StreamingEvents)
            .WithOne(x => x.Device)
            .HasForeignKey(x => x.DeviceId);
    }
}
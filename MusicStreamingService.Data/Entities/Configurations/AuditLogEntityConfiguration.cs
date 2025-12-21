using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MusicStreamingService.Data.Converters;
using MusicStreamingService.Data.Entities.Configurations.Base;

namespace MusicStreamingService.Data.Entities.Configurations;

internal sealed class AuditLogEntityConfiguration : BaseIdEntityConfiguration<AuditLogEntity>
{
    protected override void OnConfigure(EntityTypeBuilder<AuditLogEntity> builder)
    {
        builder.Property(x => x.Action).IsRequired().HasConversion<EnumStringConverter<EntityAction>>();
        builder.Property(x => x.NewValues).IsRequired().HasColumnType("jsonb");
        builder.Property(x => x.OldValues).IsRequired().HasColumnType("jsonb");
        builder.Property(x => x.TableName).IsRequired().HasMaxLength(256);
    }
}
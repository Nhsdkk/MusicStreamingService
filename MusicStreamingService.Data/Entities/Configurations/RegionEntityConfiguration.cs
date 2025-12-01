using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MusicStreamingService.Data.Constraints;
using MusicStreamingService.Data.Entities.Configurations.Base;

namespace MusicStreamingService.Data.Entities.Configurations;

internal sealed class RegionEntityConfiguration : BaseIdEntityConfiguration<RegionEntity>
{
    protected override void OnConfigure(EntityTypeBuilder<RegionEntity> builder)
    {
        builder.Property(x => x.Title).HasMaxLength(RegionEntityConstraints.RegionNameMaxLength).IsRequired();

        builder.HasIndex(x => x.Title).IsUnique();
    }
}
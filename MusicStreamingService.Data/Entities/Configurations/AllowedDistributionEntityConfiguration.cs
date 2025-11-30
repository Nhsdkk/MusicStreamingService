using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MusicStreamingService.Data.Entities.Configurations;

internal sealed class AllowedDistributionEntityConfiguration : IEntityTypeConfiguration<AllowedDistributionEntity>
{
    public void Configure(EntityTypeBuilder<AllowedDistributionEntity> builder)
    {
        builder.HasKey(x => new { x.SongId, x.RegionId });

        builder
            .HasOne(x => x.Song)
            .WithMany()
            .HasForeignKey(x => x.SongId);
        builder
            .HasOne(x => x.Region)
            .WithMany()
            .HasForeignKey(x => x.RegionId);
    }
}
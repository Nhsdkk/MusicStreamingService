using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MusicStreamingService.Data.Constraints;
using MusicStreamingService.Data.Entities.Configurations.Base;

namespace MusicStreamingService.Data.Entities.Configurations;

internal sealed class SubscriptionEntityConfiguration : BaseUpdatableEntityConfiguration<SubscriptionEntity>
{
    protected override void OnConfigure(EntityTypeBuilder<SubscriptionEntity> builder)
    {
        builder.Property(x => x.Title).IsRequired().HasMaxLength(SubscriptionEntityConstraints.TitleMaxLength);
        builder.Property(x => x.Amount).IsRequired().HasColumnType("money");
        builder.Property(x => x.Period).IsRequired().HasColumnType("interval");
        builder.Property(x => x.Discontinued).HasDefaultValue(false);
    }
}
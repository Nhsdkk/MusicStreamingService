using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MusicStreamingService.Data.Entities.Configurations;

internal sealed class SubscriberEntityConfiguration : IEntityTypeConfiguration<SubscriberEntity>
{
    public void Configure(EntityTypeBuilder<SubscriberEntity> builder)
    {
        builder.Property(x => x.ValidFrom).IsRequired();
        builder.Property(x => x.ValidTo).IsRequired();
        
        builder.HasKey(x => new { x.SubscriberId, x.SubscriptionId });

        builder
            .HasOne(x => x.Subscriber)
            .WithMany(x => x.Subscriptions)
            .HasForeignKey(x => x.SubscriberId);
        builder
            .HasOne(x => x.Subscription)
            .WithMany()
            .HasForeignKey(x => x.SubscriptionId);
    }
}
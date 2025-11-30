using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MusicStreamingService.Data.Entities.Configurations.Base;

namespace MusicStreamingService.Data.Entities.Configurations;

internal sealed class PaymentEntityConfiguration : BaseIdEntityConfiguration<PaymentEntity>
{
    protected override void OnConfigure(EntityTypeBuilder<PaymentEntity> builder)
    {
        builder.Property(x => x.PayerId).IsRequired();
        builder.Property(x => x.SubscriptionId).IsRequired();
        builder.Property(x => x.Amount).IsRequired();

        builder
            .HasOne(x => x.Payer)
            .WithMany(x => x.Payments)
            .HasForeignKey(x => x.PayerId);
        builder
            .HasOne(x => x.Subscription)
            .WithMany()
            .HasForeignKey(x => x.SubscriptionId);
    }
}
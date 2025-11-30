using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MusicStreamingService.Data.Entities.Configurations.Base;

internal abstract class BaseUpdatableEntityConfiguration<T> : BaseIdEntityConfiguration<T> where T: BaseUpdatableIdEntity
{
    public override void Configure(EntityTypeBuilder<T> builder)
    {
        base.Configure(builder);
        builder.Property(x => x.UpdatedAt)
            .HasDefaultValueSql("now()")
            .IsRequired();   
    }
}
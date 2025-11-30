using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MusicStreamingService.Data.Entities.Configurations.Base;

internal abstract class BaseIdEntityConfiguration<T> : IEntityTypeConfiguration<T> where T: BaseIdEntity
{
    public virtual void Configure(EntityTypeBuilder<T> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(x => x.CreatedAt)
            .HasDefaultValueSql("now()")
            .IsRequired();
        
        OnConfigure(builder);
    }

    protected abstract void OnConfigure(EntityTypeBuilder<T> builder);
}
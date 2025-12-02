using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MusicStreamingService.Data.Constraints;
using MusicStreamingService.Data.Entities.Configurations.Base;

namespace MusicStreamingService.Data.Entities.Configurations;

internal sealed class RoleEntityConfiguration : BaseUpdatableEntityConfiguration<RoleEntity>
{
    protected override void OnConfigure(EntityTypeBuilder<RoleEntity> builder)
    {
        builder.Property(x => x.Description).IsRequired();
        builder.Property(x => x.Title).HasMaxLength(RoleEntityConstraints.TitleMaxLength).IsRequired();

        builder.HasIndex(x => x.Title).IsUnique();

        builder
            .HasMany(x => x.Permissions)
            .WithMany()
            .UsingEntity<RolePermissionEntity>();
    }
}
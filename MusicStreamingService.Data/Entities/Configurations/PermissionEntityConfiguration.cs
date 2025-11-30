using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MusicStreamingService.Data.Constraints;
using MusicStreamingService.Data.Entities.Configurations.Base;

namespace MusicStreamingService.Data.Entities.Configurations;

internal sealed class PermissionEntityConfiguration : BaseUpdatableEntityConfiguration<PermissionEntity>
{
    protected override void OnConfigure(EntityTypeBuilder<PermissionEntity> builder)
    {
        builder.Property(x => x.Description).IsRequired().HasMaxLength(PermissionEntityConstraints.TitleMaxLength);
        builder.Property(x => x.Title).IsRequired();

        builder.HasAlternateKey(x => x.Title);

        builder
            .HasMany(x => x.UsedBy)
            .WithMany(x => x.Permissions)
            .UsingEntity<UserPermissionEntity>();
    }
}
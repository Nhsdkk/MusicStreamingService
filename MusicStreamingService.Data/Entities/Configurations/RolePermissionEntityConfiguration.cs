using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MusicStreamingService.Data.Entities.Configurations;

internal sealed class RolePermissionEntityConfiguration : IEntityTypeConfiguration<RolePermissionEntity>
{
    public void Configure(EntityTypeBuilder<RolePermissionEntity> builder)
    {
        builder.HasKey(x => new { x.RoleId, x.PermissionId });

        builder
            .HasOne(x => x.Role)
            .WithMany()
            .HasForeignKey(x => x.RoleId);
        builder
            .HasOne(x => x.Permission)
            .WithMany()
            .HasForeignKey(x => x.PermissionId);
    }
}
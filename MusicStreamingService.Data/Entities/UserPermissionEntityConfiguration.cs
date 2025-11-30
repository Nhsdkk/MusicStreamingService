using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MusicStreamingService.Data.Entities;

internal sealed class UserPermissionEntityConfiguration : IEntityTypeConfiguration<UserPermissionEntity>
{
    public void Configure(EntityTypeBuilder<UserPermissionEntity> builder)
    {
        builder.HasKey(x => new { x.UserId, x.PermissionId });

        builder
            .HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId);
        builder
            .HasOne(x => x.Permission)
            .WithMany()
            .HasForeignKey(x => x.PermissionId);
    }
}
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MusicStreamingService.Data.Entities.Configurations;

internal sealed class UserRoleEntityConfiguration : IEntityTypeConfiguration<UserRoleEntity>
{
    public void Configure(EntityTypeBuilder<UserRoleEntity> builder)
    {
        builder.HasKey(x => new { x.UserId, x.RoleId });

        builder
            .HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId);
        builder
            .HasOne(x => x.Role)
            .WithMany()
            .HasForeignKey(x => x.RoleId);
    }
}
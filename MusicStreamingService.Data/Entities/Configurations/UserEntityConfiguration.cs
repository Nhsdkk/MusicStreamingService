using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MusicStreamingService.Data.Constraints;
using MusicStreamingService.Data.Entities.Configurations.Base;

namespace MusicStreamingService.Data.Entities.Configurations;

internal class UserEntityConfiguration : BaseUpdatableEntityConfiguration<UserEntity>
{
    protected override void OnConfigure(EntityTypeBuilder<UserEntity> builder)
    {
        builder.Property(x => x.Username).HasMaxLength(UserEntityConstraints.MaxUsernameLength);
        builder.Property(x => x.Disabled).HasDefaultValue(false);
        builder.Property(x => x.Email).HasMaxLength(UserEntityConstraints.MaxEmailLength);
        builder.Property(x => x.Password).HasColumnType("bytea");

        builder.HasAlternateKey(x => x.Email);
        builder.HasAlternateKey(x => x.Username);

        builder.HasOne(x => x.Region).WithMany().HasForeignKey(x => x.RegionId);
        builder
            .HasMany(x => x.FavoriteAlbums)
            .WithMany(x => x.LikedUsers)
            .UsingEntity<AlbumFavoriteEntity>();
    }
}
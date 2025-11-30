using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MusicStreamingService.Data.Constraints;
using MusicStreamingService.Data.Entities.Configurations.Base;

namespace MusicStreamingService.Data.Entities.Configurations;

internal class UserEntityConfiguration : BaseUpdatableEntityConfiguration<UserEntity>
{
    protected override void OnConfigure(EntityTypeBuilder<UserEntity> builder)
    {
        builder.Property(x => x.Username).IsRequired().HasMaxLength(UserEntityConstraints.MaxUsernameLength);
        builder.Property(x => x.Disabled).HasDefaultValue(false);
        builder.Property(x => x.Email).IsRequired().HasMaxLength(UserEntityConstraints.MaxEmailLength);
        builder.Property(x => x.Password).IsRequired().HasColumnType("bytea");
        builder.Property(x => x.RegionId).IsRequired();
        builder.Property(x => x.FullName).IsRequired().HasMaxLength(UserEntityConstraints.MaxFullNameLength);

        builder.HasAlternateKey(x => x.Email);
        builder.HasAlternateKey(x => x.Username);

        builder.HasOne(x => x.Region).WithMany().HasForeignKey(x => x.RegionId);
        builder
            .HasMany(x => x.FavoriteAlbums)
            .WithMany()
            .UsingEntity<AlbumFavoriteEntity>();
        builder
            .HasMany(x => x.FavoriteSongs)
            .WithMany()
            .UsingEntity<SongFavoriteEntity>();
        builder
            .HasMany(x => x.ArtistAlbums)
            .WithOne(x => x.Artist)
            .HasForeignKey(x => x.ArtistId);
        builder
            .HasMany(x => x.ArtistSongs)
            .WithOne(x => x.Artist)
            .HasForeignKey(x => x.ArtistId);
        builder
            .HasMany(x => x.Subscriptions)
            .WithOne(x => x.Subscriber)
            .HasForeignKey(x => x.SubscriberId);
        builder
            .HasMany(x => x.Payments)
            .WithOne(x => x.Payer)
            .HasForeignKey(x => x.PayerId);
        builder
            .HasMany(x => x.OwnedPlaylists)
            .WithOne(x => x.Creator)
            .HasForeignKey(x => x.CreatorId);
        builder
            .HasMany(x => x.FavoritePlaylists)
            .WithMany()
            .UsingEntity<PlaylistFavoriteEntity>();
    }
}
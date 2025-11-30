using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MusicStreamingService.Data.Entities.Configurations;

public class AlbumFavoriteEntityConfiguration : IEntityTypeConfiguration<AlbumFavoriteEntity>
{
    public void Configure(EntityTypeBuilder<AlbumFavoriteEntity> builder)
    {
        builder.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId);
        builder.HasOne(x => x.Album).WithMany().HasForeignKey(x => x.AlbumId);
        
        builder.HasKey(x => new {x.AlbumId, x.UserId});
    }
}
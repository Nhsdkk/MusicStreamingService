using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MusicStreamingService.Data.Constraints;
using MusicStreamingService.Data.Entities.Configurations.Base;

namespace MusicStreamingService.Data.Entities.Configurations;

internal sealed class GenreEntityConfiguration : BaseUpdatableEntityConfiguration<GenreEntity>
{
    protected override void OnConfigure(EntityTypeBuilder<GenreEntity> builder)
    {
        builder.Property(x => x.Title).IsRequired().HasMaxLength(GenreEntityConstraints.TitleMaxLength);
        builder.Property(x => x.Description).IsRequired();

        builder.HasIndex(x => x.Title).IsUnique();

        builder
            .HasMany(x => x.Songs)
            .WithMany(x => x.Genres)
            .UsingEntity<SongGenreEntity>();
    }
}
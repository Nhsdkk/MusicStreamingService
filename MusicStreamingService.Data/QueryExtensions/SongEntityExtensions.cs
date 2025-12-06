using Microsoft.EntityFrameworkCore;
using MusicStreamingService.Data.Entities;

namespace MusicStreamingService.Data.QueryExtensions;

public static class SongEntityExtensions
{
    public static IQueryable<SongEntity> FilterByOptionalGenres(
        this IQueryable<SongEntity> query,
        List<Guid>? genres) =>
        genres is null
            ? query
            : query
                .Where(s =>
                    s.Genres.Any(g => genres.Contains(g.Id)));

    public static IQueryable<SongEntity> FilterByOptionalTitle(
        this IQueryable<SongEntity> query,
        string? title) =>
        title is null
            ? query
            : query
                .Where(s =>
                    EF.Functions.ILike(s.Title, $"%{title}%"));

    public static IQueryable<SongEntity> FilterByOptionalArtistName(
        this IQueryable<SongEntity> query,
        string? artistName) =>
        artistName is null
            ? query
            : query
                .Where(s =>
                    s.Artists.Any(a =>
                        EF.Functions.ILike(a.Artist.Username, $"%{artistName}%")
                    )
                );
    
    public static IQueryable<SongEntity> EnableExplicit(
        this IQueryable<SongEntity> query,
        bool? enableExplicit) =>
        enableExplicit is null
            ? query.Where(s => !s.Explicit)
            : query;
}
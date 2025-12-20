using Microsoft.EntityFrameworkCore;
using MusicStreamingService.Data.Entities;

namespace MusicStreamingService.Data.QueryExtensions;

public static class PlaylistEntityExtensions
{
    public static IQueryable<PlaylistEntity> FilterByOptionalTitle(
        this IQueryable<PlaylistEntity> query,
        string? title) =>
        title is null
            ? query
            : query.Where(playlist => EF.Functions.ILike(playlist.Title, $"%{title}%"));

    public static IQueryable<PlaylistEntity> FilterByOptionalGenres(
        this IQueryable<PlaylistEntity> query,
        List<Guid>? genreIds) =>
        genreIds is null
            ? query
            : query.Where(playlist => playlist.Songs
                .Any(playlistSong => playlistSong.Song.Genres
                    .Any(songGenre => genreIds.Contains(songGenre.Id))));
    
    public static IQueryable<PlaylistEntity> FilterAccess(
        this IQueryable<PlaylistEntity> query,
        Guid userId) =>
        query.Where(playlist =>
            playlist.AccessType == PlaylistAccessType.Public ||
            playlist.CreatorId == userId);
}
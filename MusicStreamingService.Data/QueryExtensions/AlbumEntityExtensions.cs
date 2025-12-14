using Microsoft.EntityFrameworkCore;
using MusicStreamingService.Data.Entities;
using MusicStreamingService.Infrastructure.DateUtils;

namespace MusicStreamingService.Data.QueryExtensions;

public static class AlbumEntityExtensions
{
    public static IQueryable<AlbumEntity> FilterByOptionalAlbumCreator(
        this IQueryable<AlbumEntity> query,
        string? artistName) =>
        artistName is null ? 
            query : 
            query.Where(x => EF.Functions.ILike(x.Artist.Username, $"%{artistName}%"));
    
    public static IQueryable<AlbumEntity> FilterByOptionalTitle(
        this IQueryable<AlbumEntity> query,
        string? title) =>
        title is null
            ? query
            : query
                .Where(a =>
                    EF.Functions.ILike(a.Title, $"%{title}%"));

    public static IQueryable<AlbumEntity> FilterByOptionalReleaseDate(
        this IQueryable<AlbumEntity> query,
        DateRange? releaseDateRange)
    {
        if (releaseDateRange is null)
        {
            return query;
        }   
        
        if (releaseDateRange.Start is not null)
        {
            query = query.Where(a => a.ReleaseDate >= releaseDateRange.Start);
        }
        
        if (releaseDateRange.End is not null)
        {
            query = query.Where(a => a.ReleaseDate <= releaseDateRange.End);
        }

        return query;
    }
}
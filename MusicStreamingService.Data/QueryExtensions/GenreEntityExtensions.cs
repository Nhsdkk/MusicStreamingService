using Microsoft.EntityFrameworkCore;
using MusicStreamingService.Data.Entities;

namespace MusicStreamingService.Data.QueryExtensions;

public static class GenreEntityExtensions
{
    public static IQueryable<GenreEntity> FilterByOptionalTitle(
        this IQueryable<GenreEntity> query,
        string? title) =>
        title is null ? query : query.Where(g => EF.Functions.ILike(g.Title, $"%{title}%"));
}
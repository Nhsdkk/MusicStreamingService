using Microsoft.EntityFrameworkCore;
using MusicStreamingService.Data.Entities;

namespace MusicStreamingService.Data.QueryExtensions;

public static class RegionEntityExtensions
{
    public static IQueryable<RegionEntity> FilterByOptionalTitle(
        this IQueryable<RegionEntity> query,
        string? title) =>
        title is null
            ? query
            : query
                .Where(s =>
                    EF.Functions.ILike(s.Title, $"%{title}%"));
}
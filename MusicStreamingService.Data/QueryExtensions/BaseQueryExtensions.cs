namespace MusicStreamingService.Data.QueryExtensions;

public static class BaseQueryExtensions
{
    public static IQueryable<T> ApplyPagination<T>(
        this IQueryable<T> query,
        int itemsPerPage,
        int page) =>
        query.Skip(itemsPerPage * page).Take(itemsPerPage);
}
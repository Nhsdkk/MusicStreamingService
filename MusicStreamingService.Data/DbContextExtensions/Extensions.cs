using Microsoft.EntityFrameworkCore;
using MusicStreamingService.Common.Result;

namespace MusicStreamingService.Data.DbContextExtensions;

public static class Extensions
{
    public static TransactionBuilder<T> InitializeTransaction<T>(this DbContext dbContext) where T: IResult =>
        new TransactionBuilder<T>(dbContext);
}
using Microsoft.EntityFrameworkCore;
using MusicStreamingService.Common.Result;

namespace MusicStreamingService.Data;

public sealed class Transaction<T>
    where T: IResult
{
    private readonly DbContext _dbContext;
    private readonly TransactionHandler<T> _handler;

    internal Transaction(DbContext dbContext, TransactionHandler<T> handler)
    {
        _dbContext = dbContext;
        _handler = handler;
    }
    
    public async ValueTask<T> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var result = await _handler(cancellationToken);

            if (result.IsError)
            {
                await transaction.RollbackAsync(cancellationToken);
                return result;
            }

            await transaction.CommitAsync(cancellationToken);
            return result;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
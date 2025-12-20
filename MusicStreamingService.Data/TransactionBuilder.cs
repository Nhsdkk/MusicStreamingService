using Microsoft.EntityFrameworkCore;
using MusicStreamingService.Common.Result;

namespace MusicStreamingService.Data;

public delegate ValueTask<T> TransactionHandler<T>(CancellationToken cancellationToken) where T: IResult;

public sealed class TransactionBuilder<T>
    where T : IResult
{
    private readonly DbContext _dbContext;
    private TransactionHandler<T>? _handler;

    internal TransactionBuilder(DbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public TransactionBuilder<T> WithHandler(TransactionHandler<T> handler)
    {
        _handler = handler;
        return this;
    }

    public Transaction<T> Build()
    {
        if (_handler is null)
        {
            throw new Exception("Transaction handler is not set.");
        }
        
        return new Transaction<T>(_dbContext, _handler);
    }
}
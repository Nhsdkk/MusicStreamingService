using Mediator;
using MusicStreamingService.Data;

namespace MusicStreamingService.Commands;

public interface ITransactionWrappedCommand<out T> : IRequest<T>
{
}

public sealed class TransactionalPipelineBehavior<T1, T2> : IPipelineBehavior<T1, T2>
    where T1 : ITransactionWrappedCommand<T2>
{
    private readonly MusicStreamingContext _context;

    public TransactionalPipelineBehavior(
        MusicStreamingContext context)
    {
        _context = context;
    }

    public async ValueTask<T2> Handle(
        T1 message,
        MessageHandlerDelegate<T1, T2> next,
        CancellationToken cancellationToken)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var response = await next(message, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return response;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
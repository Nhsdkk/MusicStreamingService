using Mediator;
using MusicStreamingService.Data;
using MusicStreamingService.Infrastructure.Result;
using IResult = MusicStreamingService.Infrastructure.Result.IResult;

namespace MusicStreamingService.Commands;

public interface ITransactionWrappedCommand<TD> : IRequest<TD>
    where TD : IResult
{
}

public sealed class TransactionalPipelineBehavior<TCommand, TResponseData>
    : IPipelineBehavior<TCommand, TResponseData>
    where TCommand : ITransactionWrappedCommand<TResponseData>
    where TResponseData : IResult

{
    private readonly MusicStreamingContext _context;

    public TransactionalPipelineBehavior(
        MusicStreamingContext context)
    {
        _context = context;
    }

    public async ValueTask<TResponseData> Handle(
        TCommand message,
        MessageHandlerDelegate<TCommand, TResponseData> next,
        CancellationToken cancellationToken)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var response = await next(message, cancellationToken);

            if (response.IsError)
            {
                await transaction.RollbackAsync(cancellationToken);
                return response;
            }
            
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
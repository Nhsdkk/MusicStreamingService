using Mediator;
using MusicStreamingService.Data;
using MusicStreamingService.Data.DbContextExtensions;
using IResult = MusicStreamingService.Common.Result.IResult;

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

    public ValueTask<TResponseData> Handle(
        TCommand message,
        MessageHandlerDelegate<TCommand, TResponseData> next,
        CancellationToken cancellationToken) =>
        _context.InitializeTransaction<TResponseData>()
            .WithHandler(ct => next(message, ct))
            .Build()
            .ExecuteAsync(cancellationToken);
}
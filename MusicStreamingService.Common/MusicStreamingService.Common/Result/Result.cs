namespace MusicStreamingService.Common.Result;

public interface IResult
{
    public bool IsError { get; }
    public bool IsSuccess { get; }
}

public delegate TOut MatchBranch<in TIn, out TOut>(TIn data);

public sealed class Result<TData> : IResult
{
    private readonly Exception? _exception;
    private readonly TData? _data;

    private Result(TData data) => _data = data;

    private Result(Exception exception) => _exception = exception;

    public static implicit operator Result<TData>(TData v) => new(v);

    public static implicit operator Result<TData>(Exception v) => new(v);

    public bool IsError => _exception is not null;

    public bool IsSuccess => _exception is null;

    public TData Success() => _data!;

    public Exception Error() => _exception!;

    public TOut Match<TOut>(
        MatchBranch<TData, TOut> onSuccess,
        MatchBranch<Exception, TOut> onError) =>
        IsSuccess ? onSuccess(_data!) : onError(_exception!);
}
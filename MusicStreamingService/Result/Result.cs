using OneOf;

namespace MusicStreamingService.Result;

public sealed class Result<T1, T2> : OneOfBase<T1, T2>
{
    public Result(OneOf<T1, T2> input) : base(input)
    {
    }

    public static implicit operator Result<T1, T2>(T1 v) => new(v);
        
    public static implicit operator Result<T1, T2>(T2 v) => new(v);
}
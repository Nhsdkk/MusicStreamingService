using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace MusicStreamingService.Data.Converters;

internal sealed class EnumStringConverter<T> : ValueConverter<T, string>
    where T: struct 
{
    public EnumStringConverter()
        : base(
            v => v.ToString()!,
            v => Enum.Parse<T>(v))
    {
    }
}
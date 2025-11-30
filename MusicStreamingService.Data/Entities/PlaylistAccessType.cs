using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace MusicStreamingService.Data.Entities;

public enum PlaylistAccessType
{
    /// <summary>
    /// Anyone can access playlist
    /// </summary>
    Public,
    
    /// <summary>
    /// Only creator can access playlist
    /// </summary>
    Private
}

internal sealed class PlaylistAccessTypeConverter : ValueConverter<PlaylistAccessType, string>
{
    public PlaylistAccessTypeConverter() :
        base(
            v => v.ToString(),
            v => Enum.Parse<PlaylistAccessType>(v))
    {
        
    }
}
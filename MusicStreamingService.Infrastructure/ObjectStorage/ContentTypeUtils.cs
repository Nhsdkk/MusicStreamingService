namespace MusicStreamingService.Infrastructure.ObjectStorage;

public static class ContentTypeUtils
{
    public static string GetFileExtensionByContentType(string contentType) =>
        contentType.ToLower() switch
        {
            "image/jpeg" => "jpeg",
            "image/png" => "png",
            "audio/mpeg" => "mp3",
            _ => throw new ArgumentOutOfRangeException(nameof(contentType), $"Unsupported content type: {contentType}")
        };
}
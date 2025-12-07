using Microsoft.Extensions.Options;
using Minio;
using MusicStreamingService.Infrastructure.Result;

namespace MusicStreamingService.Infrastructure.ObjectStorage;

public interface IAlbumStorageService
{
    public Task<Result<string, Exception>> GetPresignedUrl(
        string albumArtworkFileName);
}

public sealed class AlbumStorageService : IAlbumStorageService
{
    private readonly IMinioClient _client;
    private readonly MinioConfiguration _configuration;

    public AlbumStorageService(
        IMinioClient client,
        IOptions<MinioConfiguration> configuration)
    {
        _client = client;
        _configuration = configuration.Value;
    }

    public async Task<Result<string, Exception>> GetPresignedUrl(string albumArtworkFileName) =>
        await _client.GetPresignedUrl(Buckets.AlbumCoverBucketName, albumArtworkFileName, _configuration.ExpireTimeInSeconds);
}
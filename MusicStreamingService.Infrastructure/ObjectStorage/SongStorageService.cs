using Microsoft.Extensions.Options;
using Minio;
using MusicStreamingService.Infrastructure.Result;

namespace MusicStreamingService.Infrastructure.ObjectStorage;

public interface ISongStorageService
{
    public Task<Result<string, Exception>> GetPresignedUrl(
        string songFileName);
}

public sealed class SongStorageService : ISongStorageService
{
    private readonly IMinioClient _client;
    private readonly MinioConfiguration _configuration;

    public SongStorageService(
        IMinioClient client,
        IOptions<MinioConfiguration> configuration)
    {
        _client = client;
        _configuration = configuration.Value;
    }

    public async Task<Result<string, Exception>> GetPresignedUrl(string songFileName) =>
        await _client.GetPresignedUrl(Buckets.SongBucketName, songFileName, _configuration.ExpireTimeInSeconds);
}
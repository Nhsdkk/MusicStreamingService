using Microsoft.Extensions.Options;
using Minio;
using MusicStreamingService.Infrastructure.Result;

namespace MusicStreamingService.Infrastructure.ObjectStorage;

public interface ISongStorageService
{
    public Task<Result<string>> GetPresignedUrl(
        string songFileName);
    
    public Task<Result<string>> UploadSong(
        string songFileName,
        Stream songData);
}

public sealed class SongStorageService : ISongStorageService
{
    private const string ContentType = "audio/mpeg";
    private readonly IMinioClient _client;
    private readonly MinioConfiguration _configuration;

    public SongStorageService(
        IMinioClient client,
        IOptions<MinioConfiguration> configuration)
    {
        _client = client;
        _configuration = configuration.Value;
    }

    public async Task<Result<string>> GetPresignedUrl(string songFileName) =>
        await _client.GetPresignedUrl(Buckets.SongBucketName, songFileName, _configuration.ExpireTimeInSeconds);

    public async Task<Result<string>> UploadSong(
        string songFileName, 
        Stream songData)
    {
        await _client.UploadObject(Buckets.SongBucketName, songFileName, songData, ContentType);
        return await GetPresignedUrl(songFileName);
    }
}
using Microsoft.Extensions.Options;
using Minio;
using MusicStreamingService.Infrastructure.Result;

namespace MusicStreamingService.Infrastructure.ObjectStorage;

public interface IAlbumStorageService
{
    public Task<Result<string>> GetPresignedUrl(
        string albumArtworkFileName);

    public Task<Dictionary<string, string?>> GetPresignedUrls(
        IEnumerable<string> albumArtworkFileNames);
    
    public Task<Result<string>> UploadAlbumArtwork(
        string albumArtworkFileName,
        string contentType,
        Stream albumArtworkData);
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

    public async Task<Result<string>> GetPresignedUrl(string albumArtworkFileName) =>
        await _client.GetPresignedUrl(Buckets.AlbumCoverBucketName, albumArtworkFileName,
            _configuration.ExpireTimeInSeconds);

    public async Task<Dictionary<string, string?>> GetPresignedUrls(
        IEnumerable<string> albumArtworkFileNames)
    {
        var uniqueFileNames = albumArtworkFileNames.Distinct();
        var results = await Task.WhenAll(uniqueFileNames.Select(async fileName =>
        {
            var urlResult = await GetPresignedUrl(fileName);
            return urlResult.Match(
                url => new KeyValuePair<string, string?>(fileName, url),
                _ => new KeyValuePair<string, string?>(fileName, null));
        }));

        return new Dictionary<string, string?>(results);
    }

    public async Task<Result<string>> UploadAlbumArtwork(
        string albumArtworkFileName,
        string contentType,
        Stream albumArtworkData)
    {
        await _client.UploadObject(Buckets.AlbumCoverBucketName, albumArtworkFileName, albumArtworkData, contentType);
        return await GetPresignedUrl(albumArtworkFileName);
    }
}
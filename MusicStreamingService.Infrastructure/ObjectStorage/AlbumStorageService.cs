using System.Reactive;
using Microsoft.Extensions.Options;
using Minio;
using MusicStreamingService.Common.Result;

namespace MusicStreamingService.Infrastructure.ObjectStorage;

public interface IAlbumStorageService
{
    public Task<Result<string>> GetPresignedUrl(
        string albumArtworkFileName,
        CancellationToken cancellationToken = default);

    public Task<Dictionary<string, string?>> GetPresignedUrls(
        IEnumerable<string> albumArtworkFileNames,
        CancellationToken cancellationToken = default);

    public Task<Result<string>> UploadAlbumArtwork(
        string albumArtworkFileName,
        string contentType,
        Stream albumArtworkData,
        CancellationToken cancellationToken = default);

    public Task<Result<Unit>> DeleteAlbumArtwork(
        string albumArtworkFileName,
        CancellationToken cancellationToken = default);
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

    public Task<Result<string>> GetPresignedUrl(
        string albumArtworkFileName,
        CancellationToken cancellationToken = default) =>
        _client.GetPresignedUrl(
            Buckets.AlbumCoverBucketName,
            albumArtworkFileName,
            _configuration.ExpireTimeInSeconds,
            cancellationToken);

    public async Task<Dictionary<string, string?>> GetPresignedUrls(
        IEnumerable<string> albumArtworkFileNames,
        CancellationToken cancellationToken = default)
    {
        var uniqueFileNames = albumArtworkFileNames.Distinct();
        var results = await Task.WhenAll(uniqueFileNames.Select(async fileName =>
        {
            var urlResult = await GetPresignedUrl(fileName, cancellationToken);
            return urlResult.Match(
                url => new KeyValuePair<string, string?>(fileName, url),
                _ => new KeyValuePair<string, string?>(fileName, null));
        }));

        return new Dictionary<string, string?>(results);
    }

    public async Task<Result<string>> UploadAlbumArtwork(
        string albumArtworkFileName,
        string contentType,
        Stream albumArtworkData,
        CancellationToken cancellationToken = default)
    {
        await _client.UploadObject(
            Buckets.AlbumCoverBucketName,
            albumArtworkFileName,
            albumArtworkData,
            contentType,
            cancellationToken);
        return await GetPresignedUrl(albumArtworkFileName, cancellationToken);
    }

    public Task<Result<Unit>> DeleteAlbumArtwork(
        string albumArtworkFileName,
        CancellationToken cancellationToken = default) =>
        _client.RemoveObjects(
            Buckets.AlbumCoverBucketName,
            [albumArtworkFileName],
            cancellationToken);
}
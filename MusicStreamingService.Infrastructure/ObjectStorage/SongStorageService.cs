using System.Reactive;
using Microsoft.Extensions.Options;
using Minio;
using MusicStreamingService.Common.Result;

namespace MusicStreamingService.Infrastructure.ObjectStorage;

public interface ISongStorageService
{
    public Task<Result<string>> GetPresignedUrl(
        string songFileName,
        CancellationToken cancellationToken = default);

    public Task<Dictionary<string, string?>> GetPresignedUrls(
        List<string> songFileNames,
        CancellationToken cancellationToken = default);

    public Task<Result<string>> UploadSong(
        string songFileName,
        Stream songData,
        CancellationToken cancellationToken = default);

    public Task<Result<Unit>> DeleteSongs(
        List<string> songFileNames,
        CancellationToken cancellationToken = default);
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

    public Task<Result<string>> GetPresignedUrl(
        string songFileName,
        CancellationToken cancellationToken = default) =>
        _client.GetPresignedUrl(
            Buckets.SongBucketName,
            songFileName,
            _configuration.ExpireTimeInSeconds,
            cancellationToken);

    public async Task<Dictionary<string, string?>> GetPresignedUrls(
        List<string> songFileNames,
        CancellationToken cancellationToken = default)
    {
        var uniqueFileNames = songFileNames.Distinct().ToList();

        var presignedUrlTasks = uniqueFileNames.Select(async fname =>
        {
            var urlResult = await GetPresignedUrl(fname, cancellationToken);
            return urlResult.Match(
                url => new KeyValuePair<string, string?>(fname, url),
                _ => new KeyValuePair<string, string?>(fname, null));
        });

        var result = await Task.WhenAll(presignedUrlTasks);
        return result.ToDictionary();
    }

    public async Task<Result<string>> UploadSong(
        string songFileName,
        Stream songData,
        CancellationToken cancellationToken = default)
    {
        await _client.UploadObject(Buckets.SongBucketName, songFileName, songData, ContentType, cancellationToken);
        return await GetPresignedUrl(songFileName, cancellationToken);
    }

    public Task<Result<Unit>> DeleteSongs(
        List<string> songFileNames,
        CancellationToken cancellationToken = default) =>
        _client.RemoveObjects(
            Buckets.SongBucketName,
            songFileNames,
            cancellationToken);
}
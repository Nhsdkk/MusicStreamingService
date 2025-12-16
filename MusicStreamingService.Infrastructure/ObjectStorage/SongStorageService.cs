using System.Reactive;
using Microsoft.Extensions.Options;
using Minio;
using MusicStreamingService.Infrastructure.Result;

namespace MusicStreamingService.Infrastructure.ObjectStorage;

public interface ISongStorageService
{
    public Task<Result<string>> GetPresignedUrl(
        string songFileName);

    public Task<Dictionary<string, string?>> GetPresignedUrls(
        List<string> songFileNames);

    public Task<Result<string>> UploadSong(
        string songFileName,
        Stream songData);

    public Task<Result<Unit>> DeleteSongs(
        List<string> songFileNames);
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

    public async Task<Dictionary<string, string?>> GetPresignedUrls(List<string> songFileNames)
    {
        var uniqueFileNames = songFileNames.Distinct().ToList();

        var presignedUrlTasks = uniqueFileNames.Select(async fname =>
        {
            var urlResult = await GetPresignedUrl(fname);
            return urlResult.Match(
                url => new KeyValuePair<string, string?>(fname, url),
                _ => new KeyValuePair<string, string?>(fname, null));
        });

        var result = await Task.WhenAll(presignedUrlTasks);
        return result.ToDictionary();
    }

    public async Task<Result<string>> UploadSong(
        string songFileName,
        Stream songData)
    {
        await _client.UploadObject(Buckets.SongBucketName, songFileName, songData, ContentType);
        return await GetPresignedUrl(songFileName);
    }

    public async Task<Result<Unit>> DeleteSongs(List<string> songFileNames) =>
        await _client.RemoveObjects(Buckets.SongBucketName, songFileNames);
}
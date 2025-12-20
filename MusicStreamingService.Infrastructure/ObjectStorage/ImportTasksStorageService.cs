using Minio;
using MusicStreamingService.Common.Result;

namespace MusicStreamingService.Infrastructure.ObjectStorage;

public interface IImportTasksStorageService
{
    public Task UploadImportTaskStagingFileAsync(
        string filename,
        Stream fileStream,
        CancellationToken cancellationToken = default);

    public Task<Result<MemoryStream>> DownloadImportTask(
        string filename,
        CancellationToken cancellationToken = default);
}

public sealed class ImportTasksStorageService : IImportTasksStorageService
{
    private readonly IMinioClient _client;

    public ImportTasksStorageService(IMinioClient client)
    {
        _client = client;
    }

    public Task UploadImportTaskStagingFileAsync(
        string filename,
        Stream fileStream,
        CancellationToken cancellationToken = default) =>
        _client.UploadObject(
            Buckets.ImportTasksBucketName,
            filename,
            fileStream,
            contentType: "application/json");

    public Task<Result<MemoryStream>> DownloadImportTask(
        string filename,
        CancellationToken cancellationToken = default) =>
        _client.GetObjectStream(Buckets.ImportTasksBucketName, filename, cancellationToken);
}
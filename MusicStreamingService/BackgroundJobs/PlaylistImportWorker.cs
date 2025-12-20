using System.Text.Json.Serialization;
using Mediator;
using Microsoft.EntityFrameworkCore;
using MusicStreamingService.Data;
using MusicStreamingService.Data.Entities;
using MusicStreamingService.Infrastructure.ObjectStorage;
using MusicStreamingService.Common.Result;
using MusicStreamingService.Data.DbContextExtensions;

namespace MusicStreamingService.BackgroundJobs;

public sealed record ImportFileContent
{
    public sealed record ImportFileSong
    {
        [JsonPropertyName("title")]
        public string Title { get; init; } = null!;

        [JsonPropertyName("artist")]
        public string ArtistName { get; init; } = null!;

        [JsonPropertyName("albumName")]
        public string AlbumName { get; init; } = null!;

        [JsonPropertyName("releaseDate")]
        public DateOnly ReleaseDate { get; init; }
        
        [JsonPropertyName("playlistIndex")]
        public int PlaylistIndex { get; init; }
    }

    [JsonPropertyName("playlistNames")]
    public List<string> PlaylistNames { get; init; } = new();

    [JsonPropertyName("songs")]
    public List<ImportFileSong> Songs { get; init; } = new();
}

public class PlaylistImportWorker : BackgroundService
{
    private readonly ILogger _logger;
    private readonly IServiceProvider _services;
    private const int BatchSize = 300;
    private const double MatchingThreshold = 0.3;

    public PlaylistImportWorker(ILogger<PlaylistImportWorker> logger, IServiceProvider services)
    {
        _services = services;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Starting playlist import job at: {time}", DateTimeOffset.Now);
            await ImportPlaylistsAsync(stoppingToken);
            _logger.LogInformation("Finished playlist import job at: {time}", DateTimeOffset.Now);
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }

    private async Task ImportPlaylistsAsync(CancellationToken cancellationToken = default)
    {
        await using var scope = _services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<MusicStreamingContext>();
        var importTasksStorageService = scope.ServiceProvider.GetRequiredService<IImportTasksStorageService>();

        var tasksQuery = dbContext.PlaylistImportTasks
            .AsNoTracking()
            .Where(x => x.Status == PlaylistImportTaskStatus.Created);

        var count = await tasksQuery.CountAsync(cancellationToken);
        if (count == 0)
        {
            _logger.LogInformation("No playlist import tasks found.");
            return;
        }

        _logger.LogInformation("Found {count} playlist import tasks.", count);

        var task = await tasksQuery.AsTracking().FirstAsync(cancellationToken);

        _logger.LogInformation("Processing playlist import task with ID: {taskId}", task.Id);

        task.Status = PlaylistImportTaskStatus.Processing;
        await dbContext.SaveChangesAsync(cancellationToken);

        var result =
            await ProcessPlaylistImportTaskAsync(dbContext, importTasksStorageService, task, cancellationToken);

        if (result.IsError)
        {
            _logger.LogError(
                result.Error(),
                "Failed to process playlist import task with ID: {taskId}",
                task.Id);

            return;
        }

        _logger.LogInformation("Completed playlist import task with ID: {taskId}", task.Id);
    }

    private async ValueTask<Result<Unit>> ProcessPlaylistImportTaskAsync(
        MusicStreamingContext dbContext,
        IImportTasksStorageService importTasksStorageService,
        PlaylistImportTaskEntity task,
        CancellationToken cancellationToken)
    {
        var importFileStream = await importTasksStorageService
            .DownloadImportTask(task.S3FileName, cancellationToken);

        if (importFileStream.IsError)
        {
            task.Status = PlaylistImportTaskStatus.Finished;
            await dbContext.SaveChangesAsync(cancellationToken);
            return importFileStream.Error();
        }

        var fileContentResult = ParseFile(importFileStream.Success());
        if (fileContentResult.IsError)
        {
            task.Status = PlaylistImportTaskStatus.Finished;
            await dbContext.SaveChangesAsync(cancellationToken);
            return fileContentResult.Error();
        }
        
        var fileContent = fileContentResult.Success();

        var playlists = fileContent.PlaylistNames
            .Select(title => new PlaylistEntity
            {
                Title = title,
                Description = null,
                CreatorId = task.CreatorId,
                AccessType = PlaylistAccessType.Private,
            }).ToList();

        await dbContext.Playlists.AddRangeAsync(playlists, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        
        for (var startIndex = 0; startIndex < task.TotalEntries; startIndex += BatchSize)
        {
            var batchId = Guid.NewGuid();
            var batchSongs = fileContent.Songs
                .Skip(startIndex)
                .Take(BatchSize)
                .Select(s => new PlaylistImportStagingEntity
                {
                    BatchId = batchId,
                    ImportTaskId = task.Id,
                    SongTitle = s.Title,
                    AlbumName = s.AlbumName,
                    ArtistName = s.ArtistName,
                    ReleaseDate = s.ReleaseDate,
                    Status = StagingStatus.Pending,
                    PlaylistId = playlists[s.PlaylistIndex].Id,
                }).ToList();

            await dbContext.PlaylistImportStagingEntries.AddRangeAsync(batchSongs, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
            await ExecuteBatchMatching(dbContext, batchId, cancellationToken);
        }
        
        task.Status = PlaylistImportTaskStatus.Finished;
        await dbContext.SaveChangesAsync(cancellationToken);
        
        return Unit.Value;
    }
    
    private Task ExecuteBatchMatching(
        MusicStreamingContext dbContext,
        Guid batchId,
        CancellationToken cancellationToken) => dbContext.Database
        .ExecuteSqlAsync($"call match_songs_in_playlist_import_batch({batchId}, {MatchingThreshold});", cancellationToken);

    private Result<ImportFileContent> ParseFile(MemoryStream fileStream)
    {
        var reader = fileStream.ToArray();

        var result = System.Text.Json.JsonSerializer.Deserialize<ImportFileContent>(reader);
        if (result == null)
        {
            return new Exception("Failed to parse import file content.");
        }

        return result;
    }
}
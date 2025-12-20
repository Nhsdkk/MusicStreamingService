using System.Text.Json.Serialization;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MusicStreamingService.Data;
using MusicStreamingService.Data.Entities;
using MusicStreamingService.Data.QueryExtensions;
using MusicStreamingService.Extensions;
using MusicStreamingService.Infrastructure.Authentication;
using MusicStreamingService.Openapi;
using MusicStreamingService.Requests;
using MusicStreamingService.Responses;
using MusicStreamingService.Validators;

namespace MusicStreamingService.Features.PlaylistImportTasks;

[ApiController]
public class Get : ControllerBase
{
    private readonly IMediator _mediator;

    public Get(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get user's playlist import tasks
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("/api/v1/import-tasks")]
    [Tags(RouteGroups.PlaylistImportTasks)]
    [Authorize(Roles = Permissions.ManageAlbumsPermission)]
    [ProducesResponseType<QueryResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTasks(
        [FromQuery] Query.QueryBody request,
        CancellationToken cancellationToken = default)
    {
        var response = await _mediator.Send(new Query
        {
            UserId = User.GetUserId(),
            Body = request,
        }, cancellationToken);

        return Ok(response);
    }

    public sealed record Query : IRequest<QueryResponse>
    {
        public sealed record QueryBody : BasePaginatedRequest
        {
        }

        public sealed class Validator : BasePaginatedRequestValidator<QueryBody>
        {
            public Validator()
            {
            }
        }

        public Guid UserId { get; init; }

        public QueryBody Body { get; init; } = null!;
    }

    public sealed record QueryResponse : BasePaginatedResponse
    {
        public sealed record ImportTaskDto
        {
            [JsonPropertyName("id")]
            public Guid Id { get; init; }

            [JsonPropertyName("progress")]
            public double Progress { get; init; }

            [JsonPropertyName("status")]
            public PlaylistImportTaskStatus Status { get; init; }

            [JsonPropertyName("totalCount")]
            public long TotalCount { get; init; }
            
            [JsonPropertyName("processedCount")]
            public long ProcessedCount { get; init; }

            [JsonPropertyName("successCount")]
            public long SuccessCount { get; init; }

            [JsonPropertyName("FailedCount")]
            public long FailedCount { get; init; }
        }

        [JsonPropertyName("importTasks")]
        public List<ImportTaskDto> ImportTasks { get; init; } = null!;
    }

    public sealed class Handler : IRequestHandler<Query, QueryResponse>
    {
        private readonly MusicStreamingContext _context;

        public Handler(MusicStreamingContext context)
        {
            _context = context;
        }

        public async ValueTask<QueryResponse> Handle(Query request, CancellationToken cancellationToken)
        {
            var requestBody = request.Body;

            var totalCount = await _context.PlaylistImportTasks
                .Where(x => x.CreatorId == request.UserId)
                .CountAsync(cancellationToken);

            var tasks = await _context.PlaylistImportStagingEntries
                .AsNoTracking()
                .Where(x => x.ImportTask.CreatorId == request.UserId)
                .GroupBy(x => new
                {
                    x.ImportTaskId,
                    x.ImportTask.Status,
                    x.ImportTask.TotalEntries,
                    x.ImportTask.ProcessedEntries,
                    x.ImportTask.CreatedAt
                })
                .Select(grp => new
                {
                    grp.Key.ImportTaskId,
                    grp.Key.Status,
                    grp.Key.ProcessedEntries,
                    Progress = grp.Key.TotalEntries == 0
                        ? 100
                        : (int)((double)grp.Key.ProcessedEntries /
                            grp.Key.TotalEntries * 100),
                    grp.Key.TotalEntries,
                    grp.Key.CreatedAt,
                    FailedCount = grp.Count(p => p.Status == StagingStatus.Failed),
                    SuccessCount = grp.Count(p => p.Status == StagingStatus.Matched),
                })
                .OrderByDescending(x => x.CreatedAt)
                .ApplyPagination(requestBody.ItemsPerPage, requestBody.Page)
                .ToListAsync(cancellationToken);

            return new QueryResponse
            {
                TotalCount = totalCount,
                ItemsPerPage = requestBody.ItemsPerPage,
                ItemCount = tasks.Count,
                Page = requestBody.Page,
                ImportTasks = tasks.Select(x => new QueryResponse.ImportTaskDto
                {
                    Id = x.ImportTaskId,
                    TotalCount = x.TotalEntries,
                    FailedCount = x.FailedCount,
                    Progress = x.Progress,
                    Status = x.Status,
                    SuccessCount = x.SuccessCount,
                    ProcessedCount = x.ProcessedEntries,
                }).ToList()
            };
        }
    }
}
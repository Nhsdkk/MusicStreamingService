using System.Text.Json.Serialization;
using FluentValidation;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MusicStreamingService.Data;
using MusicStreamingService.Data.Entities;
using MusicStreamingService.Data.QueryExtensions;
using MusicStreamingService.Openapi;
using MusicStreamingService.Requests;
using MusicStreamingService.Responses;
using MusicStreamingService.Validators;

namespace MusicStreamingService.Features.Region;

[ApiController]
public sealed class Search : ControllerBase
{
    private readonly IMediator _mediator;

    public Search(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("/api/v1/regions")]
    [Tags(RouteGroups.Regions)]
    [ProducesResponseType<QueryResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchRegions(
        [FromQuery] Query request,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            request,
            cancellationToken);

        return Ok(result);
    }

    public sealed record Query : BasePaginatedRequest, IRequest<QueryResponse>
    {
        [JsonPropertyName("title")]
        public string? Title { get; init; }

        public sealed class Validator : BasePaginatedRequestValidator<Query>
        {
            public Validator()
            {
                RuleFor(x => x.Title)
                    .NotEmpty()
                    .MaximumLength(255)
                    .When(x => x.Title is not null);
            }
        }
    }

    public sealed record QueryResponse : BasePaginatedResponse
    {
        [JsonPropertyName("regions")]
        public List<RegionDto> Regions { get; init; } = null!;

        public static QueryResponse FromEntity(
            List<RegionEntity> regions,
            int totalCount,
            Query request) =>
            new QueryResponse
            {
                Regions = regions
                    .Select(RegionDto.FromEntity)
                    .ToList(),
                TotalCount = totalCount,
                ItemsPerPage = request.ItemsPerPage,
                Page = request.Page,
                ItemCount = regions.Count,
            };
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
            var query = _context.Regions
                .AsNoTracking()
                .FilterByOptionalTitle(request.Title);

            var totalCount = await query.CountAsync(cancellationToken);
            var regions = await query
                .ApplyPagination(request.ItemsPerPage, request.Page)
                .ToListAsync(cancellationToken);

            return QueryResponse.FromEntity(regions, totalCount, request);
        }
    }
}
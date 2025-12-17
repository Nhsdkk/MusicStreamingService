using System.Text.Json.Serialization;
using FluentValidation;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MusicStreamingService.Data;
using MusicStreamingService.Extensions;
using MusicStreamingService.Infrastructure.Authentication;
using MusicStreamingService.Infrastructure.ObjectStorage;
using MusicStreamingService.Infrastructure.Result;
using MusicStreamingService.Openapi;

namespace MusicStreamingService.Features.Songs;

[ApiController]
public class GetSongUrl : ControllerBase
{
    private readonly IMediator _mediator;

    public GetSongUrl(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    /// <summary>
    /// Get song url for streaming
    /// </summary>
    /// <param name="query">The query containing the song ID to get the streaming URL for.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("/api/v1/songs/url")]
    [Tags(RouteGroups.Songs)]
    [Authorize(Roles = Permissions.PlaySongsPermission)]
    [ProducesResponseType<QueryResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<Exception>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetUrl(
        [FromQuery] Query.QueryBody query,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new Query
            {
                Body = query,
                UserRegion = User.GetUserRegion()
            },
            cancellationToken);

        return result.Match<IActionResult>(Ok, BadRequest);
    }
    
    public sealed record Query : IRequest<Result<QueryResponse>>
    {
        public sealed record QueryBody
        {
            [JsonPropertyName("songId")]
            public Guid SongId { get; init; }
        }
        
        public QueryBody Body { get; init; } = null!;
        
        public RegionClaim UserRegion { get; init; } = null!;
        
        public sealed class Validator : AbstractValidator<QueryBody>
        {
            public Validator()
            {
                RuleFor(x => x.SongId).NotEmpty();
            }
        }
    }
    
    public sealed record QueryResponse
    {
        [JsonPropertyName("url")]
        public string Url { get; init; } = null!;
    }
    
    public sealed class Handler : IRequestHandler<Query, Result<QueryResponse>>
    {
        private readonly MusicStreamingContext _context;
        private readonly ISongStorageService _songStorageService;

        public Handler(
            MusicStreamingContext context,
            ISongStorageService songStorageService)
        {
            _context = context;
            _songStorageService = songStorageService;
        }

        public async ValueTask<Result<QueryResponse>> Handle(
            Query request,
            CancellationToken cancellationToken)
        {
            var song = await _context.Songs
                .Include(x => x.AllowedRegions)
                .SingleOrDefaultAsync(x => x.Id == request.Body.SongId, cancellationToken);

            if (song is null)
            {
                return new Exception("Song not found");
            }
            
            if (song.AllowedRegions.All(x => x.Id != request.UserRegion.Id))
            {
                return new Exception("Song is not available in your region");
            }
            
            var s3SongPath = song.S3MediaFileName;
            var songUrlGetResult = await _songStorageService.GetPresignedUrl(s3SongPath);
            return songUrlGetResult.Match<Result<QueryResponse>>(
                url => new QueryResponse { Url = url },
                _ => new Exception("Failed to get song URL"));
        }
    }
}
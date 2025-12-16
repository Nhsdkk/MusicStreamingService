using System.Text.Json.Serialization;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using MusicStreamingService.Commands;
using MusicStreamingService.Data.Entities;
using MusicStreamingService.Features.Songs;
using MusicStreamingService.Infrastructure.Result;
using MusicStreamingService.Openapi;

namespace MusicStreamingService.Features.Playlists;

[ApiController]
public class Create : ControllerBase
{
    private readonly IMediator _mediator;

    public Create(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("/api/v1/playlists")]
    [Tags(RouteGroups.Playlists)]
    [ProducesResponseType<CommandResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<Exception>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreatePlaylist(
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new Command(),
            cancellationToken);

        return result.Match<IActionResult>(Ok, BadRequest);
    }

    public sealed record Command : ITransactionWrappedCommand<Result<CommandResponse>>
    {
        public sealed record CommandBody
        {
            public sealed record PlaylistSong
            {
                [JsonPropertyName("songId")]
                public Guid SongId { get; init; }

                [JsonPropertyName("position")]
                public long Position { get; init; }
            }

            [JsonPropertyName("title")]
            public string Title { get; init; } = null!;

            [JsonPropertyName("description")]
            public string? Description { get; init; }

            [JsonPropertyName("accessType")]
            public PlaylistAccessType AccessType { get; init; }

            [JsonPropertyName("songs")]
            public List<PlaylistSong> Songs { get; init; } = null!;
        }

        public CommandBody Body { get; init; } = null!;

        public Guid UserId { get; init; }
    }

    public sealed record CommandResponse
    {
        public sealed record PlaylistCreator
        {
            public Guid Id { get; init; }
            
            public string Username { get; init; } = null!;
        }
        
        [JsonPropertyName("id")]
        public Guid Id { get; init; }
        
        [JsonPropertyName("title")]
        public string Title { get; init; } = null!;
        
        [JsonPropertyName("description")]
        public string? Description { get; init; }

        [JsonPropertyName("creator")]
        public PlaylistCreator Creator { get; init; } = null!;
        
        [JsonPropertyName("accessType")]
        public PlaylistAccessType AccessType { get; init; }
        
        [JsonPropertyName("likes")]
        public long Likes { get; init; }

        [JsonPropertyName("songs")]
        public List<ShortSongDto> Songs { get; init; } = null!;
        
        
    }
    
    public sealed class Hander : IRequestHandler<Command, Result<CommandResponse>>
    {
        public ValueTask<Result<CommandResponse>> Handle(
            Command request,
            CancellationToken cancellationToken = default)
        {
            // Implementation goes here
            throw new NotImplementedException();
        }
    }
}
using System.Text.Json.Serialization;
using FluentValidation;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MusicStreamingService.Data;
using MusicStreamingService.Data.Entities;
using MusicStreamingService.Extensions;
using MusicStreamingService.Infrastructure.Authentication;
using MusicStreamingService.Infrastructure.Result;
using MusicStreamingService.Openapi;

namespace MusicStreamingService.Features.StreamingEvents;

[ApiController]
public sealed class Create : ControllerBase
{
    private readonly IMediator _mediator;

    public Create(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    /// <summary>
    /// Create streaming event
    /// </summary>
    /// <param name="request">Data about the event</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost("/api/v1/streaming-events")]
    [Tags(RouteGroups.StreamingEvents)]
    [Authorize(Roles = Permissions.PlaySongsPermission)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<Exception>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateStreamingEvent(
        [FromBody] Command.CommandBody request,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new Command
            {
                Body = request,
                UserId = User.GetUserId()
            },
            cancellationToken);
        
        return result.Match<IActionResult>(_ => Ok(), BadRequest);
    }

    public sealed record Command : IRequest<Result<Unit>>
    {
        public sealed record CommandBody
        {
            [JsonPropertyName("songId")]
            public Guid SongId { get; init; }
            
            [JsonPropertyName("deviceId")]
            public Guid DeviceId { get; init; }
            
            [JsonPropertyName("positionMs")]
            public long PositionMs { get; init; }
            
            [JsonPropertyName("timePlayedSinceLastRequestMs")]
            public long TimePlayedSinceLastRequestMs { get; init; }
            
            [JsonPropertyName("eventType")]
            public StreamingEventType EventType { get; init; }

            public sealed class Validator : AbstractValidator<CommandBody>
            {
                public Validator()
                {
                    RuleFor(x => x.SongId).NotEmpty();
                    RuleFor(x => x.DeviceId).NotEmpty();
                    RuleFor(x => x.PositionMs).GreaterThanOrEqualTo(0);
                    RuleFor(x => x.TimePlayedSinceLastRequestMs).GreaterThanOrEqualTo(0);
                }
            }
        }
        
        public Guid UserId { get; init; }
        
        public CommandBody Body { get; init; } = null!;
    }
    
    public sealed class Handler : IRequestHandler<Command, Result<Unit>>
    {
        private readonly MusicStreamingContext _context;

        public Handler(MusicStreamingContext context)
        {
            _context = context;
        }

        public async ValueTask<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
        {
            var body = request.Body;
            var device = await _context.Devices.SingleOrDefaultAsync(x => x.Id == body.DeviceId, cancellationToken);

            if (device is null)
            {
                return new Exception("Device not found");
            }

            if (device.OwnerId != request.UserId)
            {
                return new Exception("Device does not belong to the user");
            }

            var song = await _context.Songs.SingleOrDefaultAsync(x => x.Id == body.SongId, cancellationToken);
            if (song is null)
            {
                return new Exception("Song not found");
            }

            var streamingEvent = new StreamingEventEntity
            {
                SongId = body.SongId,
                DeviceId = body.DeviceId,
                PositionMs = body.PositionMs,
                TimePlayedSinceLastRequestMs = body.TimePlayedSinceLastRequestMs,
                EventType = body.EventType
            };
            
            await _context.StreamingEvents.AddAsync(streamingEvent, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
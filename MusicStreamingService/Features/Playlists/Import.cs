using System.Text.Json.Serialization;
using FluentValidation;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicStreamingService.Commands;
using MusicStreamingService.Data;
using MusicStreamingService.Data.Entities;
using MusicStreamingService.Extensions;
using MusicStreamingService.Infrastructure.ObjectStorage;
using MusicStreamingService.Common.Result;
using MusicStreamingService.Infrastructure.Authentication;
using MusicStreamingService.Openapi;

namespace MusicStreamingService.Features.Playlists;

[ApiController]
public class Import : ControllerBase
{
    private readonly IMediator _mediator;

    public Import(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    [HttpPost("/api/v1/playlists/import")]
    [Tags(RouteGroups.Playlists)]
    [Consumes("multipart/form-data")]
    [Authorize(Roles = Permissions.ManagePlaylistsPermission)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<Exception>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ImportPlaylists(
        [FromForm] Command.CommandBody request,
        CancellationToken cancellationToken = default)
    {
        var command = new Command
        {
            Body = request,
            UserId = User.GetUserId()
        };
        await _mediator.Send(command, cancellationToken);
        return Ok();
    }

    public sealed record Command : IRequest<CommandResponse>
    {
        public sealed record CommandBody
        {
            [JsonPropertyName("playlistsFile")]
            public IFormFile PlaylistsFile { get; init; } = null!;
        
            [JsonPropertyName("entriesCount")]
            public long EntriesCount { get; init; }   
            
            public sealed class Validator : AbstractValidator<CommandBody>
            {
                public Validator()
                {
                    RuleFor(x => x.PlaylistsFile)
                        .NotNull()
                        .Must(file => file.ContentType == "application/json")
                        .WithMessage("Playlists file must be a JSON file.");
                
                    RuleFor(x => x.EntriesCount)
                        .GreaterThan(0)
                        .WithMessage("Entries count must be greater than zero.");
                }
            }
        }
        
        public CommandBody Body { get; init; } = null!;
        
        public Guid UserId { get; init; }
    }
    
    public sealed record CommandResponse
    {
        [JsonPropertyName("importTaskId")]
        public Guid ImportTaskId { get; init; }
    }
    
    public sealed class Handler : IRequestHandler<Command, CommandResponse>
    {
        private readonly IImportTasksStorageService _importTasksStorageService;
        private readonly MusicStreamingContext _context;

        public Handler(
            IImportTasksStorageService importTasksStorageService,
            MusicStreamingContext context)
        {
            _importTasksStorageService = importTasksStorageService;
            _context = context;
        }

        public async ValueTask<CommandResponse> Handle(Command request, CancellationToken cancellationToken)
        {
            var filename = $"{Guid.NewGuid()}.json";
            await _importTasksStorageService.UploadImportTaskStagingFileAsync(
                filename,
                request.Body.PlaylistsFile.OpenReadStream(),
                cancellationToken);

            var importTask = new PlaylistImportTaskEntity
            {
                CreatorId = request.UserId,
                Status = PlaylistImportTaskStatus.Created,
                S3FileName = filename,
                TotalEntries = request.Body.EntriesCount,
            };

            await _context.PlaylistImportTasks.AddAsync(importTask, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            
            return new CommandResponse
            {
                ImportTaskId = importTask.Id
            };
        }
    }
}
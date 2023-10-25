using Carter;
using Events.Modules.Shared.Models;
using Events.Modules.Stream.Command.AppendEvent;
using Events.Modules.Stream.Models;
using Events.Modules.Stream.Query.GetEvents;
using Events.Modules.Stream.Query.GetStreams;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Events.Modules.Stream;

public class StreamModule : CarterModule
{
    public StreamModule()
        : base("/stream")
    {
        IncludeInOpenApi();
        WithTags("Stream");
    }

    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/", GetStreams)
            .Produces<PagedListOut<StreamModelOut>>(StatusCodes.Status200OK)
            .WithName("getStreams");

        ;

        app.MapGet("/{streamId}", GetStreamEvents)
            .Produces<PagedListOut<EventModelOut>>(StatusCodes.Status200OK)
            .WithName("getStreamEvents");

        ;

        app.MapPost("/{streamId}", AppendEvent)
            .Produces<EventModelOut>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .WithName("appendEvent");

        ;
    }

    private static async Task<IResult> AppendEvent(ISender sender, string streamId, [FromBody] EventModelIn model)
    {
        var command = new AppendEventCommand
        {
            StreamId = streamId,
            ExpectedVersion = model.ExpectedVersion,
            Type = model.Type,
            Payload64 = model.Payload64,
            EventAt = model.EventAt
        };

        var response = await sender.Send(command);

        if (response == null)
        {
            return Results.BadRequest();
        }

        return Results.Ok(response);
    }

    private static async Task<IResult> GetStreamEvents(
        ISender sender,
        string streamId,
        int page = 1,
        int pageSize = 5,
        int? fromVersion = null
    )
    {
        var query = new GetEventsQuery
            { StreamId = streamId, Page = page, PageSize = pageSize, FromVersion = fromVersion };

        var response = await sender.Send(query);

        return Results.Ok(response);
    }

    private static async Task<IResult> GetStreams(ISender sender, int page = 1, int pageSize = 5)
    {
        var query = new GetStreamsQuery { Page = page, PageSize = pageSize };

        var response = await sender.Send(query);

        return Results.Ok(response);
    }
}

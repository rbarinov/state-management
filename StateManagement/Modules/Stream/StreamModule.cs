using System.Collections.Immutable;
using Carter;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using StateManagement.Modules.Shared.Models;
using StateManagement.Modules.Stream.Command.AppendEvent;
using StateManagement.Modules.Stream.Command.AppendMultipleEvents;
using StateManagement.Modules.Stream.Models;
using StateManagement.Modules.Stream.Query.GetEvents;
using StateManagement.Modules.Stream.Query.GetStreams;

namespace StateManagement.Modules.Stream;

public class StreamModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/streams")
            .WithOpenApi()
            .WithTags("Stream");

        group.MapGet("/", GetStreams)
            .Produces<PagedListOut<StreamModelOut>>(StatusCodes.Status200OK)
            .WithName("getStreams");

        group.MapGet("/{streamId}", GetStreamEvents)
            .Produces<PagedListOut<EventModelOut>>(StatusCodes.Status200OK)
            .WithName("getStreamEvents");

        group.MapPost("/{streamId}", AppendEvent)
            .Produces<EventModelOut>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .WithName("appendEvent");

        group.MapPost("/{streamId}/multiple", AppendMultipleEvents)
            .Produces<IReadOnlyList<EventModelOut>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .WithName("appendMultipleEvents");
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

    private static async Task<IResult> AppendMultipleEvents(
        ISender sender,
        string streamId,
        [FromBody] MultipleEventModelIn model
    )
    {
        var command = new AppendMultipleEventsCommand
        {
            StreamId = streamId,
            ExpectedVersion = model.ExpectedVersion,
            Events = model.Events
                .Select(
                    e => new AppendMultipleEventsItem
                    {
                        Type = e.Type,
                        EventAt = e.EventAt,
                        Payload64 = e.Payload64
                    }
                )
                .ToImmutableList()
        };

        var response = await sender.Send(command);

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

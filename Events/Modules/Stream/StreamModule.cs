using Carter;
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
        app.MapGet(
            "/",
            async (ISender sender, int page = 1, int pageSize = 5) =>
            {
                var query = new GetStreamsQuery
                {
                    Page = page,
                    PageSize = pageSize
                };

                var response = await sender.Send(query);

                return Results.Ok(response);
            }
        );

        app.MapGet(
            "/{streamId}",
            async (
                ISender sender,
                string streamId,
                int page = 1,
                int pageSize = 5,
                int? fromVersion = null
            ) =>
            {
                var query = new GetEventsQuery
                {
                    StreamId = streamId,
                    Page = page,
                    PageSize = pageSize,
                    FromVersion = fromVersion
                };

                var response = await sender.Send(query);

                return Results.Ok(response);
            }
        );

        app.MapPost(
            "/{streamId}",
            async (ISender sender, string streamId, [FromBody] EventModelIn model) =>
            {
                var command = new AppendEventCommand
                {
                    StreamId = streamId,
                    ExpectedVersion = model.ExpectedVersion,
                    Type = model.Type,
                    Payload64 = model.Payload64
                };

                var response = await sender.Send(command);

                if (response == null)
                {
                    return Results.BadRequest();
                }

                return Results.Ok(response);
            }
        );
    }
}

using System.Net;
using Carter;
using Events.Modules.Event.Query.GetEvents;
using Events.Modules.Shared.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Events.Modules.Event;

public class EventModule : CarterModule
{
    public EventModule()
        : base("/events")
    {
        IncludeInOpenApi();
        WithTags("Events");
    }

    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/", GetEvents)
            .Produces<PagedListOut<EventModelOut>>(StatusCodes.Status200OK)
            .WithName("getEvents");
    }

    private static async Task<IResult> GetEvents(
        ISender sender,
        int page = 1,
        int pageSize = 5,
        int? fromGlobalVersion = null
    )
    {
        var query = new GetEventsQuery(page, pageSize, fromGlobalVersion);

        var events = await sender.Send(query);

        return Results.Ok(events);
    }
}

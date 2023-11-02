using Carter;
using MediatR;
using StateManagement.Modules.Event.Query.GetEvents;
using StateManagement.Modules.Shared.Models;

namespace StateManagement.Modules.Event;

public class EventModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/events")
            .WithOpenApi()
            .WithTags("Events");

        group.MapGet("/", GetEvents)
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

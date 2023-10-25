using Carter;
using Events.Modules.Event.Query.GetEvents;
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
        app.MapGet(
            "/",
            async (
                [FromServices] ISender sender,
                int page = 1,
                int pageSize = 5,
                int? fromGlobalVersion = null
            ) =>
            {
                var query = new GetEventsQuery(page, pageSize, fromGlobalVersion);

                var events = await sender.Send(query);

                return Results.Ok(events);
            }
        );
    }
}

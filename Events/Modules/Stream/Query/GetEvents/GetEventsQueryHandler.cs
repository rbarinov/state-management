using Events.Data;
using Events.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Events.Modules.Stream.Query.GetEvents;

public class GetEventsQueryHandler(EventDbContext db)
    : IRequestHandler<GetEventsQuery, List<EventModelOut>>
{
    public async Task<List<EventModelOut>> Handle(
        GetEventsQuery request,
        CancellationToken cancellationToken
    )
    {
        var q = db.Events
            .Where(e => e.StreamId == request.StreamId);

        if (request.FromVersion.HasValue)
        {
            q = q.Where(e => e.Version > request.FromVersion.Value);
        }

        var events = await q
            .OrderBy(e => e.GlobalVersion)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(
                e => new EventModelOut
                {
                    GlobalVersion = e.GlobalVersion,
                    StreamId = e.StreamId,
                    Version = e.Version,
                    Type = e.Type,
                    Payload64 = Convert.ToBase64String(e.Payload)
                }
            )
            .ToListAsync(cancellationToken: cancellationToken);

        return events;
    }
}

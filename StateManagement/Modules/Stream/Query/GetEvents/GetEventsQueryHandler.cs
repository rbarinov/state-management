using MediatR;
using Microsoft.EntityFrameworkCore;
using StateManagement.Data;
using StateManagement.Modules.Shared.Models;

namespace StateManagement.Modules.Stream.Query.GetEvents;

public class GetEventsQueryHandler
    : IRequestHandler<GetEventsQuery, PagedListOut<EventModelOut>>
{
    private readonly StateManagementDbContext _db;

    public GetEventsQueryHandler(StateManagementDbContext db)
    {
        _db = db;
    }

    public async Task<PagedListOut<EventModelOut>> Handle(
        GetEventsQuery request,
        CancellationToken cancellationToken
    )
    {
        var q = _db.Events
            .AsNoTracking()
            .Where(e => e.StreamId == request.StreamId);

        if (request.FromVersion.HasValue)
        {
            q = q.Where(e => e.Version > request.FromVersion.Value);
        }

        var events = await q
            .Select(
                e => new EventModelOut
                {
                    GlobalVersion = e.GlobalVersion,
                    StreamId = e.StreamId,
                    Version = e.Version,
                    Type = e.Type,
                    EventAt = e.EventAt,
                    Payload64 = Convert.ToBase64String(e.Payload)
                }
            )
            .OrderBy(e => e.GlobalVersion)
            .ToPagedListAsync(request.Page, request.PageSize, cancellationToken);

        return events;
    }
}

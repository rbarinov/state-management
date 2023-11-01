using Events.Data;
using Events.Modules.Shared.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Events.Modules.Event.Query.GetEvents;

public class GetEventsQueryHandler
    : IRequestHandler<GetEventsQuery, PagedListOut<EventModelOut>>
{
    private readonly EventDbContext _db;

    public GetEventsQueryHandler(EventDbContext db)
    {
        _db = db;
    }

    public async Task<PagedListOut<EventModelOut>> Handle(
        GetEventsQuery request,
        CancellationToken cancellationToken
    )
    {
        var queryBuilder = _db.Events
            .AsNoTracking()
            .AsQueryable();

        if (request.FromGlobalVersion.HasValue)
        {
            queryBuilder = queryBuilder.Where(e => e.GlobalVersion > request.FromGlobalVersion.Value);
        }

        var paged = await queryBuilder
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

        return paged;
    }
}

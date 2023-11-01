using Events.Data;
using Events.Modules.Shared.Models;
using Events.Modules.Stream.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Events.Modules.Stream.Query.GetStreams;

public class GetStreamsQueryHandle : IRequestHandler<GetStreamsQuery, PagedListOut<StreamModelOut>>
{
    private readonly EventDbContext _db;

    public GetStreamsQueryHandle(EventDbContext db)
    {
        _db = db;
    }

    public async Task<PagedListOut<StreamModelOut>> Handle(GetStreamsQuery request, CancellationToken cancellationToken)
    {
        var streams = await _db.Streams
            .AsNoTracking()
            .Select(
                e => new StreamModelOut
                {
                    StreamId = e.StreamId,
                    Version = e.Version
                }
            )
            .OrderBy(e => e.StreamId)
            .ToPagedListAsync(request.Page, request.PageSize, cancellationToken);

        return streams;
    }
}

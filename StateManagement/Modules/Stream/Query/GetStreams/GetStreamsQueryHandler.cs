using MediatR;
using Microsoft.EntityFrameworkCore;
using StateManagement.Data;
using StateManagement.Modules.Shared.Models;
using StateManagement.Modules.Stream.Models;

namespace StateManagement.Modules.Stream.Query.GetStreams;

public class GetStreamsQueryHandle : IRequestHandler<GetStreamsQuery, PagedListOut<StreamModelOut>>
{
    private readonly StateManagementDbContext _db;

    public GetStreamsQueryHandle(StateManagementDbContext db)
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

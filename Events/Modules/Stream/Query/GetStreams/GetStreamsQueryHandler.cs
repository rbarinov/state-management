using Events.Data;
using Events.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Events.Modules.Stream.Query.GetStreams;

public class GetStreamsQueryHandler(EventDbContext db) : IRequestHandler<GetStreamsQuery, IList<StreamModelOut>>
{
    public async Task<IList<StreamModelOut>> Handle(GetStreamsQuery request, CancellationToken cancellationToken)
    {
        var streams = await db.Streams
            .Select(
                e => new StreamModelOut
                {
                    StreamId = e.StreamId,
                    Version = e.Version
                }
            )
            .ToListAsync(cancellationToken: cancellationToken);

        return streams;
    }
}

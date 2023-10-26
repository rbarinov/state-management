using System.Collections.Immutable;
using Events.Data;
using Events.Modules.Shared.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Events.Modules.Stream.Command.AppendMultipleEvents;

public class AppendMultipleEventsCommandHandler : IRequestHandler<AppendMultipleEventsCommand,
    IReadOnlyList<EventModelOut>?>
{
    private readonly EventDbContext _db;

    public AppendMultipleEventsCommandHandler(EventDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<EventModelOut>?> Handle(
        AppendMultipleEventsCommand request,
        CancellationToken cancellationToken
    )
    {
        StreamDto? stream;

        if (request.ExpectedVersion == -1)
        {
            stream = new StreamDto
            {
                StreamId = request.StreamId,
                Version = -1
            };

            _db.Streams.Add(stream);
        }
        else
        {
            stream = await _db.Streams.FirstOrDefaultAsync(
                e => e.StreamId == request.StreamId,
                cancellationToken: cancellationToken
            );
        }

        if (stream == null || stream.Version != request.ExpectedVersion)
        {
            return default;
        }

        var buffer = new List<(AppendMultipleEventsItem item, EventDto ev)>();

        foreach (var item in request.Events)
        {
            stream.Version++;

            var eventAt = DateTime.SpecifyKind(item.EventAt, DateTimeKind.Utc);

            var ev = new EventDto
            {
                StreamId = request.StreamId,
                Version = stream.Version,
                GlobalVersion = 0,
                Type = item.Type,
                EventAt = eventAt,
                Payload = Convert.FromBase64String(item.Payload64)
            };

            _db.Events.Add(ev);
            buffer.Add((item, ev));
        }

        await _db.SaveChangesAsync(cancellationToken);

        return buffer.Select(
                e =>
                    new EventModelOut
                    {
                        GlobalVersion = e.ev.GlobalVersion,
                        StreamId = request.StreamId,
                        Version = stream.Version,
                        Type = e.item.Type,
                        EventAt = e.item.EventAt,
                        Payload64 = e.item.Payload64
                    }
            )
            .ToImmutableList();
    }
}

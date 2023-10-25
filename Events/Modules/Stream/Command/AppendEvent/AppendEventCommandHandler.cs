using Events.Data;
using Events.Modules.Shared.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Events.Modules.Stream.Command.AppendEvent;

public class AppendEventCommandHandler : IRequestHandler<AppendEventCommand, EventModelOut?>
{
    private readonly EventDbContext _db;

    public AppendEventCommandHandler(EventDbContext db)
    {
        _db = db;
    }

    public async Task<EventModelOut?> Handle(AppendEventCommand request, CancellationToken cancellationToken)
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
            return null;
        }

        stream.Version++;
        var eventAt = DateTime.SpecifyKind(request.EventAt, DateTimeKind.Utc);

        var ev = new EventDto
        {
            StreamId = request.StreamId,
            Version = stream.Version,
            GlobalVersion = 0,
            Type = request.Type,
            EventAt = eventAt,
            Payload = Convert.FromBase64String(request.Payload64)
        };

        _db.Events.Add(ev);

        await _db.SaveChangesAsync(cancellationToken);

        var response = new EventModelOut
        {
            GlobalVersion = ev.GlobalVersion,
            StreamId = request.StreamId,
            Version = stream.Version,
            Type = request.Type,
            EventAt = eventAt,
            Payload64 = request.Payload64
        };

        return response;
    }
}

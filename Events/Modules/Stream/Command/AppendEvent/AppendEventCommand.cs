using Events.Models;
using MediatR;

namespace Events.Modules.Stream.Command.AppendEvent;

public sealed record AppendEventCommand : IRequest<EventModelOut?>
{
    public string StreamId { get; init; }
    public int ExpectedVersion { get; init; }
    public required string Type { get; init; }
    public required string Payload64 { get; init; }
}

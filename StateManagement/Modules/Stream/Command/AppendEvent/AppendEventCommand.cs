using MediatR;
using StateManagement.Modules.Shared.Models;

namespace StateManagement.Modules.Stream.Command.AppendEvent;

public sealed record AppendEventCommand : IRequest<EventModelOut?>
{
    public required string StreamId { get; init; }
    public required int ExpectedVersion { get; init; }
    public required string Type { get; init; }
    public required string Payload64 { get; init; }
    public required DateTime EventAt { get; init; }
}

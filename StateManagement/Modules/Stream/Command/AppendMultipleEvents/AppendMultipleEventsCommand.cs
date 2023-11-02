using MediatR;
using StateManagement.Modules.Shared.Models;

namespace StateManagement.Modules.Stream.Command.AppendMultipleEvents;

public record AppendMultipleEventsCommand : IRequest<IReadOnlyList<EventModelOut>>
{
    public required string StreamId { get; init; }
    public required int ExpectedVersion { get; init; }
    public required IReadOnlyList<AppendMultipleEventsItem> Events { get; init; }
}
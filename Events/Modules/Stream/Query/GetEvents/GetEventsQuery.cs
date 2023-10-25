using Events.Modules.Shared.Models;
using MediatR;

namespace Events.Modules.Stream.Query.GetEvents;

public sealed record GetEventsQuery : IRequest<PagedListOut<EventModelOut>>
{
    public required string StreamId { get; set; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int? FromVersion { get; init; }
}

using Events.Models;
using MediatR;

namespace Events.Modules.Stream.Query.GetEvents;

public sealed record GetEventsQuery : IRequest<List<EventModelOut>>
{
    public string StreamId { get; set; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int? FromVersion { get; init; }
}

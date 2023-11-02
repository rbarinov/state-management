using MediatR;
using StateManagement.Modules.Shared.Models;

namespace StateManagement.Modules.Stream.Query.GetEvents;

public sealed record GetEventsQuery : IRequest<PagedListOut<EventModelOut>>
{
    public required string StreamId { get; set; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int? FromVersion { get; init; }
}

using Events.Modules.Shared.Models;
using Events.Modules.Stream.Models;
using MediatR;

namespace Events.Modules.Stream.Query.GetStreams;

public record GetStreamsQuery : IRequest<PagedListOut<StreamModelOut>>
{
    public int Page { get; init; }
    public int PageSize { get; init; }
}

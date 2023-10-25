using Events.Models;
using MediatR;

namespace Events.Modules.Stream.Query.GetStreams;

public record GetStreamsQuery : IRequest<IList<StreamModelOut>>
{
    public int Page { get; init; }
    public int PageSize { get; init; }
}

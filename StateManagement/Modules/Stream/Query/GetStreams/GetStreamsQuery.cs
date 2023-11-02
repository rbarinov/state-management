using MediatR;
using StateManagement.Modules.Shared.Models;
using StateManagement.Modules.Stream.Models;

namespace StateManagement.Modules.Stream.Query.GetStreams;

public record GetStreamsQuery : IRequest<PagedListOut<StreamModelOut>>
{
    public int Page { get; init; }
    public int PageSize { get; init; }
}

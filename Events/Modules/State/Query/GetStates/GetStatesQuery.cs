using Events.Modules.Shared.Models;
using Events.Modules.State.Models;
using MediatR;

namespace Events.Modules.State.Query.GetStates;

public class GetStatesQuery : IRequest<PagedListOut<StateInfoModelOut>>
{
    public int Page { get; init; }
    public int PageSize { get; init; }
}

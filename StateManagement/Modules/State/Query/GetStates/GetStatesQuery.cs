using MediatR;
using StateManagement.Modules.Shared.Models;
using StateManagement.Modules.State.Models;

namespace StateManagement.Modules.State.Query.GetStates;

public class GetStatesQuery : IRequest<PagedListOut<StateInfoModelOut>>
{
    public int Page { get; init; }
    public int PageSize { get; init; }
}

using MediatR;
using StateManagement.Modules.State.Models;

namespace StateManagement.Modules.State.Query.GetState;

public class GetStateQuery : IRequest<StateFullModelOut?>
{
    public required string Key { get; set; }
}

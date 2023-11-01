using Events.Modules.State.Models;
using MediatR;

namespace Events.Modules.State.Query.GetState;

public class GetStateQuery : IRequest<StateFullModelOut?>
{
    public required string Key { get; set; }
}

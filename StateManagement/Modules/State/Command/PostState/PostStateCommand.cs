using MediatR;
using StateManagement.Modules.State.Models;

namespace StateManagement.Modules.State.Command.PostState;

public class PostStateCommand : IRequest<StateFullModelOut>
{
    public required string Key { get; set; }

    public int? ReferenceVersion { get; set; }
    
    public required DateTime UpdatedAt { get; set; }

    public required string Payload64 { get; init; }
}

using Events.Modules.State.Models;
using MediatR;

namespace Events.Modules.State.Command.PostState;

public class PostStateCommand : IRequest<StateFullModelOut>
{
    public required string Key { get; set; }

    public int? ReferenceVersion { get; set; }
    
    public required DateTime UpdatedAt { get; set; }

    public required string Payload64 { get; init; }
}

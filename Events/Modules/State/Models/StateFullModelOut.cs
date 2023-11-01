namespace Events.Modules.State.Models;

public class StateFullModelOut : StateInfoModelOut
{
    public required string Payload64 { get; init; }
}
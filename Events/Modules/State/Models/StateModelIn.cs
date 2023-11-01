namespace Events.Modules.State.Models;

public class StateModelIn
{
    public int? ReferenceVersion { get; set; }
    public required DateTime UpdatedAt { get; init; }
    public required string Payload64 { get; init; }
}

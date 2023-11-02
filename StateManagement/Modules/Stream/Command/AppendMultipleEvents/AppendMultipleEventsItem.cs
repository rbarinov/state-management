namespace StateManagement.Modules.Stream.Command.AppendMultipleEvents;

public record AppendMultipleEventsItem
{
    public required string Type { get; init; }
    public required string Payload64 { get; init; }
    public required DateTime EventAt { get; init; }
}

namespace StateManagement.Modules.Stream.Models;

public record MultipleEventModelItemIn
{
    public required string Type { get; init; }
    public required DateTime EventAt { get; init; }
    public required string Payload64 { get; init; }
}

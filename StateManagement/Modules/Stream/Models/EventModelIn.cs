namespace StateManagement.Modules.Stream.Models;

public record EventModelIn
{
    public int ExpectedVersion { get; init; }
    public required string Type { get; init; }
    public required DateTime EventAt { get; init; }
    public required string Payload64 { get; init; }
}
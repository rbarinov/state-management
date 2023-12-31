namespace StateManagement.Modules.Stream.Models;

public record StreamModelOut
{
    public required string StreamId { get; init; }
    public required int Version { get; init; }
}

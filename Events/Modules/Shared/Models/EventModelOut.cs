namespace Events.Modules.Shared.Models;

public record EventModelOut
{
    public required int GlobalVersion { get; init; }
    public required string StreamId { get; init; }
    public required int Version { get; init; }
    public required string Type { get; init; }
    public required DateTime EventAt { get; set; }
    public required string Payload64 { get; init; }
}

namespace StateManagement.Modules.Stream.Models;

public record MultipleEventModelIn
{
    public required int ExpectedVersion { get; init; }
    public required List<MultipleEventModelItemIn> Events { get; set; }
}

namespace Events.Data;

public class EventDto
{
    public int GlobalVersion { get; set; }

    public required string StreamId { get; set; }

    public StreamDto Stream { get; set; } = null!;

    public required int Version { get; set; }

    public required DateTime EventAt { get; set; }

    public required string Type { get; set; }

    public required byte[] Payload { get; set; }
}

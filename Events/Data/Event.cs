namespace Events.Data;

public class Event
{
    public int GlobalVersion { get; set; }

    public required string StreamId { get; set; }

    public Stream Stream { get; set; } = null!;

    public int Version { get; set; }

    public required string Type { get; set; }

    public required byte[] Payload { get; set; }
}

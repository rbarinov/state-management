namespace Events.Data;

public class Stream
{
    public required string StreamId { get; set; }

    public int Version { get; set; }

    public ICollection<Event> Events { get; set; } = new List<Event>();
}

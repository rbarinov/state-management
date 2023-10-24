namespace Events.Data;

public class StreamDto
{
    public required string StreamId { get; set; }

    public int Version { get; set; }

    public ICollection<EventDto> Events { get; set; } = new List<EventDto>();
}

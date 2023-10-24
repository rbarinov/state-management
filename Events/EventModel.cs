public class EventModel
{
    public int ExpectedVersion { get; set; }
    public required string Type { get; set; }
    public required string Payload64 { get; set; }
}

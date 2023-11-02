namespace StateManagement.Data.Entities;

public class StateDto
{
    public required string Key { get; set; }

    public int? ReferenceVersion { get; set; }

    public required DateTime UpdatedAt { get; set; }

    public required byte[] Payload { get; set; }
}

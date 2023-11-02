namespace StateManagement.Modules.State.Models;

public class StateInfoModelOut
{
    public required string Key { get; set; }

    public int? ReferenceVersion { get; set; }

    public DateTime? UpdatedAt { get; set; }
}

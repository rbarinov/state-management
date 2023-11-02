namespace EventImporter;

public class Configuration
{
    public string ConnectionString { get; set; }
    public int ReadChannelCapacity { get; set; }
    public int BufferSize { get; set; }
    public int MaxLiveQueueSize { get; set; }
    public int ReadBatchSize { get; set; }
    public string StateManagementUrl { get; set; } = null!;
    public int WriteThreads { get; set; }
    public int WriteBatchSize { get; set; }
}

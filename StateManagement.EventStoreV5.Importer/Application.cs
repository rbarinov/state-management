using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using EventStore.ClientAPI;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EventImporter;

public class Application : BackgroundService
{
    private readonly IHostApplicationLifetime _applicationLifetime;
    private readonly EventsClient _client;
    private readonly ILogger<Application> _logger;
    private readonly EventStoreReader _reader;
    private readonly Configuration _configuration;

    public Application(
        IHostApplicationLifetime applicationLifetime,
        EventsClient client,
        ILogger<Application> logger,
        EventStoreReader reader,
        Configuration configuration
    )
    {
        _applicationLifetime = applicationLifetime;
        _client = client;
        _logger = logger;
        _reader = reader;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken
    )
    {
        var eventStoreConnection = EventStoreConnection.Create(
            _configuration.ConnectionString
        );

        await eventStoreConnection.ConnectAsync();

        await _reader.ExtractEventsAndProcess(
            eventStoreConnection,
            _configuration.ReadChannelCapacity,
            _configuration.BufferSize,
            _configuration.MaxLiveQueueSize,
            _configuration.ReadBatchSize,
            async buffer =>
            {
                var groupsByStream = buffer.GroupBy(e => e.StreamId)
                    .Select(
                        g => (streamId: g.Key, batchedEvents: g
                                .Select((e, i) => (index: i, value: e))
                                .GroupBy(e => e.index / _configuration.WriteBatchSize, e => e.value)
                                .Select(g2 => g2.ToList())
                                .ToList()
                            )
                    )
                    .ToList();

                await groupsByStream.Select(
                        e => (streamId: e.streamId, streamObservable: e.batchedEvents
                                .Select(
                                    batch => Observable.FromAsync(
                                            () => stoppingToken.IsCancellationRequested
                                                ? Task.FromResult<ICollection<EventModelOut>>(default!)
                                                : _client.AppendMultipleEventsAsync(
                                                    e.streamId,
                                                    new MultipleEventModelIn
                                                    {
                                                        ExpectedVersion = batch.First()
                                                            .Version - 1,
                                                        Events = batch.Select(
                                                                c => new MultipleEventModelItemIn
                                                                {
                                                                    Type = c.Type,
                                                                    EventAt = c.EventAt,
                                                                    Payload64 = c.Payload64
                                                                }
                                                            )
                                                            .ToList()
                                                    },
                                                    stoppingToken
                                                )
                                        )
                                        .IgnoreElements()
                                        .Cast<Unit>()
                                        .Catch<Unit, Exception>(
                                            ex =>
                                            {
                                                _logger.LogError($"Failed to invoke API with an error: {ex}");

                                                return Observable.Empty<Unit>();
                                            }
                                        )
                                )
                                .Concat()
                            )
                    )
                    .ToObservable()
                    .GroupBy(
                        e => e.streamId.ToCharArray()
                            .Sum(c => c) % _configuration.WriteThreads,
                        e => e.streamObservable
                    )
                    .Select(
                        groupConcurrency => groupConcurrency
                            .Concat()
                    )
                    .Merge()
                    .LastOrDefaultAsync()
                    .ToTask(stoppingToken);
            },
            stoppingToken
        );

        _applicationLifetime.StopApplication();
    }
}

public record EventContainer
{
    public required string StreamId { get; init; }
    public required int Version { get; init; }
    public required DateTime EventAt { get; init; }
    public required string Type { get; init; }
    public required string Payload64 { get; init; }
}

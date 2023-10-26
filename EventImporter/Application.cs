using System.Reactive.Linq;
using System.Text;
using EventStore.ClientAPI;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace EventImporter;

public class Application : BackgroundService
{
    private readonly IHostApplicationLifetime _applicationLifetime;
    private readonly EventsClient _client;
    private readonly ILogger<Application> _logger;
    private readonly EventStoreReader _reader;

    public Application(
        IHostApplicationLifetime applicationLifetime,
        EventsClient client,
        ILogger<Application> logger,
        EventStoreReader reader
    )
    {
        _applicationLifetime = applicationLifetime;
        _client = client;
        _logger = logger;
        _reader = reader;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken
    )
    {
        var eventStoreConnection = EventStoreConnection.Create(
            "ConnectTo=tcp://admin:changeit@localhost:1113; HeartBeatTimeout=500"
            // "ConnectTo=tcp://admin:changeit@192.168.1.165:1113; HeartBeatTimeout=500"
        );

        await eventStoreConnection.ConnectAsync();

        await _reader.ExtractEventsAndProcess(
            eventStoreConnection,
            2048,
            2048,
            async buffer =>
            {
                await buffer
                    .GroupBy(
                        e => e.StreamId.ToCharArray()
                            .Sum(c => c) % 16
                    )
                    .Select(
                        group => group
                            .Select(
                                e => Observable.FromAsync(
                                        () =>
                                            _client.AppendEventAsync(
                                                e.StreamId,
                                                new EventModelIn()
                                                {
                                                    Type = e.Type,
                                                    EventAt = e.EventAt,
                                                    ExpectedVersion = e.Version - 1,
                                                    Payload64 = e.Payload64
                                                },
                                                stoppingToken
                                            )
                                    )
                                    .Catch<EventModelOut, ApiException>(
                                        ex =>
                                        {
                                            _logger.LogError(
                                                $"got error: {ex.StatusCode} {ex.Message} for {JsonConvert.SerializeObject(e with {
                                                    Payload64 = Encoding.UTF8.GetString(Convert.FromBase64String(e.Payload64)) })}"
                                            );

                                            return Observable.Empty<EventModelOut>();
                                        }
                                    )
                            )
                            .Merge(1)
                    )
                    .Merge()
                    .LastOrDefaultAsync();
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

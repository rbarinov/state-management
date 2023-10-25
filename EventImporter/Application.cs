using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using EventStore.ClientAPI;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NYActor.EventSourcing.EventStore.v5;

namespace EventImporter;

public class Application : BackgroundService
{
    private readonly IHostApplicationLifetime _applicationLifetime;
    private readonly EventsClient _client;
    private readonly ILogger<Application> _logger;

    public Application(
        IHostApplicationLifetime applicationLifetime,
        EventsClient client,
        ILogger<Application> logger
    )
    {
        _applicationLifetime = applicationLifetime;
        _client = client;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken
    )
    {
        var eventStoreConnection = EventStoreConnection.Create(
            "ConnectTo=tcp://admin:changeit@localhost:1113; HeartBeatTimeout=500"
        );

        await eventStoreConnection.ConnectAsync();

        var allEvents = Observable.Create<EventContainer>(
            observer =>
            {
                var subscription = eventStoreConnection.SubscribeToAllFrom(
                    Position.Start,
                    new CatchUpSubscriptionSettings(512, 512, false, false),
                    eventAppeared: (subscription, ese) =>
                    {
                        var currentCommitPosition = ese.OriginalPosition?.CommitPosition ?? default;
                        var currentPreparePosition = ese.OriginalPosition?.PreparePosition ?? default;

                        var streamId = ese.OriginalStreamId;
                        var bytes = ese.Event.Data;
                        var type = ese.Event.EventType;
                        var version = (int)ese.Event.EventNumber;
                        var eventAt = ese.Event.Created;

                        var payload64 = Convert.ToBase64String(bytes);

                        observer.OnNext(
                            new EventContainer
                            {
                                StreamId = streamId,
                                Version = version,
                                EventAt = eventAt,
                                Type = type,
                                Payload64 = payload64
                            }
                        );
                    },
                    liveProcessingStarted: c =>
                    {
                        _logger.LogWarning($"got liveProcessingStarted: {c}");
                        observer.OnCompleted();
                    },
                    subscriptionDropped: (sub, res, ex) =>
                    {
                        if (res == SubscriptionDropReason.UserInitiated)
                        {
                            _logger.LogWarning($"subscription closed: {res}");

                            return;
                        }

                        _logger.LogCritical($"got error: {res} {ex}");

                        if (ex is not null)
                        {
                            observer.OnError(ex);
                        }
                    }
                );

                return Disposable.Create(
                    () => subscription.Stop()
                );
            }
        );

        await allEvents
            .Where(e => !e.StreamId.StartsWith("$"))
            .GroupBy(
                e => e.StreamId.ToCharArray()
                    .Sum(c => c) % Environment.ProcessorCount
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

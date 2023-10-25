using System.Reactive.Disposables;
using System.Reactive.Linq;
using EventStore.ClientAPI;
using Microsoft.Extensions.Hosting;
using NYActor.EventSourcing.EventStore.v5;

namespace EventImporter;

public class Application : BackgroundService
{
    private readonly IHostApplicationLifetime _applicationLifetime;
    private readonly EventClient _client;

    public Application(
        IHostApplicationLifetime applicationLifetime,
        EventClient client
    )
    {
        _applicationLifetime = applicationLifetime;
        _client = client;
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
                        var payload64 = Convert.ToBase64String(bytes);

                        observer.OnNext(
                            new EventContainer
                            {
                                StreamId = streamId,
                                Version = version,
                                Type = type,
                                Payload64 = payload64
                            }
                        );
                    },
                    liveProcessingStarted: c =>
                        observer.OnCompleted(),
                    subscriptionDropped: (sub, res, ex) =>
                        observer.OnError(ex)
                );

                return Disposable.Create(
                    () => subscription.Stop()
                );
            }
        );

        await allEvents
            .LastOrDefaultAsync();

        _applicationLifetime.StopApplication();
    }
}

public record EventContainer
{
    public required string StreamId { get; init; }
    public required int Version { get; init; }
    public required string Type { get; init; }
    public required string Payload64 { get; init; }
}

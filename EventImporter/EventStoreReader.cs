using System.Threading.Channels;
using EventStore.ClientAPI;
using Microsoft.Extensions.Logging;

namespace EventImporter;

public class EventStoreReader
{
    private readonly ILogger<EventStoreReader> _logger;

    public EventStoreReader(ILogger<EventStoreReader> logger)
    {
        _logger = logger;
    }

    public async Task ExtractEventsAndProcess(
        IEventStoreConnection eventStoreConnection,
        int channelCapacity,
        int bufferSize,
        Func<IList<EventContainer>, Task> processEvents,
        CancellationToken cancellationToken
    )
    {
        var channel = Channel.CreateBounded<EventContainer>(
            new BoundedChannelOptions(channelCapacity)
            {
                FullMode = BoundedChannelFullMode.Wait,
                SingleReader = true,
                SingleWriter = true
            }
        );

        var reader = channel.Reader;

        var readerTask = Task.Run(
            async () =>
            {
                var buffer = new List<EventContainer>();

                while (!cancellationToken.IsCancellationRequested)
                {
                    buffer.Clear();

                    while (buffer.Count < bufferSize && !cancellationToken.IsCancellationRequested)
                    {
                        try
                        {
                            var item = await reader.ReadAsync();

                            buffer.Add(item);
                        }
                        catch (OperationCanceledException)
                        {
                            break;
                        }
                        catch (ChannelClosedException)
                        {
                            break;
                        }
                    }

                    if (buffer.Count == 0) break;

                    _logger.LogDebug($"staring buffer processing {buffer.Count}");

                    await processEvents(buffer);

                    _logger.LogDebug($"finished buffer processing {buffer.Count}");
                }

                _logger.LogInformation($"exiting worker thread");
            }
        );

        var subscription = eventStoreConnection.SubscribeToAllFrom(
            Position.Start,
            new CatchUpSubscriptionSettings(512, 512, false, false),
            eventAppeared: async (subscription, ese) =>
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    channel.Writer.Complete();
                    return;
                }
                
                var streamId = ese.OriginalStreamId;

                if (streamId.StartsWith("$"))
                {
                    return;
                }

                // var commitPosition = ese.OriginalPosition?.CommitPosition ?? default;
                // var preparePosition = ese.OriginalPosition?.PreparePosition ?? default;

                var bytes = ese.Event.Data;
                var type = ese.Event.EventType;
                var version = (int)ese.Event.EventNumber;
                var eventAt = ese.Event.Created;

                var payload64 = Convert.ToBase64String(bytes);
                
                await channel.Writer.WriteAsync(
                    new EventContainer
                    {
                        StreamId = streamId,
                        Version = version,
                        EventAt = eventAt,
                        Type = type,
                        Payload64 = payload64
                    }
                );

                _logger.LogDebug("wrote to channel");
            },
            liveProcessingStarted: c =>
            {
                _logger.LogWarning($"got liveProcessingStarted: {c}");
                channel.Writer.Complete();
                // observer.OnCompleted();
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
                    throw ex;
                    // observer.OnError(ex);
                }
            }
        );

        await channel.Reader.Completion;
        await readerTask;

        subscription.Stop();
    }
}

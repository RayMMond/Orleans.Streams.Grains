using Orleans.Providers.Streams.Common;
using Orleans.Runtime;

// ReSharper disable InconsistentNaming

namespace Orleans.Streams.Grains;

[Serializable]
[GenerateSerializer]
[Alias("Orleans.Streams.Grains.GrainsQueueBatchContainer")]
public class GrainsQueueBatchContainer : IBatchContainer
{
    [Id(0)]
    private EventSequenceTokenV2? sequenceToken;

    [Id(1)]
    private readonly List<object> events;

    [Id(2)]
    private readonly Dictionary<string, object>? requestContext;

    [Id(3)]
    public StreamId StreamId { get; }

    public StreamSequenceToken SequenceToken => sequenceToken!;

    internal EventSequenceTokenV2 RealSequenceToken
    {
        set => sequenceToken = value;
    }

    public GrainsQueueBatchContainer(
        StreamId streamId,
        List<object> events,
        Dictionary<string, object> requestContext,
        EventSequenceTokenV2 sequenceToken)
        : this(streamId, events, requestContext)
    {
        this.sequenceToken = sequenceToken;
    }

    public GrainsQueueBatchContainer(StreamId streamId, List<object>? events,
        Dictionary<string, object>? requestContext)
    {
        StreamId = streamId;
        this.events = events ?? throw new ArgumentNullException(nameof(events), "Message contains no events");
        this.requestContext = requestContext;
    }

    public IEnumerable<Tuple<T, StreamSequenceToken>> GetEvents<T>()
    {
        return events.OfType<T>().Select((e, i) =>
            Tuple.Create<T, StreamSequenceToken>(e, sequenceToken!.CreateSequenceTokenForEvent(i)));
    }

    public bool ImportRequestContext()
    {
        if (requestContext == null) return false;
        RequestContextExtensions.Import(requestContext);
        return true;
    }

    public override string ToString()
    {
        return $"[{nameof(GrainsQueueBatchContainer)}:Stream={StreamId},#Items={events.Count},#Token={SequenceToken}]";
    }
}
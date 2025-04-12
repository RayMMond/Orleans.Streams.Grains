namespace Orleans.Streams.Grains;

[RegisterConverter]
public sealed class QueueGrainStateSurrogateConverter : IConverter<QueueGrainState, QueueGrainStateSurrogate>
{
    public QueueGrainState ConvertFromSurrogate(in QueueGrainStateSurrogate surrogate)
    {
        return new QueueGrainState
        {
            LastReadMessage = surrogate.LastReadMessage,
            Messages = new Queue<GrainsQueueBatchContainer>(surrogate.Messages),
            PendingMessages = new Queue<GrainsQueueBatchContainer>(surrogate.PendingMessages),
            DroppedMessages = new Queue<GrainsQueueBatchContainer>(surrogate.DroppedMessages)
        };
    }

    public QueueGrainStateSurrogate ConvertToSurrogate(in QueueGrainState value)
    {
        return new QueueGrainStateSurrogate
        {
            LastReadMessage = value.LastReadMessage,
            Messages = value.Messages.ToList(),
            PendingMessages = value.PendingMessages.ToList(),
            DroppedMessages = value.DroppedMessages.ToList()
        };
    }
}
namespace Orleans.Streams.Grains;

[GenerateSerializer]
[Serializable]
[Alias("Orleans.Streams.Grains.QueueGrainStateSurrogate")]
public struct QueueGrainStateSurrogate
{
    [Id(0)]
    public long LastReadMessage;

    [Id(1)]
    public List<GrainsQueueBatchContainer> Messages;

    [Id(2)]
    public List<GrainsQueueBatchContainer> PendingMessages;

    [Id(3)]
    public List<GrainsQueueBatchContainer> DroppedMessages;
}
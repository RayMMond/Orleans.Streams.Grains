namespace Orleans.Streams.Grains;

[Serializable]
public class QueueGrainState
{
    public long LastReadMessage { get; set; }

    public Queue<GrainsQueueBatchContainer> Messages { get; set; } = new();

    public Queue<GrainsQueueBatchContainer> PendingMessages { get; set; } = new();
    
    public Queue<GrainsQueueBatchContainer> DroppedMessages { get; set; } = new();
}
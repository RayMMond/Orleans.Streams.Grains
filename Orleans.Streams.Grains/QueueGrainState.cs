namespace Orleans.Streams.Grains;

[Serializable]
public class QueueGrainState
{
    public long LastReadMessage { get; set; }

    public Queue<GrainsQueueBatchContainer> Messages { get; set; } = new();

    public Queue<GrainsQueueBatchContainer> PendingMessages { get; set; } = new();

    public Queue<GrainsQueueBatchContainer> DroppedMessages { get; set; } = new();
}

// This is the surrogate which will act as a stand-in for the foreign type.
// Surrogates should use plain fields instead of properties for better performance.
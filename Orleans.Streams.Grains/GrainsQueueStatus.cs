namespace Orleans.Streams.Grains;

[Serializable]
[GenerateSerializer]
public class GrainsQueueStatus : Dictionary<QueueId, QueueStatus>
{
}
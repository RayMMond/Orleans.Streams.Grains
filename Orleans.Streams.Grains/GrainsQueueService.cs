namespace Orleans.Streams.Grains;

public class GrainsQueueService(IClusterClient client) : IGrainsQueueService
{
    public Task QueueMessageBatchAsync(QueueId queueId, GrainsQueueBatchContainer message)
    {
        return client.GetGrain<IQueueGrain>(queueId.ToString()).QueueMessageBatchAsync(message);
    }

    public Task<IList<GrainsQueueBatchContainer>> GetQueueMessagesAsync(QueueId queueId, int maxCount)
    {
        return client.GetGrain<IQueueGrain>(queueId.ToString()).GetQueueMessagesAsync(maxCount);
    }

    public Task DeleteQueueMessageAsync(QueueId queueId, GrainsQueueBatchContainer message)
    {
        return client.GetGrain<IQueueGrain>(queueId.ToString()).DeleteQueueMessageAsync(message);
    }

    public Task ShutdownAsync(QueueId queueId)
    {
        return client.GetGrain<IQueueGrain>(queueId.ToString()).ShutdownAsync();
    }
}
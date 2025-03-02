using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orleans.Providers.Streams.Common;
using Orleans.Runtime;

namespace Orleans.Streams.Grains;

public class QueueGrain(
    [PersistentState(GrainsStreamProviderConsts.QueueStateName, GrainsStreamProviderConsts.QueueStorageName)]
    IPersistentState<QueueGrainState> persistentState,
    ILogger<QueueGrain> logger,
    IOptions<GrainsQueueOptions> options)
    : Grain, IQueueGrain
{
    public async Task QueueMessageBatchAsync(GrainsQueueBatchContainer message)
    {
        message.RealSequenceToken = new EventSequenceTokenV2(persistentState.State.LastReadMessage++);
        persistentState.State.Messages.Enqueue(message);
        await persistentState.WriteStateAsync();
    }

    public async Task<IList<GrainsQueueBatchContainer>> GetQueueMessagesAsync(int maxCount)
    {
        var messages = new List<GrainsQueueBatchContainer>();
        while (persistentState.State.Messages.Count > 0 && messages.Count < maxCount)
        {
            var message = persistentState.State.Messages.Dequeue();
            persistentState.State.PendingMessages.Enqueue(message);
            messages.Add(message);
        }

        await persistentState.WriteStateAsync();
        return messages;
    }

    public async Task DeleteQueueMessageAsync(GrainsQueueBatchContainer message)
    {
        var pending = persistentState.State.PendingMessages.Dequeue();
        while (!pending.SequenceToken.Equals(pending.SequenceToken))
        {
            switch (options.Value.DeadLetterStrategy)
            {
                case DeadLetterStrategyType.Requeue:
                    persistentState.State.Messages.Enqueue(pending);
                    break;
                case DeadLetterStrategyType.DeadLetterQueue:
                    persistentState.State.DroppedMessages.Enqueue(pending);
                    break;
                case DeadLetterStrategyType.Log:
                default:
                    logger.LogWarning(
                        new QueueMessageDroppedException(pending.ToString()),
                        "Message with token {Token} was not found in the pending messages queue.",
                        pending.SequenceToken);
                    break;
            }

            pending = persistentState.State.PendingMessages.Dequeue();
        }

        await persistentState.WriteStateAsync();
    }

    public async Task ShutdownAsync()
    {
        persistentState.State.LastReadMessage = 0;
        await persistentState.WriteStateAsync();
    }

    public Task<QueueStatus> GetStatusAsync()
    {
        var status = new QueueStatus(persistentState.Etag,
            persistentState.State.LastReadMessage,
            persistentState.State.Messages.Count,
            persistentState.State.PendingMessages.Count,
            persistentState.State.DroppedMessages.Count);

        return Task.FromResult(status);
    }
}
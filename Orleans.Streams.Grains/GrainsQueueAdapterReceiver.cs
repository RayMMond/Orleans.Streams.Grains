using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Orleans.Streams.Grains;

public class GrainsQueueAdapterReceiver : IQueueAdapterReceiver
{
    private readonly QueueId _queueId;
    private readonly IGrainsQueueService _grainsQueueService;
    private Task? _outstandingTask;
    private readonly List<PendingDelivery> _pending = [];
    private readonly ILogger<GrainsQueueAdapterReceiver> _logger;
    private bool _shutdown;

    public GrainsQueueAdapterReceiver(QueueId queueId,
        IGrainsQueueService grainsQueueService,
        ILoggerFactory loggerFactory)
    {
        _queueId = queueId;
        _grainsQueueService = grainsQueueService;
        _logger = loggerFactory.CreateLogger<GrainsQueueAdapterReceiver>();
    }

    public Task Initialize(TimeSpan timeout)
    {
        return Task.CompletedTask;
    }

    public async Task<IList<IBatchContainer>> GetQueueMessagesAsync(int maxCount)
    {
        const int maxNumberOfMessagesToPeek = 32;

        try
        {
            if (_shutdown) return new List<IBatchContainer>();

            var count = maxCount < 0
                ? maxNumberOfMessagesToPeek
                : Math.Min(maxCount, maxNumberOfMessagesToPeek);

            var task = _grainsQueueService.GetQueueMessagesAsync(_queueId, count);
            _outstandingTask = task;
            var messages = await task;

            var azureQueueMessages = new List<IBatchContainer>();
            foreach (var message in messages)
            {
                _pending.Add(new PendingDelivery(message.SequenceToken, message));
            }

            return azureQueueMessages;
        }
        finally
        {
            _outstandingTask = null;
        }
    }

    public async Task Shutdown(TimeSpan timeout)
    {
        try
        {
            if (!_shutdown)
            {
                await _grainsQueueService.ShutdownAsync(_queueId);
            }

            // await the last storage operation, so after we shutdown and stop this receiver we don't get async operation completions from pending storage operations.
            if (_outstandingTask != null)
                await _outstandingTask;
        }
        finally
        {
            // remember that we shut down so we never try to read from the queue again.
            _shutdown = true;
        }
    }


    public async Task MessagesDeliveredAsync(IList<IBatchContainer> messages)
    {
        try
        {
            if (messages.Count == 0 || _shutdown)
            {
                return;
            }

            // get sequence tokens of delivered messages
            var deliveredTokens = messages.Select(message => message.SequenceToken).ToHashSet();

            // find oldest delivered message
            var newest = deliveredTokens.Max()!;

            // finalize all pending messages at or before the newest
            var finalizedDeliveries = _pending
                .Where(pendingDelivery => !pendingDelivery.Token.Newer(newest))
                .ToList();

            if (finalizedDeliveries.Count == 0)
            {
                return;
            }

            // remove all finalized deliveries from pending, regardless of if it was delivered or not.
            _pending.RemoveRange(0, finalizedDeliveries.Count);

            // get the queue messages for all finalized deliveries that were delivered.
            var deliveredCloudQueueMessages = finalizedDeliveries
                .Where(finalized => deliveredTokens.Contains(finalized.Token))
                .Select(finalized => finalized.Message)
                .OrderBy(x => x.SequenceToken)
                .ToList();
            if (deliveredCloudQueueMessages.Count == 0)
            {
                return;
            }

            // delete all delivered queue messages from the queue.  Anything finalized but not delivered will show back up later
            _outstandingTask =
                Task.WhenAll(deliveredCloudQueueMessages.Select(x =>
                    _grainsQueueService.DeleteQueueMessageAsync(_queueId, x)));
            try
            {
                await _outstandingTask;
            }
            catch (Exception e)
            {
                _logger.LogWarning(e,
                    "Exception upon DeleteQueueMessage on queue {QueueName}. Ignoring.", _queueId);
            }
        }
        finally
        {
            _outstandingTask = null;
        }
    }

    private record PendingDelivery(StreamSequenceToken Token, GrainsQueueBatchContainer Message);
}
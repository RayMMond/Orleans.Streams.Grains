using System.Collections.Concurrent;
using System.Collections.Immutable;
using Orleans.Runtime;

namespace Orleans.Streams.Grains;

public class GrainsStreamQueueMapper : IStreamQueueMapper
{
    private readonly GrainsStreamProviderOptions _options;
    private readonly ConcurrentDictionary<StreamId, QueueId> _streamToQueue = new();
    private readonly Dictionary<string, Queue<QueueId>> _streamNamespaceQueues = new();
    private readonly Lock _lock = new();

    public GrainsStreamQueueMapper(GrainsStreamProviderOptions options)
    {
        if (options.MaxStreamNamespaceQueueCount < 1)
        {
            throw new ArgumentException("至少需要1个队列");
        }

        _options = options;
    }

    public IEnumerable<QueueId> GetAllQueues()
    {
        lock (_lock)
        {
            return _streamNamespaceQueues.Values
                .SelectMany(x => x)
                .ToImmutableArray();
        }
    }

    public QueueId GetQueueForStream(StreamId streamId)
    {
        return _streamToQueue.GetOrAdd(streamId, key =>
        {
            lock (_lock)
            {
                var streamNamespace = key.GetNamespace() ?? "(empty)";
                if (!_streamNamespaceQueues.TryGetValue(streamNamespace, out var queueIds))
                {
                    queueIds = new Queue<QueueId>();
                    _streamNamespaceQueues.Add(streamNamespace, queueIds);
                }

                var queueId = queueIds.Count < _options.MaxStreamNamespaceQueueCount
                    ? QueueId.GetQueueId(streamNamespace, (uint)queueIds.Count, 0)
                    : queueIds.Dequeue();
                queueIds.Enqueue(queueId);
                return queueId;
            }
        });
    }
}
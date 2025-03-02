using System.Collections.Concurrent;
using Orleans.Configuration;
using Orleans.Runtime;

namespace Orleans.Streams.Grains;

public class GrainsStreamQueueMapper : IConsistentRingStreamQueueMapper
{
    private const string EmptyNamespace = "(empty)";
    private readonly ConcurrentDictionary<string, HashRingBasedStreamQueueMapper> _ringQueues = [];

    public GrainsStreamQueueMapper(GrainsStreamProviderOptions options)
    {
        if (options.MaxStreamNamespaceQueueCount < 1)
        {
            throw new ArgumentException("至少需要1个队列");
        }

        foreach (var ns in options.Namespaces.Concat([EmptyNamespace]).Distinct())
        {
            _ringQueues.TryAdd(ns, new HashRingBasedStreamQueueMapper(new HashRingStreamQueueMapperOptions
            {
                TotalQueueCount = options.MaxStreamNamespaceQueueCount
            }, ns));
        }
    }

    public IEnumerable<QueueId> GetAllQueues()
    {
        return _ringQueues.Values.SelectMany(x => x.GetAllQueues());
    }

    public QueueId GetQueueForStream(StreamId streamId)
    {
        if (!_ringQueues.TryGetValue(streamId.GetNamespace() ?? EmptyNamespace, out var ringQueue))
        {
            throw new ArgumentException($"未找到命名空间{streamId.GetNamespace()}对应的队列");
        }

        return ringQueue.GetQueueForStream(streamId);
    }

    public IEnumerable<QueueId> GetQueuesForRange(IRingRange range)
    {
        return _ringQueues.Values.SelectMany(x => x.GetQueuesForRange(range));
    }
}
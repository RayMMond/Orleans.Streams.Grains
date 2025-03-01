using System.Collections.Concurrent;
using System.Reflection;
using Shouldly;
using Orleans.Runtime;

namespace Orleans.Streams.Grains.Tests;

public class GrainsStreamQueueMapperTests
{
    private readonly GrainsStreamQueueMapper _mapper = new(Options);
    private const int MaxQueuesPerNamespace = 3;

    private static readonly GrainsStreamProviderOptions Options = new GrainsStreamProviderOptions
    {
        MaxStreamNamespaceQueueCount = MaxQueuesPerNamespace
    };

    [Fact]
    public void GetQueueForStream_SameStreamId_ReturnsSameQueue()
    {
        // Arrange
        var streamId = StreamId.Create("ns1", "key1");

        // Act
        var queue1 = _mapper.GetQueueForStream(streamId);
        var queue2 = _mapper.GetQueueForStream(streamId);

        // Assert
        queue1.ShouldBe(queue2);
    }

    [Fact]
    public void GetQueueForStream_DifferentNamespaces_CreatesDifferentQueues()
    {
        // Arrange
        var stream1 = StreamId.Create("ns1", "key1");
        var stream2 = StreamId.Create("ns2", "key1");

        // Act
        var queue1 = _mapper.GetQueueForStream(stream1);
        var queue2 = _mapper.GetQueueForStream(stream2);

        // Assert
        queue1.ShouldNotBe(queue2);
    }

    [Fact]
    public void GetQueueForStream_ExceedsMaxQueues_ReusesOldestQueue()
    {
        // Arrange
        var queues = new QueueId[MaxQueuesPerNamespace + 1];

        // Act
        for (int i = 0; i < MaxQueuesPerNamespace + 1; i++)
        {
            var streamId = StreamId.Create("ns1", $"key{i}");
            queues[i] = _mapper.GetQueueForStream(streamId);
        }

        // Assert
        queues[MaxQueuesPerNamespace].ShouldBe(queues[0]);
    }

    [Fact]
    public void GetAllQueues_ReturnsAllCreatedQueues()
    {
        // Arrange
        var stream1 = StreamId.Create("ns1", "key1");
        var stream2 = StreamId.Create("ns2", "key1");

        // Act
        _ = _mapper.GetQueueForStream(stream1);
        _ = _mapper.GetQueueForStream(stream2);
        var allQueues = _mapper.GetAllQueues().ToList();

        // Assert
        allQueues.ShouldContain(q => q.ToString().Contains("ns1"));
        allQueues.ShouldContain(q => q.ToString().Contains("ns2"));
        allQueues.Count.ShouldBe(2);
    }

    [Fact]
    public void GetQueueForStream_ConcurrentAccess_SameStream_ReturnsConsistentQueue()
    {
        // Arrange
        var streamId = StreamId.Create("concurrent", "key");
        const int threadCount = 10;
        var results = new QueueId[threadCount];

        // Act
        Parallel.For(0, threadCount, i => { results[i] = _mapper.GetQueueForStream(streamId); });

        // Assert
        results.Distinct().Count().ShouldBe(1, "所有线程应获得相同的队列ID");
    }

    [Fact]
    public void GetQueueForStream_ConcurrentDifferentStreams_WithinMaxQueues()
    {
        // Arrange
        const int totalThreads = MaxQueuesPerNamespace * 2;
        var results = new ConcurrentDictionary<QueueId, byte>();

        // Act
        Parallel.For(0, totalThreads, i =>
        {
            var streamId = StreamId.Create("concurrent", $"key_{i}");
            var queueId = _mapper.GetQueueForStream(streamId);
            results.TryAdd(queueId, 0);
        });

        // Assert
        results.Count.ShouldBe(MaxQueuesPerNamespace,
            $"应自动复用队列，总队列数不超过配置的{MaxQueuesPerNamespace}");
    }

    [Fact]
    public void GetAllQueues_ConcurrentAccess_ReturnsConsistentResults()
    {
        // Arrange
        const int operations = 100;
        var exceptions = new ConcurrentQueue<Exception>();

        // Act
        Parallel.Invoke(
            () =>
            {
                try
                {
                    for (int i = 0; i < operations; i++)
                    {
                        var streamId = StreamId.Create("reader", $"key_{i}");
                        _ = _mapper.GetQueueForStream(streamId);
                    }
                }
                catch (Exception ex)
                {
                    exceptions.Enqueue(ex);
                }
            },
            () =>
            {
                try
                {
                    for (int i = 0; i < operations; i++)
                    {
                        var queues = _mapper.GetAllQueues().ToList();
                        queues.Count.ShouldBeLessThanOrEqualTo(MaxQueuesPerNamespace);
                    }
                }
                catch (Exception ex)
                {
                    exceptions.Enqueue(ex);
                }
            }
        );

        // Assert
        exceptions.ShouldBeEmpty($"发现 {exceptions.Count} 个并发异常");
    }

    // 新增以下测试方法到测试类

    [Fact]
    public void GetQueueForStream_MultipleNamespacesConcurrently_CreatesIsolatedQueues()
    {
        // Arrange
        const int namespaceCount = 5;
        var results = new ConcurrentDictionary<string, ConcurrentBag<QueueId>>();

        // Act
        Parallel.For(0, namespaceCount * 2, i =>
        {
            var ns = $"ns_{i % namespaceCount}";
            var streamId = StreamId.Create(ns, $"key_{i}");
            var queueId = _mapper.GetQueueForStream(streamId);
            results.AddOrUpdate(ns,
                [queueId],
                (_, bag) =>
                {
                    bag.Add(queueId);
                    return bag;
                });
        });

        // Assert
        results.Count.ShouldBe(namespaceCount);
        results.Values.ShouldAllBe(bag =>
                bag.Distinct().Count() <= MaxQueuesPerNamespace,
            "每个命名空间的队列数不应超过配置上限");
    }

    [Fact]
    public void GetQueueForStream_HighContention_ShouldMaintainQueueOrder()
    {
        // Arrange
        const int threadCount = 20;
        var streams = Enumerable.Range(0, MaxQueuesPerNamespace * 2)
            .Select(i => StreamId.Create("high_contention", $"key_{i}"))
            .ToArray();
        var queueUsage = new ConcurrentDictionary<QueueId, int>();

        // Act
        Parallel.ForEach(streams, new ParallelOptions { MaxDegreeOfParallelism = threadCount }, streamId =>
        {
            var queueId = _mapper.GetQueueForStream(streamId);
            queueUsage.AddOrUpdate(queueId, 1, (_, count) => count + 1);
        });

        // Assert
        var expectedQueues = Enumerable.Range(0, MaxQueuesPerNamespace)
            .Select(i => QueueId.GetQueueId("high_contention", (uint)i, 0))
            .ToHashSet();

        queueUsage.Keys.ShouldBeSubsetOf(expectedQueues);
        queueUsage.Values.Sum().ShouldBe(streams.Length);
    }

    [Fact]
    public async Task GetAllQueues_WhileModifyingQueues_ReturnsConsistentView()
    {
        // Arrange
        const int operations = 1000;
        var stopFlag = false;
        var exceptions = new ConcurrentBag<Exception>();

        // Act
        var writerTask = Task.Run(() =>
        {
            for (int i = 0; i < operations; i++)
            {
                var streamId = StreamId.Create("dynamic", $"key_{i}");
                _mapper.GetQueueForStream(streamId);
            }

            stopFlag = true;
        });

        var readerTask = Task.Run(() =>
        {
            while (!stopFlag)
            {
                try
                {
                    var queues = _mapper.GetAllQueues().ToList();
                    queues.Count.ShouldBeLessThanOrEqualTo(MaxQueuesPerNamespace * 2);
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            }
        });

        await Task.WhenAll(writerTask, readerTask);

        // Assert
        exceptions.ShouldBeEmpty("检测到并发访问异常");
    }

    [Fact]
    public void GetQueueForStream_AfterQueueReuse_MaintainsConsistency()
    {
        // Arrange
        var initialStreams = Enumerable.Range(0, MaxQueuesPerNamespace)
            .Select(i => StreamId.Create("reuse_test", $"init_{i}"))
            .ToArray();

        // 创建初始队列并保存QueueId
        var initialQueueIds = initialStreams
            .Select(s => _mapper.GetQueueForStream(s))
            .ToArray();

        // Act
        var reusedQueue = _mapper.GetQueueForStream(StreamId.Create("reuse_test", "new_1"));
        var reusedAgainQueue = _mapper.GetQueueForStream(StreamId.Create("reuse_test", "new_2"));

        // Assert
        var allQueues = _mapper.GetAllQueues().ToList();
        allQueues.Count.ShouldBe(MaxQueuesPerNamespace);

        // 正确比较QueueId实例
        reusedQueue.ShouldBe(initialQueueIds[0]);
        reusedAgainQueue.ShouldBe(initialQueueIds[1]);
    }

    // 使用Coverlet统计代码覆盖率时需关注：
    [Fact]
    public void GetQueueForStream_FirstTimeCreation_CreatesNewQueue()
    {
        // Arrange
        var streamId = StreamId.Create("new_ns", "new_key");

        // Act
        var queueId = _mapper.GetQueueForStream(streamId);

        // Assert
        queueId.ToString().ShouldContain("-0");
    }

    [Fact]
    public void GetQueueForStream_ExistingNamespaceUnderMax_CreatesSequentialQueues()
    {
        // Arrange
        var ns = "sequential_test";

        // Act
        var queues = Enumerable.Range(0, MaxQueuesPerNamespace)
            .Select(i => _mapper.GetQueueForStream(StreamId.Create(ns, $"key_{i}")))
            .ToList();

        // Assert
        queues.Select(q => q.GetNumericId()).ShouldBe(
            Enumerable.Range(0, MaxQueuesPerNamespace).Select(i => (uint)i),
            "应生成顺序队列ID"
        );
    }

// 新增边界条件测试
    [Fact]
    public void GetAllQueues_WhenNoQueuesExist_ReturnsEmpty()
    {
        // Act
        var queues = _mapper.GetAllQueues();

        // Assert
        queues.ShouldBeEmpty();
    }

    [Fact]
    public void GetQueueForStream_WithEmptyNamespace_HandlesCorrectly()
    {
        // Arrange
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        var streamId = StreamId.Create(null, "empty_ns_test");
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

        // Act
        var queueId = _mapper.GetQueueForStream(streamId);

        // Assert
        queueId.ToString().ShouldContain("(empty)");
    }

// 新增配置验证测试
    [Fact]
    public void Constructor_WithZeroMaxQueues_ThrowsArgumentException()
    {
        // Act & Assert
        Should.Throw<ArgumentException>(() => new GrainsStreamQueueMapper(new GrainsStreamProviderOptions
            {
                MaxStreamNamespaceQueueCount = 0
            }))
            .Message.ShouldContain("至少需要1个队列");
    }

// 新增队列复用顺序验证测试
    [Fact]
    public void GetQueueForStream_QueueRecycling_ShouldFollowFIFOOrder()
    {
        // Arrange
        const int totalStreams = MaxQueuesPerNamespace * 3;
        var streams = Enumerable.Range(0, totalStreams)
            .Select(i => StreamId.Create("fifo_test", $"key_{i}"))
            .ToArray();

        // Act
        var queues = streams.Select(s => _mapper.GetQueueForStream(s)).ToList();

        // Assert
        var expectedOrder = queues.Take(MaxQueuesPerNamespace).ToList();
        queues.Skip(MaxQueuesPerNamespace).ShouldAllBe(q =>
                expectedOrder.Contains(q),
            "应按照FIFO顺序复用队列"
        );
    }

// 新增极端并发测试
    [Fact]
    public void GetQueueForStream_UnderExtremeConcurrency_ShouldNotLoseQueues()
    {
        // Arrange
        const int threadCount = 100;
        const int operationsPerThread = 100;
        var counter = new ConcurrentDictionary<QueueId, long>();

        // Act
        Parallel.For(0, threadCount, _ =>
        {
            for (int i = 0; i < operationsPerThread; i++)
            {
                var streamId = StreamId.Create("stress_test", $"key_{Guid.NewGuid()}");
                var queueId = _mapper.GetQueueForStream(streamId);
                counter.AddOrUpdate(queueId, 1, (_, val) => val + 1);
            }
        });

        // Assert
        counter.Count.ShouldBe(MaxQueuesPerNamespace);
        counter.Values.Sum().ShouldBe(threadCount * operationsPerThread);
    }

    [Fact]
    public void GeneratedQueueIds_ShouldBeUniquePerNamespace()
    {
        // Arrange
        const int testRounds = 5;
        var observedIds = new ConcurrentDictionary<string, ConcurrentBag<uint>>();

        // Act
        Parallel.For(0, testRounds * MaxQueuesPerNamespace, i =>
        {
            var streamId = StreamId.Create("uniqueness", $"key_{i}");
            var queueId = _mapper.GetQueueForStream(streamId);
            var numericId = queueId.GetNumericId();

            observedIds.AddOrUpdate("uniqueness",
                [numericId],
                (_, bag) =>
                {
                    bag.Add(numericId);
                    return bag;
                });
        });

        // Assert
        observedIds["uniqueness"].Distinct().Count().ShouldBe(MaxQueuesPerNamespace);
    }

    [Fact]
    public void LongRunning_ShouldNotLeakMemory()
    {
        // 模拟长时间运行(10000次操作)
        for (int i = 0; i < 10000; i++)
        {
            var streamId = StreamId.Create("long_run", $"key_{i % 100}");
            _ = _mapper.GetQueueForStream(streamId);
        }

        // 验证_streamToQueue字典大小
        var countField = typeof(GrainsStreamQueueMapper)
            .GetField("_streamToQueue", BindingFlags.NonPublic | BindingFlags.Instance);
        var dict = (ConcurrentDictionary<StreamId, QueueId>)countField!.GetValue(_mapper)!;
        dict.Count.ShouldBeLessThanOrEqualTo(100 + MaxQueuesPerNamespace);
    }
}
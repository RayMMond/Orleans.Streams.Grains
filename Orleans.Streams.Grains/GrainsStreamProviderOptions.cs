namespace Orleans.Streams.Grains;

public class GrainsStreamProviderOptions
{
    public const int DefaultMaxStreamNamespaceQueueCount = 5;

    public int MaxStreamNamespaceQueueCount { get; set; } = DefaultMaxStreamNamespaceQueueCount;

    public string[] Namespaces { get; set; } = [];

    public Func<QueueId, Task<IStreamFailureHandler>>? StreamFailureHandlerFactory { get; set; }
}
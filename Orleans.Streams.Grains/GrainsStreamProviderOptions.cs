namespace Orleans.Streams.Grains;

public class GrainsStreamProviderOptions
{
    public const int DefaultMaxStreamNamespaceQueueCount = 5;

    public int MaxStreamNamespaceQueueCount { get; set; } = DefaultMaxStreamNamespaceQueueCount;
}
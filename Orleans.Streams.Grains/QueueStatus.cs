namespace Orleans.Streams.Grains;

[Serializable]
[GenerateSerializer]
public class QueueStatus
{
    public QueueStatus(string? eTag,
        long lastReadMessage,
        int messageCount,
        int pendingMessagesCount,
        long droppedMessagesCount)
    {
        ETag = eTag;
        LastReadMessage = lastReadMessage;
        MessageCount = messageCount;
        PendingMessagesCount = pendingMessagesCount;
        DroppedMessagesCount = droppedMessagesCount;
    }

    [Id(0)]
    public string? ETag { get; set; }

    [Id(1)]
    public long LastReadMessage { get; set; }

    [Id(2)]
    public int MessageCount { get; set; }

    [Id(3)]
    public int PendingMessagesCount { get; set; }

    [Id(4)]
    public long DroppedMessagesCount { get; set; }

    public override string ToString()
    {
        return
            $"ETag:{ETag},LastReadMessage:{LastReadMessage},MessageCount:{MessageCount},PendingMessagesCount:{PendingMessagesCount},DroppedMessagesCount:{DroppedMessagesCount}";
    }
}
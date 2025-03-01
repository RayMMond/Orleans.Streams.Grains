namespace Orleans.Streams.Grains;

public class QueueMessageDroppedException : Exception
{
    public QueueMessageDroppedException()
    {
    }

    public QueueMessageDroppedException(string message) : base(message)
    {
    }

    public QueueMessageDroppedException(string message, Exception inner) : base(message, inner)
    {
    }
}
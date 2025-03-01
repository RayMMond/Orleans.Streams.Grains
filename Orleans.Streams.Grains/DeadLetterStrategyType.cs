namespace Orleans.Streams.Grains;

public enum DeadLetterStrategyType
{
    Log = 0,
    Requeue = 1,
    DeadLetterQueue = 2
}
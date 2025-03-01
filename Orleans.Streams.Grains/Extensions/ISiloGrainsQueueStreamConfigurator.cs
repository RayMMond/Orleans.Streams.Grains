using Orleans.Hosting;

namespace Orleans.Streams.Grains.Extensions;

public interface ISiloGrainsQueueStreamConfigurator : IGrainsQueueStreamConfigurator,
    ISiloPersistentStreamConfigurator
{
}
using Orleans.Hosting;

namespace Orleans.Streams.Grains.Extensions;

public interface IClusterClientGrainsQueueStreamConfigurator : IGrainsQueueStreamConfigurator,
    IClusterClientPersistentStreamConfigurator
{
}
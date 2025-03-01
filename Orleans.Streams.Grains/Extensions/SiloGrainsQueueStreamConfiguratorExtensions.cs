using Orleans.Configuration;
using Orleans.Hosting;

namespace Orleans.Streams.Grains.Extensions;

public static class SiloGrainsQueueStreamConfiguratorExtensions
{
    public static void ConfigureCacheSize(this ISiloGrainsQueueStreamConfigurator configurator,
        int cacheSize = SimpleQueueCacheOptions.DEFAULT_CACHE_SIZE)
    {
        configurator.Configure<SimpleQueueCacheOptions>(
            ob => ob.Configure(options => options.CacheSize = cacheSize));
    }
}
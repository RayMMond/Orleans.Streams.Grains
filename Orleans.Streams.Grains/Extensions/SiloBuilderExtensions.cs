using Microsoft.Extensions.Options;
using Orleans.Configuration;
using Orleans.Hosting;

namespace Orleans.Streams.Grains.Extensions;

public static class SiloBuilderExtensions
{
    /// <summary>
    /// Configure silo to use Grains queue persistent streams.
    /// </summary>
    public static ISiloBuilder AddGrainsStreams(this ISiloBuilder builder, string name,
        Action<SiloGrainsQueueStreamConfigurator> configure)
    {
        var configurator = new SiloGrainsQueueStreamConfigurator(name,
            configureServicesDelegate => builder.ConfigureServices(configureServicesDelegate));
        configure.Invoke(configurator);
        return builder;
    }

    /// <summary>
    /// Configure silo to use Grains queue persistent streams with default settings
    /// </summary>
    public static ISiloBuilder AddGrainsStreams(this ISiloBuilder builder, string name,
        Action<OptionsBuilder<GrainsStreamProviderOptions>>? configureOptions = null,
        TimeSpan? siloMaturityPeriod = null,
        int cacheSize = SimpleQueueCacheOptions.DEFAULT_CACHE_SIZE)
    {
        builder.AddGrainsStreams(name, configure =>
        {
            if (configureOptions != null) configure.ConfigureGrainsQueue(configureOptions);
            configure.UseConsistentRingQueueBalancer();
            configure.ConfigureCacheSize(cacheSize);
        });
        return builder;
    }
}
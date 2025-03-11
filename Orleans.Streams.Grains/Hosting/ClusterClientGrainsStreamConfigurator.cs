using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.Configuration;
using Orleans.Hosting;

namespace Orleans.Streams.Grains.Hosting;

/// <summary>
/// Helps set up an individual stream provider on a silo.
/// </summary>
public class ClusterClientGrainsStreamConfigurator : ClusterClientPersistentStreamConfigurator
{
    public ClusterClientGrainsStreamConfigurator(string name, IClientBuilder clientBuilder) : base(name, clientBuilder,
        GrainsQueueAdapterFactory.Create)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(clientBuilder);

        clientBuilder.ConfigureServices(services =>
        {
            services
                .ConfigureNamedOptionForLogging<GrainsStreamOptions>(name)
                .ConfigureNamedOptionForLogging<SimpleQueueCacheOptions>(name)
                .AddTransient<IConfigurationValidator>(sp =>
                    new GrainsStreamOptionsValidator(sp.GetOptionsByName<GrainsStreamOptions>(name),
                        name));
        });
    }

    public ClusterClientGrainsStreamConfigurator ConfigureGrains(
        Action<OptionsBuilder<GrainsStreamOptions>> configureOptions)
    {
        ArgumentNullException.ThrowIfNull(configureOptions);

        this.Configure(configureOptions);

        return this;
    }

    public ClusterClientGrainsStreamConfigurator ConfigureCache(
        int cacheSize = SimpleQueueCacheOptions.DEFAULT_CACHE_SIZE)
    {
        this.Configure<SimpleQueueCacheOptions>(ob => ob.Configure(options => options.CacheSize = cacheSize));

        return this;
    }
}
using Microsoft.Extensions.DependencyInjection;
using Orleans.Configuration;
using Orleans.Hosting;

namespace Orleans.Streams.Grains.Extensions;

public class SiloGrainsQueueStreamConfigurator : SiloPersistentStreamConfigurator,
    ISiloGrainsQueueStreamConfigurator
{
    public SiloGrainsQueueStreamConfigurator(string name,
        Action<Action<IServiceCollection>> configureServicesDelegate)
        : base(name, configureServicesDelegate, GrainsQueueAdapterFactory.Create)
    {
        this.ConfigureComponent(GrainsStreamProviderOptionsValidator.Create);
        this.ConfigureComponent(SimpleQueueCacheOptionsValidator.Create);
        ConfigureDelegate(services => services.AddGrainsQueue());
        // ConfigureDelegate(services =>
        //     services.TryAddSingleton<IQueueDataAdapter<string, IBatchContainer>, GrainsQueueDataAdapter>());
    }
}
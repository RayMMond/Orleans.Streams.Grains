using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.Hosting;

namespace Orleans.Streams.Grains.Extensions;

public static class GrainsQueueStreamConfiguratorExtensions
{
    public static void ConfigureGrainsQueue(this IGrainsQueueStreamConfigurator configurator,
        Action<OptionsBuilder<GrainsStreamProviderOptions>> configureOptions)
    {
        configurator.Configure(configureOptions);
    }

    public static void ConfigureQueueDataAdapter(this IGrainsQueueStreamConfigurator configurator,
        Func<IServiceProvider, string, IQueueDataAdapter<string, IBatchContainer>> factory)
    {
        configurator.ConfigureComponent(factory);
    }

    public static void ConfigureQueueDataAdapter<TQueueDataAdapter>(
        this IGrainsQueueStreamConfigurator configurator)
        where TQueueDataAdapter : IQueueDataAdapter<string, IBatchContainer>
    {
        configurator.ConfigureComponent<IQueueDataAdapter<string, IBatchContainer>>((sp, n) =>
            ActivatorUtilities.CreateInstance<TQueueDataAdapter>(sp));
    }
}
using Microsoft.Extensions.Options;
using Orleans.Hosting;

namespace Orleans.Streams.Grains.Extensions;

public static class SiloBuilderExtensions
{
    /// <summary>
    /// Configure silo to use Grains queue persistent streams.
    /// </summary>
    public static ISiloBuilder AddGrainsQueueStreams(this ISiloBuilder builder, string name,
        Action<SiloGrainsQueueStreamConfigurator> configure)
    {
        var configurator = new SiloGrainsQueueStreamConfigurator(name,
            configureServicesDelegate => builder.ConfigureServices(configureServicesDelegate));
        configure?.Invoke(configurator);
        return builder;
    }

    /// <summary>
    /// Configure silo to use Grains queue persistent streams with default settings
    /// </summary>
    public static ISiloBuilder AddGrainsQueueStreams(this ISiloBuilder builder, string name,
        Action<OptionsBuilder<GrainsStreamProviderOptions>> configureOptions)
    {
        builder.AddGrainsQueueStreams(name, b =>
            b.ConfigureGrainsQueue(configureOptions));
        return builder;
    }
}
using Microsoft.Extensions.Options;
using Orleans.Hosting;

namespace Orleans.Streams.Grains.Extensions;

public static class ClientBuilderExtensions
{
    /// <summary>
    /// Configure cluster client to use Grains queue persistent streams.
    /// </summary>
    public static IClientBuilder AddGrainsStreams(this IClientBuilder builder,
        string name,
        Action<ClusterClientGrainsQueueStreamConfigurator>? configure = null)
    {
        //the constructor wires up DI with GrainsQueueStream, so has to be called regardless configure is null or not
        var configurator = new ClusterClientGrainsQueueStreamConfigurator(name, builder);
        configure?.Invoke(configurator);
        return builder;
    }

    /// <summary>
    /// Configure cluster client to use Grains queue persistent streams.
    /// </summary>
    public static IClientBuilder AddGrainsStreams(this IClientBuilder builder,
        string name, Action<OptionsBuilder<GrainsStreamProviderOptions>> configureOptions)
    {
        builder.AddGrainsStreams(name, b =>
            b.ConfigureGrainsQueue(configureOptions));
        return builder;
    }
}
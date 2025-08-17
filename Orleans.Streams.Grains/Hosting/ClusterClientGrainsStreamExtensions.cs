using Orleans.Hosting;

namespace Orleans.Streams.Grains.Hosting;

/// <summary>
/// Allows configuration of individual Grains streams in a cluster client.
/// </summary>
public static class ClusterClientGrainsStreamExtensions
{
    /// <summary>
    /// Configure cluster client to use Grains persistent streams with default settings.
    /// </summary>
    public static IClientBuilder AddGrainsStreams(this IClientBuilder builder, string? name,
        Action<GrainsStreamOptions> configureOptions)
    {
        ArgumentNullException.ThrowIfNull(name);

        return builder.AddGrainsStreams(name, b => { b.ConfigureGrains(ob => ob.Configure(configureOptions)); });
    }

    /// <summary>
    /// Configure cluster client to use Grains persistent streams.
    /// </summary>
    public static IClientBuilder AddGrainsStreams(this IClientBuilder builder, string? name,
        Action<ClusterClientGrainsStreamConfigurator> configure)
    {
        ArgumentNullException.ThrowIfNull(name);

        var configurator = new ClusterClientGrainsStreamConfigurator(name, builder);

        configure.Invoke(configurator);

        return builder;
    }
}
using Orleans.Hosting;

namespace Orleans.Streams.Grains.Hosting;

/// <summary>
/// Allows configuration of individual Grains streams in a silo.
/// </summary>
public static class SiloBuilderGrainsStreamExtensions
{
    /// <summary>
    /// Configure silo to use Grains persistent streams.
    /// </summary>
    public static ISiloBuilder AddGrainsStreams(this ISiloBuilder builder, string name,
        Action<GrainsStreamOptions> configureOptions)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(configureOptions);

        return builder.AddGrainsStreams(name, b => { b.ConfigureGrains(ob => ob.Configure(configureOptions)); });
    }

    /// <summary>
    /// Configure silo to use Grains persistent streams.
    /// </summary>
    public static ISiloBuilder AddGrainsStreams(this ISiloBuilder builder, string name,
        Action<SiloGrainsStreamConfigurator> configure)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(configure);

        var configurator = new SiloGrainsStreamConfigurator(name,
            configureServicesDelegate => builder.ConfigureServices(configureServicesDelegate));

        configure.Invoke(configurator);

        return builder;
    }
}
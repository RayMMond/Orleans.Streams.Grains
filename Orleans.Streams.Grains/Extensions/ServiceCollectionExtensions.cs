using Microsoft.Extensions.DependencyInjection;

namespace Orleans.Streams.Grains.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGrainsQueue(this IServiceCollection services,
        Action<GrainsQueueOptions>? configureOptions = null)
    {
        ArgumentNullException.ThrowIfNull((object)services, nameof(services));
        services.Configure(configureOptions ?? (Action<GrainsQueueOptions>)(_ => { }));
        return services;
    }
}
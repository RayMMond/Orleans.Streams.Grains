using Orleans.Hosting;

namespace Orleans.Streams.Grains.Extensions
{
    public class ClusterClientGrainsQueueStreamConfigurator : ClusterClientPersistentStreamConfigurator,
        IClusterClientGrainsQueueStreamConfigurator
    {
        public ClusterClientGrainsQueueStreamConfigurator(string name, IClientBuilder builder)
            : base(name, builder, GrainsQueueAdapterFactory.Create)
        {
            this.ConfigureComponent(GrainsStreamProviderOptionsValidator.Create);
            ConfigureDelegate(services => services.AddGrainsQueue());
            // ConfigureDelegate(services =>
            //     services.TryAddSingleton<IQueueDataAdapter<string, IBatchContainer>, GrainsQueueDataAdapter>());
        }
    }
}
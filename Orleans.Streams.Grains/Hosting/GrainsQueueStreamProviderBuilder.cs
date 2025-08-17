using Microsoft.Extensions.Configuration;
using Orleans;
using Orleans.Hosting;
using Orleans.Providers;
using Orleans.Streams.Grains;
using Orleans.Streams.Grains.Hosting;

[assembly:
    RegisterProvider(GrainsStreamProviderConsts.ProviderType, "Streaming", "Silo",
        typeof(GrainsQueueStreamProviderBuilder))]
[assembly:
    RegisterProvider(GrainsStreamProviderConsts.ProviderType, "Streaming", "Client",
        typeof(GrainsQueueStreamProviderBuilder))]

// string json = """
// {
// 	"Orleans": {
// 		"Streaming": {
// 			"{{StreamProviderName}}": {
// 				"ProviderType": "GrainsQueueStorage",
// 				"MaxStreamNamespaceQueueCount": "10",
//              "NamespaceQueue":[{"Namespace":"ns1","QueueCount":5},{"Namespace":"ns2","QueueCount":5}],
//              "CacheSize": "5"
// 			}
// 		}
// 	}
// }
// """;

namespace Orleans.Streams.Grains.Hosting;

public sealed class GrainsQueueStreamProviderBuilder : IProviderBuilder<ISiloBuilder>, IProviderBuilder<IClientBuilder>
{
    public void Configure(ISiloBuilder builder, string? name, IConfigurationSection configurationSection)
    {
        builder.AddGrainsStreams(name, ConfigureSilo(configurationSection));
    }

    public void Configure(IClientBuilder builder, string? name, IConfigurationSection configurationSection)
    {
        builder.AddGrainsStreams(name, ConfigureClient(configurationSection));
    }

    private static Action<ClusterClientGrainsStreamConfigurator> ConfigureClient(
        IConfigurationSection configurationSection)
    {
        return configurator =>
        {
            configurator.ConfigureGrains(optionsBuilder => { optionsBuilder.Configure(configurationSection.Bind); });

            if (int.TryParse(configurationSection["CacheSize"], out var cacheSize))
            {
                configurator.ConfigureCache(cacheSize);
            }
        };
    }

    private static Action<SiloGrainsStreamConfigurator> ConfigureSilo(
        IConfigurationSection configurationSection)
    {
        return configurator =>
        {
            configurator.ConfigureGrains(optionsBuilder => { optionsBuilder.Configure(configurationSection.Bind); });

            if (int.TryParse(configurationSection["CacheSize"], out var cacheSize))
            {
                configurator.ConfigureCache(cacheSize);
            }

            configurator.UseConsistentRingQueueBalancer();
        };
    }
}
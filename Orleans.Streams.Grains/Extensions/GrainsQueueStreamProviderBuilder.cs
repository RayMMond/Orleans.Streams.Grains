using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Orleans;
using Orleans.Hosting;
using Orleans.Providers;
using Orleans.Streams.Grains;
using Orleans.Streams.Grains.Extensions;

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
// 				"MaxStreamNamespaceQueueCount": "10"
// 			}
// 		}
// 	}
// }
// """;

namespace Orleans.Streams.Grains.Extensions;

public sealed class GrainsQueueStreamProviderBuilder : IProviderBuilder<ISiloBuilder>, IProviderBuilder<IClientBuilder>
{
    public void Configure(ISiloBuilder builder, string name, IConfigurationSection configurationSection)
    {
        builder.AddGrainsStreams(name, GetQueueOptionBuilder(configurationSection));
    }

    public void Configure(IClientBuilder builder, string name, IConfigurationSection configurationSection)
    {
        builder.AddGrainsStreams(name, GetQueueOptionBuilder(configurationSection));
    }

    private static Action<OptionsBuilder<GrainsStreamProviderOptions>> GetQueueOptionBuilder(
        IConfigurationSection configurationSection)
    {
        return optionsBuilder =>
        {
            optionsBuilder.Configure<GrainsStreamProviderOptions>((options, _) =>
            {
                options.MaxStreamNamespaceQueueCount =
                    int.TryParse(configurationSection["MaxStreamNamespaceQueueCount"], out var c)
                        ? c
                        : GrainsStreamProviderOptions.DefaultMaxStreamNamespaceQueueCount;
            });
        };
    }
}
using Orleans.Runtime;

namespace Orleans.Streams.Grains;

public class GrainsStreamProviderOptionsValidator : IConfigurationValidator
{
    private readonly GrainsStreamProviderOptions _options;
    private readonly string _name;

    private GrainsStreamProviderOptionsValidator(GrainsStreamProviderOptions options, string name)
    {
        _options = options;
        _name = name;
    }

    public void ValidateConfiguration()
    {
        if (_options.MaxStreamNamespaceQueueCount < 1)
        {
            throw new OrleansConfigurationException(
                $"{nameof(GrainsStreamProviderOptions)}.{nameof(GrainsStreamProviderOptions.MaxStreamNamespaceQueueCount)} on stream provider {_name} is invalid. {nameof(GrainsStreamProviderOptions.MaxStreamNamespaceQueueCount)} must be greater than 0");
        }
    }

    public static IConfigurationValidator Create(IServiceProvider services, string name)
    {
        GrainsStreamProviderOptions aqOptions = services.GetOptionsByName<GrainsStreamProviderOptions>(name);
        return new GrainsStreamProviderOptionsValidator(aqOptions, name);
    }
}
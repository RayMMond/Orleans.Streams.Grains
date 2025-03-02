using Microsoft.CodeAnalysis;
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

        var duplicates = _options.NamespaceQueue
            .GroupBy(x => x.Namespace)
            .Where(x => x.Count() > 1)
            .ToList();

        if (duplicates.Count > 0)
        {
            var duplicateNamespaces = string.Join(", ", duplicates.Select(x => x.Key));
            throw new OrleansConfigurationException(
                $"{nameof(GrainsStreamProviderOptions)}.{nameof(GrainsStreamProviderOptions.NamespaceQueue)} on stream provider {_name} contains duplicate namespaces: {duplicateNamespaces}");
        }

        foreach (var namespaceQueue in _options.NamespaceQueue)
        {
            if (namespaceQueue.QueueCount < 1)
            {
                throw new OrleansConfigurationException(
                    $"{nameof(GrainsStreamProviderOptions)}.{nameof(GrainsStreamProviderOptions.NamespaceQueue)}.{nameof(GrainsStreamProviderOptions.GrainsStreamProviderNamespaceQueueOptions.QueueCount)} on stream provider {_name} is invalid. {nameof(GrainsStreamProviderOptions.GrainsStreamProviderNamespaceQueueOptions.QueueCount)} must be greater than 0");
            }
        }
    }

    public static IConfigurationValidator Create(IServiceProvider services, string name)
    {
        GrainsStreamProviderOptions aqOptions = services.GetOptionsByName<GrainsStreamProviderOptions>(name);
        return new GrainsStreamProviderOptionsValidator(aqOptions, name);
    }
}
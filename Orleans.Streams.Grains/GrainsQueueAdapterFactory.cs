using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orleans.Configuration;
using Orleans.Providers.Streams.Common;

namespace Orleans.Streams.Grains;

public class GrainsQueueAdapterFactory : IQueueAdapterFactory
{
    private readonly string _providerName;
    private readonly IGrainsQueueService _grainsQueueService;
    private readonly ILoggerFactory _loggerFactory;
    private readonly SimpleQueueAdapterCache _adapterCache;
    private readonly GrainsStreamQueueMapper _grainStreamQueueMapper;

    /// <summary>
    /// Application level failure handler override.
    /// </summary>
    protected Func<QueueId, Task<IStreamFailureHandler>>? StreamFailureHandlerFactory { private get; set; }

    public GrainsQueueAdapterFactory(
        string name,
        GrainsStreamProviderOptions options,
        SimpleQueueCacheOptions cacheOptions,
        IGrainsQueueService grainsQueueService,
        ILoggerFactory loggerFactory)
    {
        _providerName = name;
        _grainsQueueService = grainsQueueService;
        _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        _adapterCache = new SimpleQueueAdapterCache(cacheOptions, _providerName, _loggerFactory);
        _grainStreamQueueMapper = new GrainsStreamQueueMapper(options);
    }

    /// <summary> Init the factory.</summary>
    public virtual void Init()
    {
        StreamFailureHandlerFactory ??= _ => Task.FromResult<IStreamFailureHandler>(
            new NoOpStreamDeliveryFailureHandler());
    }

    /// <summary>Creates the Grains Queue based adapter.</summary>
    public virtual Task<IQueueAdapter> CreateAdapter()
    {
        var adapter = new GrainsQueueAdapter(
            _grainStreamQueueMapper,
            _grainsQueueService,
            _loggerFactory,
            _providerName);
        return Task.FromResult<IQueueAdapter>(adapter);
    }

    /// <summary>Creates the adapter cache.</summary>
    public virtual IQueueAdapterCache GetQueueAdapterCache()
    {
        return _adapterCache;
    }

    /// <summary>Creates the factory stream queue mapper.</summary>
    public IStreamQueueMapper GetStreamQueueMapper()
    {
        return _grainStreamQueueMapper;
    }

    /// <summary>
    /// Creates a delivery failure handler for the specified queue.
    /// </summary>
    /// <param name="queueId"></param>
    /// <returns></returns>
    public Task<IStreamFailureHandler> GetDeliveryFailureHandler(QueueId queueId)
    {
        return StreamFailureHandlerFactory!(queueId);
    }

    public static GrainsQueueAdapterFactory Create(IServiceProvider services, string name)
    {
        var options = services.GetOptionsByName<GrainsStreamProviderOptions>(name);
        var cacheOptions = services.GetOptionsByName<SimpleQueueCacheOptions>(name);
        // var dataAdapter = services.GetKeyedService<IQueueDataAdapter<string, IBatchContainer>>(name)
        //                   ?? services.GetService<IQueueDataAdapter<string, IBatchContainer>>();
        var factory =
            ActivatorUtilities.CreateInstance<GrainsQueueAdapterFactory>(services, name, options,
                cacheOptions);
        factory.Init();
        return factory;
    }
}
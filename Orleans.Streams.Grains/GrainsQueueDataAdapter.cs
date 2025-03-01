// using Microsoft.Extensions.DependencyInjection;
// using Orleans.Providers.Streams.Common;
// using Orleans.Runtime;
// using Orleans.Serialization;
//
// namespace Orleans.Streams.Grains;
//
// /// <summary>
// /// Data adapter that uses types that support custom serializers (like json).
// /// </summary>
// [SerializationCallbacks(typeof(OnDeserializedCallbacks))]
// public class GrainsQueueDataAdapter : IQueueDataAdapter<string, IBatchContainer>, IOnDeserialized
// {
//     private Serializer<GrainsQueueBatchContainer> _serializer;
//
//     /// <summary>
//     /// Initializes a new instance of the <see cref="GrainsQueueDataAdapter"/> class.
//     /// </summary>
//     /// <param name="serializer"></param>
//     public GrainsQueueDataAdapter(Serializer serializer)
//     {
//         _serializer = serializer.GetSerializer<GrainsQueueBatchContainer>();
//     }
//
//     /// <summary>
//     /// Creates a cloud queue message from stream event data.
//     /// </summary>
//     public string ToQueueMessage<T>(StreamId streamId, IEnumerable<T> events, StreamSequenceToken token,
//         Dictionary<string, object> requestContext)
//     {
//         var azureQueueBatchMessage =
//             new GrainsQueueBatchContainer(streamId, events.Cast<object>().ToList(), requestContext);
//         var rawBytes = _serializer.SerializeToArray(azureQueueBatchMessage);
//         return Convert.ToBase64String(rawBytes);
//     }
//
//     /// <summary>
//     /// Creates a batch container from a cloud queue message
//     /// </summary>
//     public IBatchContainer FromQueueMessage(string cloudMsg, long sequenceId)
//     {
//         var azureQueueBatch = _serializer.Deserialize(Convert.FromBase64String(cloudMsg));
//         azureQueueBatch.RealSequenceToken = new EventSequenceTokenV2(sequenceId);
//         return azureQueueBatch;
//     }
//
//     void IOnDeserialized.OnDeserialized(DeserializationContext context)
//     {
//         _serializer = context.ServiceProvider.GetRequiredService<Serializer<GrainsQueueBatchContainer>>();
//     }
// }
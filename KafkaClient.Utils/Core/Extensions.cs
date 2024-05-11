using Confluent.Kafka;

namespace KafkaClient.Utils.Core;
internal static class Extensions
{
    public static Headers ToHeaders(this IReadOnlyDictionary<string, string> dict)
    {
        var headers = new Headers();

        foreach(var (key, val) in dict)
        {
            headers.Add(key, Serializers.Utf8.Serialize(val, SerializationContext.Empty));
        }

        return headers;
    }
}

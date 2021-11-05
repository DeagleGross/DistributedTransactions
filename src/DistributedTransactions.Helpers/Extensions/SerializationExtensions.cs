using System.Text.Json;

namespace DistributedTransactions.Helpers.Extensions
{
    public static class SerializationExtensions
    {
        public static string Serialize<T>(this T instance) => JsonSerializer.Serialize(instance);
        
        public static T Deserialize<T>(this string json) => JsonSerializer.Deserialize<T>(json);
    }
}

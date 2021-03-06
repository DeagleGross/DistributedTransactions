using System;
using System.Text.Json;

namespace DistributedTransactions.Helpers.Extensions
{
    public static class SerializationExtensions
    {
        public static string Serialize<T>(this T instance) => JsonSerializer.Serialize(instance);

        public static string Serialize(this object instance, Type type) => JsonSerializer.Serialize(instance, type);
        

        public static T Deserialize<T>(this string json) => JsonSerializer.Deserialize<T>(json);

        public static object Deserialize(this string json, Type type) => JsonSerializer.Deserialize(json, type);
    }
}

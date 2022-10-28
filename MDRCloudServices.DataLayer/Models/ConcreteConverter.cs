using Newtonsoft.Json;

namespace MDRCloudServices.DataLayer.Models;

/// <summary>
/// Used by JSON serialisation to convert from an interface to a concrete class
/// </summary>
/// <typeparam name="I"></typeparam>
/// <typeparam name="T"></typeparam>
public class ConcreteConverter<I, T> : JsonConverter
{
    public override bool CanConvert(Type objectType) => objectType == typeof(I);

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        return serializer.Deserialize<T>(reader);
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        serializer.Serialize(writer, value);
    }
}

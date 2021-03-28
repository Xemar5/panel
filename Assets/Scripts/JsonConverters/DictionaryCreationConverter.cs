using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class DictionaryCreationConverter : CustomCreationConverter<IDictionary<string, object>>
{
    private static JsonConverter[] converters = null;
    public static JsonConverter[] Converters
    {
        get
        {
            if (converters == null)
            {
                converters = new JsonConverter[] 
                {
                    new Vector3Converter(),
                    new Vector2Converter(),
                    new Vector4Converter(),
                    new ColorConverter(),
                    new QuaternionConverter(),
                    new DictionaryCreationConverter(),
                };
            }
            return converters;
        }
    }


    public static IDictionary<string, object> Deserialize(string json)
    {
        return JsonConvert.DeserializeObject<IDictionary<string, object>>(json, Converters);
    }
    public static string Serialize(IDictionary<string, object> obj)
    {
        return JsonConvert.SerializeObject(obj, Converters);
    }

    public override IDictionary<string, object> Create(Type objectType)
    {
        return new Dictionary<string, object>();
    }

    public override bool CanConvert(Type objectType)
    {
        // in addition to handling IDictionary<string, object>
        // we want to handle the deserialization of dict value
        // which is of type object
        return objectType == typeof(object) || base.CanConvert(objectType);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.StartObject
            || reader.TokenType == JsonToken.Null)
            return base.ReadJson(reader, objectType, existingValue, serializer);

        // if the next token is not an object
        // then fall back on standard deserializer (strings, numbers etc.)
        return serializer.Deserialize(reader);
    }
}

public static class DictionaryCreationConverterExtensions
{
    public static Dictionary<string, object> At(this Dictionary<string, object> dict, string key)
    {
        return (Dictionary<string, object>)dict[key];
    }
    public static T Get<T>(this Dictionary<string, object> dict, string key)
    {
        return (T)dict[key];
    }
}
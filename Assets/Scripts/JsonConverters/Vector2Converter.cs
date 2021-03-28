using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class Vector2Converter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        if (objectType == typeof(Vector2))
        {
            return true;
        }
        return false;
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        var t = serializer.Deserialize(reader);
        var iv = JsonConvert.DeserializeObject<Vector2>(t.ToString());
        return iv;
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        Vector2 v = (Vector2)value;

        writer.WriteStartObject();
        writer.WritePropertyName("x");
        writer.WriteValue(v.x);
        writer.WritePropertyName("y");
        writer.WriteValue(v.y);
        writer.WriteEndObject();
    }
}
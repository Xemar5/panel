﻿using Newtonsoft.Json;
using System;
using UnityEngine;

public class QuaternionConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        if (objectType == typeof(Quaternion))
        {
            return true;
        }
        return false;
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        var t = serializer.Deserialize(reader);
        var iv = JsonConvert.DeserializeObject<Quaternion>(t.ToString());
        return iv;
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        Quaternion v = (Quaternion)value;

        writer.WriteStartObject();
        writer.WritePropertyName("x");
        writer.WriteValue(v.x);
        writer.WritePropertyName("y");
        writer.WriteValue(v.y);
        writer.WritePropertyName("z");
        writer.WriteValue(v.z);
        writer.WritePropertyName("w");
        writer.WriteValue(v.w);
        writer.WriteEndObject();
    }
}

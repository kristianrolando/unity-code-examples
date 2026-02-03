//
// UnityConverters.cs
// Purpose:
//   A collection of custom Newtonsoft.Json converters for common Unity types
//   (Vector2/3/4, Quaternion, Color, Rect, Bounds, Matrix4x4, LayerMask).
//   These types are not serialized by Newtonsoft.Json out-of-the-box in a
//   Unity-friendly way, so this file defines explicit JSON shapes for them.
//
// When to use:
//   - You are building a save/load system using Newtonsoft.Json
//   - You want stable, human-readable JSON for Unity structs
//   - You want to avoid self-referencing loop errors on Unity types
//
// How to plug in:
//   var settings = new JsonSerializerSettings {
//       Converters = {
//           new Vector3Converter(),
//           new QuaternionConverter(),
//           ...
//       }
//   };
//
// Notes:
//   - All converters write a minimal JSON shape (x/y/z/etc) for clarity.
//   - All converters are symmetric: what they write is exactly what they read.
//   - Keep this file small and focused on Unity value types only.
//   - If you add new converters here, remember to register them in your SaveManager.
//
// --------------------------------------------------------------
// Simple example:
//
//   var settings = new JsonSerializerSettings();
//   settings.Converters.Add(new Vector3Converter());
//
//   var v = new Vector3(1, 2, 3);
//   string json = JsonConvert.SerializeObject(v, settings);
//   // json → { "x":1.0, "y":2.0, "z":3.0 }
//
//   var back = JsonConvert.DeserializeObject<Vector3>(json, settings);
//
// --------------------------------------------------------------
//

using System;
using Newtonsoft.Json;
using UnityEngine;

namespace Game.Systems.SaveSystem
{
    /// <summary>
    /// JSON converter for UnityEngine.Vector2.
    /// Serialized shape: { "x": ..., "y": ... }
    /// </summary>
    public class Vector2Converter : JsonConverter<Vector2>
    {
        public override void WriteJson(JsonWriter writer, Vector2 value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("x"); writer.WriteValue(value.x);
            writer.WritePropertyName("y"); writer.WriteValue(value.y);
            writer.WriteEndObject();
        }

        public override Vector2 ReadJson(JsonReader reader, Type objectType, Vector2 existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            float x = 0, y = 0;
            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.EndObject)
                    break;
                if (reader.TokenType == JsonToken.PropertyName)
                {
                    var prop = (string)reader.Value;
                    reader.Read();
                    switch (prop)
                    {
                        case "x": x = Convert.ToSingle(reader.Value); break;
                        case "y": y = Convert.ToSingle(reader.Value); break;
                    }
                }
            }
            return new Vector2(x, y);
        }
    }

    /// <summary>
    /// JSON converter for UnityEngine.Vector3.
    /// Serialized shape: { "x": ..., "y": ..., "z": ... }
    /// </summary>
    public class Vector3Converter : JsonConverter<Vector3>
    {
        public override void WriteJson(JsonWriter writer, Vector3 value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("x"); writer.WriteValue(value.x);
            writer.WritePropertyName("y"); writer.WriteValue(value.y);
            writer.WritePropertyName("z"); writer.WriteValue(value.z);
            writer.WriteEndObject();
        }

        public override Vector3 ReadJson(JsonReader reader, Type objectType, Vector3 existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            float x = 0, y = 0, z = 0;
            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.EndObject)
                    break;

                if (reader.TokenType == JsonToken.PropertyName)
                {
                    var prop = (string)reader.Value;
                    reader.Read();
                    switch (prop)
                    {
                        case "x": x = Convert.ToSingle(reader.Value); break;
                        case "y": y = Convert.ToSingle(reader.Value); break;
                        case "z": z = Convert.ToSingle(reader.Value); break;
                    }
                }
            }
            return new Vector3(x, y, z);
        }
    }

    /// <summary>
    /// JSON converter for UnityEngine.Vector4.
    /// Serialized shape: { "x": ..., "y": ..., "z": ..., "w": ... }
    /// </summary>
    public class Vector4Converter : JsonConverter<Vector4>
    {
        public override void WriteJson(JsonWriter writer, Vector4 value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("x"); writer.WriteValue(value.x);
            writer.WritePropertyName("y"); writer.WriteValue(value.y);
            writer.WritePropertyName("z"); writer.WriteValue(value.z);
            writer.WritePropertyName("w"); writer.WriteValue(value.w);
            writer.WriteEndObject();
        }

        public override Vector4 ReadJson(JsonReader reader, Type objectType, Vector4 existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            float x = 0, y = 0, z = 0, w = 0;
            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.EndObject)
                    break;

                if (reader.TokenType == JsonToken.PropertyName)
                {
                    var prop = (string)reader.Value;
                    reader.Read();
                    switch (prop)
                    {
                        case "x": x = Convert.ToSingle(reader.Value); break;
                        case "y": y = Convert.ToSingle(reader.Value); break;
                        case "z": z = Convert.ToSingle(reader.Value); break;
                        case "w": w = Convert.ToSingle(reader.Value); break;
                    }
                }
            }
            return new Vector4(x, y, z, w);
        }
    }

    /// <summary>
    /// JSON converter for UnityEngine.Quaternion.
    /// Serialized shape: { "x": ..., "y": ..., "z": ..., "w": ... }
    /// </summary>
    public class QuaternionConverter : JsonConverter<Quaternion>
    {
        public override void WriteJson(JsonWriter writer, Quaternion value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("x"); writer.WriteValue(value.x);
            writer.WritePropertyName("y"); writer.WriteValue(value.y);
            writer.WritePropertyName("z"); writer.WriteValue(value.z);
            writer.WritePropertyName("w"); writer.WriteValue(value.w);
            writer.WriteEndObject();
        }

        public override Quaternion ReadJson(JsonReader reader, Type objectType, Quaternion existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            float x = 0, y = 0, z = 0, w = 1;
            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.EndObject)
                    break;

                if (reader.TokenType == JsonToken.PropertyName)
                {
                    var prop = (string)reader.Value;
                    reader.Read();
                    switch (prop)
                    {
                        case "x": x = Convert.ToSingle(reader.Value); break;
                        case "y": y = Convert.ToSingle(reader.Value); break;
                        case "z": z = Convert.ToSingle(reader.Value); break;
                        case "w": w = Convert.ToSingle(reader.Value); break;
                    }
                }
            }
            return new Quaternion(x, y, z, w);
        }
    }

    /// <summary>
    /// JSON converter for UnityEngine.Color.
    /// Serialized shape: { "r": ..., "g": ..., "b": ..., "a": ... }
    /// </summary>
    public class ColorConverter : JsonConverter<Color>
    {
        public override void WriteJson(JsonWriter writer, Color value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("r"); writer.WriteValue(value.r);
            writer.WritePropertyName("g"); writer.WriteValue(value.g);
            writer.WritePropertyName("b"); writer.WriteValue(value.b);
            writer.WritePropertyName("a"); writer.WriteValue(value.a);
            writer.WriteEndObject();
        }

        public override Color ReadJson(JsonReader reader, Type objectType, Color existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            float r = 0, g = 0, b = 0, a = 1;
            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.EndObject)
                    break;
                if (reader.TokenType == JsonToken.PropertyName)
                {
                    var prop = (string)reader.Value;
                    reader.Read();
                    switch (prop)
                    {
                        case "r": r = Convert.ToSingle(reader.Value); break;
                        case "g": g = Convert.ToSingle(reader.Value); break;
                        case "b": b = Convert.ToSingle(reader.Value); break;
                        case "a": a = Convert.ToSingle(reader.Value); break;
                    }
                }
            }
            return new Color(r, g, b, a);
        }
    }

    /// <summary>
    /// JSON converter for UnityEngine.Rect.
    /// Serialized shape: { "x": ..., "y": ..., "width": ..., "height": ... }
    /// </summary>
    public class RectConverter : JsonConverter<Rect>
    {
        public override void WriteJson(JsonWriter writer, Rect value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("x"); writer.WriteValue(value.x);
            writer.WritePropertyName("y"); writer.WriteValue(value.y);
            writer.WritePropertyName("width"); writer.WriteValue(value.width);
            writer.WritePropertyName("height"); writer.WriteValue(value.height);
            writer.WriteEndObject();
        }

        public override Rect ReadJson(JsonReader reader, Type objectType, Rect existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            float x = 0, y = 0, w = 0, h = 0;
            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.EndObject)
                    break;
                if (reader.TokenType == JsonToken.PropertyName)
                {
                    var prop = (string)reader.Value;
                    reader.Read();
                    switch (prop)
                    {
                        case "x": x = Convert.ToSingle(reader.Value); break;
                        case "y": y = Convert.ToSingle(reader.Value); break;
                        case "width": w = Convert.ToSingle(reader.Value); break;
                        case "height": h = Convert.ToSingle(reader.Value); break;
                    }
                }
            }
            return new Rect(x, y, w, h);
        }
    }

    /// <summary>
    /// JSON converter for UnityEngine.Bounds.
    /// Serialized shape: { "center": {x,y,z}, "size": {x,y,z} }
    /// Uses serializer for nested Vector3, so make sure Vector3Converter is registered too.
    /// </summary>
    public class BoundsConverter : JsonConverter<Bounds>
    {
        public override void WriteJson(JsonWriter writer, Bounds value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("center"); serializer.Serialize(writer, value.center);
            writer.WritePropertyName("size"); serializer.Serialize(writer, value.size);
            writer.WriteEndObject();
        }

        public override Bounds ReadJson(JsonReader reader, Type objectType, Bounds existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            Vector3 center = Vector3.zero;
            Vector3 size = Vector3.one;

            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.EndObject)
                    break;
                if (reader.TokenType == JsonToken.PropertyName)
                {
                    var prop = (string)reader.Value;
                    reader.Read();
                    switch (prop)
                    {
                        case "center": center = serializer.Deserialize<Vector3>(reader); break;
                        case "size": size = serializer.Deserialize<Vector3>(reader); break;
                    }
                }
            }

            return new Bounds(center, size);
        }
    }

    /// <summary>
    /// JSON converter for UnityEngine.Matrix4x4.
    /// Serialized shape: array of 16 floats [m00, m10, m20, m30, m01, ...]
    /// This is compact and sufficient for save/load.
    /// </summary>
    public class Matrix4x4Converter : JsonConverter<Matrix4x4>
    {
        public override void WriteJson(JsonWriter writer, Matrix4x4 value, JsonSerializer serializer)
        {
            writer.WriteStartArray();
            for (int i = 0; i < 16; i++)
                writer.WriteValue(value[i]);
            writer.WriteEndArray();
        }

        public override Matrix4x4 ReadJson(JsonReader reader, Type objectType, Matrix4x4 existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var m = new Matrix4x4();
            int idx = 0;

            if (reader.TokenType == JsonToken.StartArray)
            {
                while (reader.Read() && reader.TokenType != JsonToken.EndArray)
                {
                    if (idx < 16)
                    {
                        m[idx] = Convert.ToSingle(reader.Value);
                        idx++;
                    }
                }
            }

            return m;
        }
    }

    /// <summary>
    /// JSON converter for UnityEngine.LayerMask.
    /// Serialized shape: { "value": int }
    /// </summary>
    public class LayerMaskConverter : JsonConverter<LayerMask>
    {
        public override void WriteJson(JsonWriter writer, LayerMask value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("value"); writer.WriteValue(value.value);
            writer.WriteEndObject();
        }

        public override LayerMask ReadJson(JsonReader reader, Type objectType, LayerMask existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            int v = 0;
            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.EndObject)
                    break;
                if (reader.TokenType == JsonToken.PropertyName)
                {
                    var prop = (string)reader.Value;
                    reader.Read();
                    if (prop == "value")
                        v = Convert.ToInt32(reader.Value);
                }
            }

            return new LayerMask { value = v };
        }
    }
}

/*
--------------------------------------------------------------
Quick usage examples
--------------------------------------------------------------

// 1) Register converters in your save system:
var settings = new JsonSerializerSettings
{
    Converters =
    {
        new Vector2Converter(),
        new Vector3Converter(),
        new Vector4Converter(),
        new QuaternionConverter(),
        new ColorConverter(),
        new RectConverter(),
        new BoundsConverter(),
        new Matrix4x4Converter(),
        new LayerMaskConverter()
    }
};

// 2) Serialize a Unity struct:
var pos = new Vector3(1f, 2f, 3f);
string json = JsonConvert.SerializeObject(pos, settings);

// 3) Deserialize it back:
var posBack = JsonConvert.DeserializeObject<Vector3>(json, settings);

// 4) Use inside your SaveManager:
// _jsonSettings = new JsonSerializerSettings { ... }
// _jsonSettings.Converters.Add(new Vector3Converter());
// _jsonSettings.Converters.Add(new QuaternionConverter());
// ...
*/

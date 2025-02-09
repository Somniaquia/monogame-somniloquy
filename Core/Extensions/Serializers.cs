namespace Somniloquy {
    using System;
    using System.Collections.Generic;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    using Microsoft.Xna.Framework;

    public class Vector2IKeyDictionaryConverter<TValue> : JsonConverter<Dictionary<Vector2I, TValue>> {
        public override Dictionary<Vector2I, TValue> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
            var result = new Dictionary<Vector2I, TValue>();
            if (reader.TokenType != JsonTokenType.StartObject)
                throw new JsonException();
            
            reader.Read(); // Move past StartObject
            while (reader.TokenType == JsonTokenType.PropertyName) {
                var keyPair = reader.GetString().Split(' ');
                Vector2I vectorKey = new Vector2I(int.Parse(keyPair[0]), int.Parse(keyPair[1]));

                reader.Read(); // Move to value
                TValue value = JsonSerializer.Deserialize<TValue>(ref reader, options);
                result.Add(vectorKey, value);

                reader.Read(); // Move to next property or EndObject
            }

            if (reader.TokenType != JsonTokenType.EndObject)
                throw new JsonException();

            return result;
        }

        public override void Write(Utf8JsonWriter writer, Dictionary<Vector2I, TValue> value, JsonSerializerOptions options) {
            writer.WriteStartObject();
            foreach (var kvp in value) {
                writer.WritePropertyName($"{kvp.Key.X} {kvp.Key.Y}");
                JsonSerializer.Serialize(writer, kvp.Value, options);
            }
            writer.WriteEndObject();
        }
    }

    public class Vector2IConverter : JsonConverter<Vector2I> {
        public override Vector2I Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
            if (reader.TokenType != JsonTokenType.String)
                throw new JsonException("Expected string for Vector2I.");

            var pair = reader.GetString().Split(' ');
            return new Vector2I(int.Parse(pair[0]), int.Parse(pair[1]));
        }

        public override void Write(Utf8JsonWriter writer, Vector2I value, JsonSerializerOptions options) {
            writer.WriteStringValue($"{value.X} {value.Y}");
        }
    }

    public class Vector2Converter : JsonConverter<Vector2> {
        public override Vector2 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
            if (reader.TokenType != JsonTokenType.String)
                throw new JsonException("Expected string for Vector2.");

            var pair = reader.GetString().Split(' ');
            return new Vector2(float.Parse(pair[0]), float.Parse(pair[1]));
        }

        public override void Write(Utf8JsonWriter writer, Vector2 value, JsonSerializerOptions options) {
            writer.WriteStringValue($"{value.X} {value.Y}");
        }
    }

    public class RectangleFConverter : JsonConverter<RectangleF> {
        public override RectangleF Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
            if (reader.TokenType != JsonTokenType.String)
                throw new JsonException("Expected string for RectangleF.");

            var pair = reader.GetString().Split(' ');
            return new RectangleF(float.Parse(pair[0]), float.Parse(pair[1]), float.Parse(pair[2]), float.Parse(pair[3]));
        }

        public override void Write(Utf8JsonWriter writer, RectangleF value, JsonSerializerOptions options) {
            writer.WriteStringValue($"{value.X} {value.Y} {value.Width} {value.Height}");
        }
    }
    
    public class RectangleConverter : JsonConverter<Rectangle> {
        public override Rectangle Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
            if (reader.TokenType != JsonTokenType.String)
                throw new JsonException("Expected string for Rectangle.");

            var pair = reader.GetString().Split(' ');
            return new Rectangle(int.Parse(pair[0]), int.Parse(pair[1]), int.Parse(pair[2]), int.Parse(pair[3]));
        }

        public override void Write(Utf8JsonWriter writer, Rectangle value, JsonSerializerOptions options) {
            writer.WriteStringValue($"{value.X} {value.Y} {value.Width} {value.Height}");
        }
    }
    
    public class CircleFConverter : JsonConverter<CircleF> {
        public override CircleF Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
            if (reader.TokenType != JsonTokenType.String)
                throw new JsonException("Expected string for CircleF.");

            var pair = reader.GetString().Split(' ');
            return new CircleF(new Vector2(float.Parse(pair[0]), float.Parse(pair[1])), float.Parse(pair[2]));
        }

        public override void Write(Utf8JsonWriter writer, CircleF value, JsonSerializerOptions options) {
            writer.WriteStringValue($"{value.Center.X} {value.Center.Y} {value.Radius}");
        }
    }
}
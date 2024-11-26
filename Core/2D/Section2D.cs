namespace Somniloquy {
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    
    using Microsoft.Xna.Framework;

    public class Section2D {
        public Section2DScreen Screen;
        public World World;

        public string Identifier;
        public Vector2 CoordsInWorldMap;
        public Dictionary<int, LayerGroup2D> LayerGroups = new();

        public void LoadLayerGroup() { }
        public void UnloadLayerGroup() { }

        public void Update() {
            foreach (var layerGroup in LayerGroups.Select(pair => pair.Value)) {
                if (!layerGroup.Loaded) continue;
                layerGroup.Update();
            }
        }

        public void Draw(Camera2D camera) {
            foreach (var layerGroup in LayerGroups) {
                layerGroup.Value.Draw(camera);
            }
        }

        public string Serialize() {
            var options = new JsonSerializerOptions();
            options.Converters.Add(new SQTexture2DConverter());
            options.Converters.Add(new TextureChunk2DConverter());
            options.Converters.Add(new TextureLayer2DConverter());
            options.Converters.Add(new LayerGroup2DConverter());
            options.Converters.Add(new Section2DConverter());

            return JsonSerializer.Serialize(this, options);
        }

        public static Section2D Deserialize(string json) {
            var options = new JsonSerializerOptions();
            options.Converters.Add(new SQTexture2DConverter());
            options.Converters.Add(new TextureChunk2DConverter());
            options.Converters.Add(new TextureLayer2DConverter());
            options.Converters.Add(new LayerGroup2DConverter());
            options.Converters.Add(new Section2DConverter());

            return JsonSerializer.Deserialize<Section2D>(json, options);
        }
    }

    public class Section2DConverter : JsonConverter<Section2D> {
        public override Section2D Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
            reader.Read();

            string identifier = null;
            Vector2 coordsInWorldMap = default;
            var layerGroups = new Dictionary<int, LayerGroup2D>();

            while (reader.TokenType == JsonTokenType.PropertyName) {
                string propertyName = reader.GetString();
                reader.Read();

                if (propertyName == "Identifier") {
                    identifier = reader.GetString();
                } else if (propertyName == "CoordsInWorldMap") {
                    coordsInWorldMap = JsonSerializer.Deserialize<Vector2>(ref reader, options);
                } else if (propertyName == "LayerGroups") {
                    if (reader.TokenType != JsonTokenType.StartObject)
                        throw new JsonException("LayerGroups should be an object");

                    reader.Read();
                    while (reader.TokenType == JsonTokenType.PropertyName) {
                        int groupKey = int.Parse(reader.GetString());
                        reader.Read();

                        var layerGroup = JsonSerializer.Deserialize<LayerGroup2D>(ref reader, options);
                        layerGroups.Add(groupKey, layerGroup);

                        reader.Read();
                    }
                }
                reader.Read();
            }

            var section = new Section2D {
                Identifier = identifier,
                CoordsInWorldMap = coordsInWorldMap,
                LayerGroups = layerGroups
            };

            return section;
        }

        public override void Write(Utf8JsonWriter writer, Section2D value, JsonSerializerOptions options) {
            writer.WriteStartObject();

            writer.WriteString("Identifier", value.Identifier);
            writer.WritePropertyName("CoordsInWorldMap");
            JsonSerializer.Serialize(writer, value.CoordsInWorldMap, options);

            writer.WritePropertyName("LayerGroups");
            writer.WriteStartObject();

            foreach (var kvp in value.LayerGroups) {
                writer.WritePropertyName(kvp.Key.ToString());
                JsonSerializer.Serialize(writer, kvp.Value, options);
            }

            writer.WriteEndObject();
            writer.WriteEndObject();
        }
    }
}
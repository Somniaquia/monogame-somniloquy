namespace Somniloquy {
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;

    public partial class Section2D {
        public Section2DScreen Screen; // ignore when serializing
        public World World; // circular reference to parent World

        public string Identifier;
        public Vector2 CoordsInWorldMap;
        public Dictionary<string, LayerGroup2D> LayerGroups = new();

        public void LoadLayerGroup() { }
        public void UnloadLayerGroup() { }

        public void AddLayerGroup(string groupName) {
            LayerGroup2D layerGroup = new(groupName);
            LayerGroups.Add(groupName, layerGroup);
        }

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
            var layerGroups = new Dictionary<string, LayerGroup2D>();

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
                        string groupName = reader.GetString();
                        reader.Read();

                        var layerGroup = JsonSerializer.Deserialize<LayerGroup2D>(ref reader, options);
                        layerGroups.Add(groupName, layerGroup);

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
                writer.WritePropertyName(kvp.Key);
                JsonSerializer.Serialize(writer, kvp.Value, options);
            }

            writer.WriteEndObject();
            writer.WriteEndObject();
        }
    }


    public partial class LayerGroup2D {
        public Section2D Section; // again, circular reference
        
        public string Identifier;
        public List<Layer2D> Layers = new();
        public bool Loaded; // ignore

        public void AddLayer(Layer2D layer) {
            Layers.Add(layer);
        }

        public LayerGroup2D(string identifier) {
            Identifier = identifier;
        }

        public Rectangle GetTextureBounds() {
            List<Rectangle> bounds = (from layer in Layers where layer is TextureLayer2D let textureLayer = (TextureLayer2D)layer select textureLayer.GetTextureBounds()).ToList();

            int xMin = bounds.Min(bound => bound.Left);
            int xMax = bounds.Max(bound => bound.Right);
            int yMin = bounds.Min(bound => bound.Top);
            int yMax = bounds.Max(bound => bound.Bottom);

            return new Rectangle(xMin, yMin, xMax - xMin, yMax - yMin);
        }

        public void SaveTexture(string path) {
            Texture2D texture = GetTexture();
            using var fileStream = new FileStream(path, FileMode.Create);
            texture.SaveAsPng(fileStream, texture.Width, texture.Height);
        }

        public Texture2D GetTexture() {
            var bounds = GetTextureBounds();
            RenderTarget2D target = new(SQ.GD, bounds.Width, bounds.Height);

            SQ.GD.SetRenderTarget(target);
            SQ.GD.Clear(Color.Transparent);
            SQ.SB.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp);
            foreach (var layer in Layers) {
                if (layer is TextureLayer2D textureLayer) {
                    textureLayer.Draw(bounds.TopLeft(), bounds.BottomRight());
                }
            }
            SQ.SB.End();
            SQ.GD.SetRenderTarget(null);
            return target;
        }

        public void Update() {
            foreach (var layer in Layers) {
                layer.Update();
            }
        }

        public void Draw() {

        }

        public void Draw(Camera2D camera) {
            foreach (var layer in Layers) {
                layer.Draw(camera);
            }
        }
    }

    public class LayerGroup2DConverter : JsonConverter<LayerGroup2D> {
        public override LayerGroup2D Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
            if (reader.TokenType != JsonTokenType.StartObject) throw new JsonException();

            string identifier = null;
            var layers = new List<Layer2D>();

            while (reader.Read() && reader.TokenType != JsonTokenType.EndObject) {
                if (reader.TokenType != JsonTokenType.PropertyName) continue;

                string propertyName = reader.GetString();
                reader.Read();

                if (propertyName == "Identifier") {
                    identifier = reader.GetString();
                } else if (propertyName == "Layers") {
                    if (reader.TokenType != JsonTokenType.StartArray) throw new JsonException("Layers should be an array");

                    // Read each layer in the array
                    while (reader.Read() && reader.TokenType != JsonTokenType.EndArray) {
                        if (reader.TokenType != JsonTokenType.StartObject) throw new JsonException("Expected object in Layers array");

                        string type = null;
                        JsonDocument layerData = null;

                        // Read properties within each layer object
                        while (reader.Read() && reader.TokenType != JsonTokenType.EndObject) {
                            if (reader.TokenType != JsonTokenType.PropertyName) continue;

                            string layerProperty = reader.GetString();
                            reader.Read();

                            if (layerProperty == "Type") {
                                type = reader.GetString();
                            } else if (layerProperty == "Data") {
                                layerData = JsonDocument.ParseValue(ref reader);
                            }
                        }

                        if (type != null && layerData != null) {
                            Layer2D layer;
                            if (type == "TextureLayer2D") {
                                layer = JsonSerializer.Deserialize<TextureLayer2D>(layerData.RootElement.GetRawText(), options);
                            } else {
                                throw new JsonException($"Unknown Layer2D type: {type}");
                            }
                            layers.Add(layer);
                        }
                    }
                }
            }

            if (identifier == null) throw new JsonException("LayerGroup2D is missing Identifier");

            return new LayerGroup2D(identifier) { Layers = layers };
        }

        public override void Write(Utf8JsonWriter writer, LayerGroup2D value, JsonSerializerOptions options) {
            writer.WriteStartObject();

            writer.WriteString("Identifier", value.Identifier);
            writer.WritePropertyName("Layers");
            writer.WriteStartArray();

            foreach (var layer in value.Layers) {
                writer.WriteStartObject();

                if (layer is TextureLayer2D) {
                    writer.WriteString("Type", "TextureLayer2D");
                    writer.WritePropertyName("Data");
                    JsonSerializer.Serialize(writer, (TextureLayer2D)layer, options);
                } else {
                    throw new JsonException($"Unsupported Layer2D type: {layer.GetType().Name}");
                }

                writer.WriteEndObject();
            }

            writer.WriteEndArray();
            writer.WriteEndObject();
        }
    }

}
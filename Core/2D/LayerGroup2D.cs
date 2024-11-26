namespace Somniloquy {
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;

    public class LayerGroup2D {
        public Section2D Section;
        
        public string Identifier;
        public Dictionary<int, Layer2D> Layers = new();
        public bool Loaded;

        public LayerGroup2D() {
        }

        public Rectangle GetTextureBounds() {
            List<Rectangle> bounds = (from layer in Layers where layer.Value is TextureLayer2D let textureLayer = (TextureLayer2D)layer.Value select textureLayer.GetTextureBounds()).ToList();

            int xMin = bounds.Min(bound => bound.Left);
            int xMax = bounds.Max(bound => bound.Right);
            int yMin = bounds.Min(bound => bound.Top);
            int yMax = bounds.Max(bound => bound.Bottom);

            return new Rectangle(xMin, yMin, xMax - xMin, yMax - yMin);
        }

        public void SaveTexture(string path) {
            Texture2D texture = GetTexture();
            // using var fileStream = new FileStream(path, FileMode.Create);
            // texture.SaveAsPng(fileStream, texture.Width, texture.Height);
            IOManager.SaveTextureDataAsPngAsync(texture, path);
        }

        public Texture2D GetTexture() {
            var bounds = GetTextureBounds();
            RenderTarget2D target = new(SQ.GD, bounds.Width, bounds.Height);

            SQ.GD.SetRenderTarget(target);
            SQ.GD.Clear(Color.Transparent);
            SQ.SB.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp);
            foreach (var layer in Layers) {
                if (layer.Value is TextureLayer2D textureLayer) {
                    textureLayer.Draw(bounds.TopLeft(), bounds.BottomRight());
                }
            }
            SQ.SB.End();
            SQ.GD.SetRenderTarget(null);
            return target;
        }

        public void Update() {
            foreach (var layer in Layers) {
                layer.Value.Update();
            }
        }

        public void Draw() {

        }

        public void Draw(Camera2D camera) {
            foreach (var layer in Layers) {
                layer.Value.Draw(camera);
            }
        }
    }

    public class LayerGroup2DConverter : JsonConverter<LayerGroup2D> {
        public override LayerGroup2D Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
            if (reader.TokenType != JsonTokenType.StartObject) throw new JsonException();

            var layers = new Dictionary<int, Layer2D>();
            string identifier = null;
            while (reader.Read() && reader.TokenType != JsonTokenType.EndObject) {
                if (reader.TokenType != JsonTokenType.PropertyName) continue;

                string propertyName = reader.GetString();
                reader.Read();

                if (propertyName == "Identifier") {
                    identifier = reader.GetString();
                } else if (propertyName == "Layers") {
                    if (reader.TokenType != JsonTokenType.StartObject) throw new JsonException("Layers should be an object");

                    while (reader.Read() && reader.TokenType != JsonTokenType.EndObject) {
                        if (reader.TokenType != JsonTokenType.PropertyName) continue;

                        int key = int.Parse(reader.GetString());
                        reader.Read();

                        if (reader.TokenType != JsonTokenType.StartObject) throw new JsonException("Expected object for Layer2D");

                        string type = null;
                        JsonDocument layerData = null;

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
                            layers[key] = layer;
                        }
                    }
                }
            }

            return new LayerGroup2D() { Identifier = identifier, Layers = layers };
        }


        public override void Write(Utf8JsonWriter writer, LayerGroup2D value, JsonSerializerOptions options) {
            writer.WriteStartObject();

            writer.WriteString("Identifier", value.Identifier);
            writer.WritePropertyName("Layers");
            writer.WriteStartArray();

            foreach (var layer in value.Layers) {
                writer.WriteStartObject();

                if (layer.Value is TextureLayer2D textureLayer) {
                    writer.WriteString("Type", "TextureLayer2D");
                    writer.WritePropertyName("Data");
                    JsonSerializer.Serialize(writer, layer, options);
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
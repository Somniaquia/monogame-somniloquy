namespace Somniloquy {
    using System;
    using System.Collections.Generic;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using Microsoft.Xna.Framework;

    public abstract class Layer2D {
        [JsonIgnore] public Section2D Section;
        [JsonInclude] public string Identifier = "";
        [JsonIgnore] public bool Enabled = true;
        [JsonIgnore] public float Opacity = 1f;
        
        public abstract void Update();
        public abstract void Draw(Camera2D camera);
    }

    public interface IPaintableLayer2D {
        public void PaintRectangle(Vector2I startPosition, Vector2I endPosition, Color color, float opacity, bool filled, CommandChain chain = null) {
            PixelActions.ApplyRectangleAction(startPosition, endPosition, filled, (Vector2I position) => {
                PaintPixel(position, color, opacity, chain);
            });
        }

        public void PaintLine(Vector2I start, Vector2I end, Color color, float opacity, int width = 0, CommandChain chain = null) {
            PixelActions.ApplyLineAction(start, end, width, (Vector2I position) => {
                PaintPixel(position, color, opacity, chain);
            });
        }

        public void PaintCircle(Vector2I center, int radius, Color color, float opacity, bool filled = true, CommandChain chain = null) {
            PixelActions.ApplyCircleAction(center, radius, filled, (Vector2I position) => {
                PaintPixel(position, color, opacity, chain);
            });
        }

        // TODO: Paint Fill

        public virtual Color? GetColor(Vector2I position) { return null; }

        public abstract void PaintPixel(Vector2I position, Color color, float opacity, CommandChain chain = null);
    }

    public class Layer2DConverter : JsonConverter<Layer2D> {
        public override Layer2D Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
            using (JsonDocument document = JsonDocument.ParseValue(ref reader)) {
                var root = document.RootElement;

                if (!root.TryGetProperty("Type", out var typeProp))
                    throw new JsonException("Missing type discriminator in JSON.");

                string type = typeProp.GetString();

                return type switch {
                    "TextureLayer2D" => JsonSerializer.Deserialize<TextureLayer2D>(root.GetRawText(), options),
                    "TileLayer2D" => JsonSerializer.Deserialize<TileLayer2D>(root.GetRawText(), options),
                    _ => throw new JsonException($"Unsupported type: {type}")
                };
            }
        }

        public override void Write(Utf8JsonWriter writer, Layer2D value, JsonSerializerOptions options) {
            writer.WriteStartObject();
            writer.WriteString("Type", value.GetType().Name);
            foreach (var property in JsonSerializer.SerializeToElement(value, value.GetType(), options).EnumerateObject()) {
                property.WriteTo(writer);
            }

            writer.WriteEndObject();
        }
    }
}
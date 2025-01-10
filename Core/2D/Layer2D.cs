namespace Somniloquy {
    using System;
    using System.Collections.Generic;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using Microsoft.Xna.Framework;

    /// <summary>
    /// **Layer** is the smallest unit of the World. 
    /// It is a 3D data representation of space (I'm thinking of a 1. chunked voxel map like in Minecraft 2. aided by additional Object Layer which stores objects and their orientations placed on top of the voxel map). 
    /// By applying it's parent Portion's transformations, distortion of space will occur 
    /// (Cityscape - buildings, roads fold like in movie Inception, impossible geometries like in Monumental Valley, shortening or lengthening, rotation, complex distortions of map for puzzle solutions, gravity direction, time flow speed, and environmental effects.) 
    /// Also stores per-voxel event data in the voxel map.<br/>
    /// <br/>
    /// Layer2D is its 2D counterpart, with Tile2Ds and 2D transformations
    /// <br/><br/>
    /// It is a more viable choice to render the chunks in the Layer2D classes, rather than to have the datatypes as Node2Ds, that will cause drawbacks in performance.
    /// </summary>
    public class Layer2D {
        [JsonInclude] public string Identifier;
        [JsonInclude] public Vector2 CoordsInSection;
        // TODO: Translation expressions

        public virtual void Update() { }

        public virtual void Draw(Camera2D camera, float opacity = 1f) { }
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
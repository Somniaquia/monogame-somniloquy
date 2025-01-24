namespace Somniloquy {
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    
    using Microsoft.Xna.Framework;

    public class Section2D {
        [JsonIgnore] public Section2DScreen Screen;
        [JsonIgnore] public World World;

        [JsonInclude] public string Identifier;
        [JsonInclude] public Vector2 CoordsInWorldMap;
        [JsonInclude] public List<Layer2D> Layers = new();
        [JsonInclude] public TileSpriteSheet SpriteSheet = new(16, 64);

        public Layer2D AddLayer(Layer2D layer) {
            Layers.Add(layer);
            layer.Section = this;
            return layer;
        }

        public void Update() {
            foreach (var layer in Layers) {
                if (layer.Enabled) layer.Update();
            }
        }

        public void Draw(Camera2D camera) {
            foreach (var layer in Layers) {
                if (layer.Enabled) layer.Draw(camera);
            }
        }

        public string Serialize() {
            var options = new JsonSerializerOptions {
                WriteIndented = true, Converters = {
                    new Layer2DConverter(),
                    new SQTexture2DConverter(),
                    new Vector2IKeyDictionaryConverter<TextureChunk2D>(),
                    new Vector2IKeyDictionaryConverter<TileChunk2D>(),
                    new TileChunk2DConverter(),
                    new Vector2IConverter(),
                }
            };

            return JsonSerializer.Serialize(this, options);
        }

        public static Section2D Deserialize(string json) {
            var options = new JsonSerializerOptions {
                Converters = {
                    new Layer2DConverter(),
                    new SQTexture2DConverter(),
                    new Vector2IKeyDictionaryConverter<TextureChunk2D>(),
                    new Vector2IKeyDictionaryConverter<TileChunk2D>(),
                    new TileChunk2DConverter(),
                    new Vector2IConverter(),
                }
            };

            return JsonSerializer.Deserialize<Section2D>(json, options);
        }
    }
}
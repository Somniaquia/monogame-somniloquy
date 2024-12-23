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
        [JsonInclude] public Dictionary<int, LayerGroup2D> LayerGroups = new();

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
            var options = new JsonSerializerOptions {
                WriteIndented = true, Converters = {
                    new Layer2DConverter(),
                    new SQTexture2DConverter(),
                    new Vector2IKeyDictionaryConverter<TextureChunk2D>(),
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
                    new Vector2IConverter(),
                }
            };

            return JsonSerializer.Deserialize<Section2D>(json, options);
        }
    }
}
namespace Somniloquy {
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    
    using Microsoft.Xna.Framework;

    public class Section2D {
        [JsonInclude] public World World;
        [JsonInclude] public string Identifier;
        [JsonInclude] public Vector2 CoordsInWorldMap;
        [JsonInclude] public Layer2D Root;
        [JsonInclude] public TileSpriteSheet SpriteSheet = new(16, 64);

        public Section2D() {
            Root = new Layer2D("Root") { Section = this };
        }

        public void Update() {
            Root.Update();
        }

        public void Draw(Camera2D camera) {
            Root.Draw(camera);
        }

        public string Serialize() {
            var options = new JsonSerializerOptions {
                WriteIndented = true, 
                ReferenceHandler = ReferenceHandler.Preserve, 
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                Converters = {
                    new Vector2IKeyDictionaryConverter<TextureChunk2D>(),
                    new Vector2IKeyDictionaryConverter<TileChunk2D>(),
                    new Tile2DArrayConverter(),
                    new SQTexture2DConverter(),
                    new Vector2IConverter(),
                }
            };

            return JsonSerializer.Serialize(this, options);
        }

        public static Section2D Deserialize(string json) {
            var options = new JsonSerializerOptions {
                ReferenceHandler = ReferenceHandler.Preserve, 
                Converters = {
                    new Vector2IKeyDictionaryConverter<TextureChunk2D>(),
                    new Vector2IKeyDictionaryConverter<TileChunk2D>(),
                    new Tile2DArrayConverter(),
                    new SQTexture2DConverter(),
                    new Vector2IConverter(),
                }
            };

            return JsonSerializer.Deserialize<Section2D>(json, options);
        }
    }
}
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
        
        [JsonInclude] public TileSpriteSheet TileSpriteSheet;
        [JsonInclude] public Sprite2D TileSprite;
        [JsonInclude] public uint NextTileID;

        [JsonInclude] public Color BackgroundColor = Color.Black;
        [JsonInclude] public Color GridColor;

        public Section2D() {
            Root = new Layer2D("Root") { Section = this };
            GridColor = Util.InvertColor(BackgroundColor);

            TileSpriteSheet = new(16, 64);
            TileSprite = new(("Tile", "0"), ("Animation", "0"), ("Frame", "0"));
        }

        public void Update() {
            Root.Update();
        }

        public void Draw(Camera2D camera, bool collisionBounds = false) {
            SQ.GD.Clear(BackgroundColor);
            Root.Draw(camera, collisionBounds);
        }

        public string Serialize() {
            var options = new JsonSerializerOptions {
                ReferenceHandler = ReferenceHandler.Preserve, 
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                Converters = {
                    new Vector2IKeyDictionaryConverter<TextureChunk2D>(),
                    new Vector2IKeyDictionaryConverter<TileChunk2D>(),
                    new Vector2Converter(),
                    new Vector2IConverter(),
                    new RectangleConverter(),
                    new RectangleFConverter(),
                    new CircleFConverter(),
                    new Tile2DArrayConverter(),
                    new SQTexture2DConverter(),
                    new Vector2IConverter(),
                }
            };
            options.ReferenceHandler = new SQReferenceHandler();
            return JsonSerializer.Serialize(this, options);
        }

        public static Section2D Deserialize(string json) {
            var options = new JsonSerializerOptions {
                ReferenceHandler = ReferenceHandler.Preserve, 
                Converters = {
                    new Vector2IKeyDictionaryConverter<TextureChunk2D>(),
                    new Vector2IKeyDictionaryConverter<TileChunk2D>(),
                    new Vector2Converter(),
                    new Vector2IConverter(),
                    new RectangleConverter(),
                    new RectangleFConverter(),
                    new CircleFConverter(),
                    new Tile2DArrayConverter(),
                    new SQTexture2DConverter(),
                    new Vector2IConverter(),
                }
            };
            options.ReferenceHandler = new SQReferenceHandler();
            return JsonSerializer.Deserialize<Section2D>(json, options);
        }
    }
}
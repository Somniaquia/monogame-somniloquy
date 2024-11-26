namespace Somniloquy {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;

    public partial class TextureLayer2D : Layer2D, IPaintableLayer2D {
        public int ChunkLength = 128;
        public Dictionary<Vector2I, TextureChunk2D> Chunks = new();

        public Vector2I GetChunkPosition(Vector2I canvasPosition) => canvasPosition / ChunkLength;
        public Vector2I GetPositionInChunk(Vector2I canvasPosition) => Util.PosMod(canvasPosition, new Vector2I(ChunkLength, ChunkLength));
        
        public void PaintImage(Vector2I position, Texture2D texture, float opacity, CommandChain chain = null) {
            Color[] data = new Color[texture.Width * texture.Height];
            texture.GetData(data);

            for (int y = 0; y < texture.Height; y++) {
                for (int x = 0; x < texture.Width; x++) {
                    var pos = new Vector2I(x, y);
                    PaintPixel(position + pos, data[pos.Unwrap(texture.Width)], opacity, chain);
                }
            }
        }

        public void PaintPixel(Vector2I position, Color color, float opacity, CommandChain chain = null) {
            var (chunkPosition, positionInChunk) = (GetChunkPosition(position), GetPositionInChunk(position));
            
            if (!Chunks.ContainsKey(chunkPosition)) {
                var chunk = new TextureChunk2D(this, ChunkLength);
                Chunks.Add(chunkPosition, chunk);
                chain?.AddCommand(new TextureChunkSetCommand(this, chunkPosition, null, chunk));
            }
            
            if (chain.AffectedPixels.ContainsKey(position)) {
                if (chain.AffectedPixels[position].Item2 >= opacity) return;
                Chunks[chunkPosition].SetPixel(positionInChunk, Util.BlendColor(chain.AffectedPixels[position].Item1, color, opacity), chain);
                chain.AffectedPixels[position] = (chain.AffectedPixels[position].Item1, opacity);
            } else {
                chain.AffectedPixels[position] = (Chunks[chunkPosition].GetColor(positionInChunk), opacity);
                Chunks[chunkPosition].PaintPixel(positionInChunk, color, opacity, chain);
            }
        }
        
        public Color? GetColor(Vector2I position) {
            var (chunkPosition, positionInChunk) = (GetChunkPosition(position), GetPositionInChunk(position));
            if (!Chunks.ContainsKey(chunkPosition)) return null;
            var color = Chunks[chunkPosition].GetColor(positionInChunk);
            if (color == Color.Transparent) return null;
            return color;
        }

        public Rectangle GetTextureBounds() {
            RemoveEmptyChunks();
            int chunkXMin = Chunks.Min(chunk => chunk.Key.X);
            int chunkXMax = Chunks.Max(chunk => chunk.Key.X);
            int chunkYMin = Chunks.Min(chunk => chunk.Key.Y);
            int chunkYMax = Chunks.Max(chunk => chunk.Key.Y);

            // var minChunk = Chunks[new Vector2I(chunkXMin, chunkYMin)];
            // var maxChunk = Chunks[new Vector2I(chunkXMax, chunkYMax)];
            
            // var minChunkBounds = minChunk.Texture.GetNonTransparentBounds(); // Oversight!
            // var maxChunkBounds = minChunk.Texture.GetNonTransparentBounds();

            // var xMin = chunkXMin * ChunkLength + minChunkBounds.Left;
            // var yMin = chunkYMin * ChunkLength + minChunkBounds.Top;
            // var xMax = chunkXMax * ChunkLength + minChunkBounds.Right;
            // var yMax = chunkYMax * ChunkLength + minChunkBounds.Bottom;
            var xMin = chunkXMin * ChunkLength;
            var yMin = chunkYMin * ChunkLength;
            var xMax = (chunkXMax + 1) * ChunkLength;
            var yMax = (chunkYMax + 1) * ChunkLength;

            return new Rectangle(xMin, yMin, xMax - xMin, yMax - yMin);
        }

        public void RemoveEmptyChunks() {
            // TODO: ensure that empty chunks are removed
        }

        public void Draw(Vector2I topLeft, Vector2I bottomRight, float opacity = 1f) {
            Vector2I topLeftChunk = new((int)((float)topLeft.X / ChunkLength) - 1, (int)((float)topLeft.Y / ChunkLength) - 1);
            Vector2I bottomRightChunk = new((int)((float)bottomRight.X / ChunkLength) + 1, (int)((float)bottomRight.Y / ChunkLength) + 1);

            for (int y = topLeftChunk.Y; y < bottomRightChunk.Y; y++) {
                for (int x = topLeftChunk.X; x < bottomRightChunk.X; x++) {
                    var chunkIndex = new Vector2I(x, y);
                    var chunkPos = new Vector2I(x, y) * ChunkLength;
                    var nextChunkPos = (chunkIndex + new Vector2I(1, 1)) * ChunkLength;

                    float xLeft = MathF.Min(MathF.Max(topLeft.X, chunkPos.X), bottomRight.X);
                    float xRight = MathF.Max(MathF.Min(bottomRight.X, nextChunkPos.X), topLeft.X);
                    float yTop = MathF.Min(MathF.Max(topLeft.Y, chunkPos.Y), bottomRight.Y);
                    float yBottom = MathF.Max(MathF.Min(bottomRight.Y, nextChunkPos.Y), topLeft.Y);

                    if (!Chunks.ContainsKey(chunkIndex)) continue;

                    SQ.SB.Draw(Chunks[chunkIndex].Texture, (Rectangle)new RectangleF(xLeft - topLeft.X, yTop - topLeft.Y, xRight - xLeft, yBottom - yTop), (Rectangle)new RectangleF(xLeft - chunkPos.X, yTop - chunkPos.Y , xRight - xLeft, yBottom - yTop), Color.White * opacity);
                }
            }
        }

        public override void Draw(Camera2D camera, bool drawOutlines = false, float opacity = 1f) {
            Vector2 topLeft = camera.VisibleRectangleInWorld.TopLeft() - new Vector2(1);
            Vector2 bottomRight = camera.VisibleRectangleInWorld.BottomRight() + new Vector2(1);
            Vector2I topLeftChunk = new((int)(topLeft.X / ChunkLength) - 1, (int)(topLeft.Y / ChunkLength) - 1);
            Vector2I bottomRightChunk = new((int)(bottomRight.X / ChunkLength) + 1, (int)(bottomRight.Y / ChunkLength) + 1);

            for (int y = topLeftChunk.Y; y < bottomRightChunk.Y; y++) {
                for (int x = topLeftChunk.X; x < bottomRightChunk.X; x++) {
                    var chunkIndex = new Vector2I(x, y);
                    var chunkPos = chunkIndex * ChunkLength;
                    var nextChunkPos = (chunkIndex + new Vector2I(1, 1)) * ChunkLength;

                    float xLeft = MathF.Min(MathF.Max(topLeft.X, chunkPos.X), bottomRight.X);
                    float xRight = MathF.Max(MathF.Min(bottomRight.X, nextChunkPos.X), topLeft.X);
                    float yTop = MathF.Min(MathF.Max(topLeft.Y, chunkPos.Y), bottomRight.Y);
                    float yBottom = MathF.Max(MathF.Min(bottomRight.Y, nextChunkPos.Y), topLeft.Y);

                    if (drawOutlines) {   
                        camera.DrawLine(chunkPos, (chunkIndex + new Vector2I(1, 0)) * ChunkLength, Color.Gray * 0.5f, scale: false);
                        camera.DrawLine(chunkPos, (chunkIndex + new Vector2I(0, 1)) * ChunkLength, Color.Gray * 0.5f, scale: false);
                    }
                    
                    if (!Chunks.ContainsKey(chunkIndex)) continue;

                    camera.Draw(Chunks[chunkIndex].Texture, (Rectangle)new RectangleF(xLeft, yTop, xRight - xLeft, yBottom - yTop), (Rectangle)new RectangleF(xLeft - chunkPos.X, yTop - chunkPos.Y , xRight - xLeft, yBottom - yTop), Color.White * opacity);
                }
            }
        }
    }
    
    public class TextureChunk2D {
        public TextureLayer2D ParentLayer;
        public SQTexture2D Texture;
        public int ChunkLength;
        
        public TextureChunk2D (TextureLayer2D parentLayer, int chunkLength) {
            ParentLayer = parentLayer;
            ChunkLength = chunkLength;
            Texture = new(SQ.GD, ChunkLength, ChunkLength);
        }

        public void SetPixel(Vector2I positionInChunk, Color color, CommandChain chain) {
            Texture.SetPixel(positionInChunk, color, chain);
        }

        public void PaintPixel(Vector2I positionInChunk, Color color, float opacity, CommandChain chain) {
            Texture.PaintPixel(positionInChunk, color, opacity, chain);
        }

        public Color GetColor(Vector2I positionInChunk) {
            return Texture.GetColor(positionInChunk);
        }
    }

    public class TextureLayer2DConverter : JsonConverter<TextureLayer2D> {
        public override TextureLayer2D Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
            reader.Read();

            int chunkLength = 0;
            var chunks = new Dictionary<Vector2I, TextureChunk2D>();

            while (reader.TokenType == JsonTokenType.PropertyName) {
                string propertyName = reader.GetString();
                reader.Read();

                if (propertyName == "ChunkLength") {
                    chunkLength = reader.GetInt32();
                } else if (propertyName == "Chunks") {
                    if (reader.TokenType != JsonTokenType.StartObject)
                        throw new JsonException("Chunks should be an object");

                    reader.Read();
                    while (reader.TokenType == JsonTokenType.PropertyName) {
                        string chunkKey = reader.GetString();
                        Vector2I chunkPosition = Vector2I.Deserialize(chunkKey);

                        reader.Read();
                        TextureChunk2D chunk = JsonSerializer.Deserialize<TextureChunk2D>(ref reader, options);
                        chunks.Add(chunkPosition, chunk);

                        reader.Read();
                    }
                }
                reader.Read();
            }

            var textureLayer = new TextureLayer2D();
            textureLayer.ChunkLength = chunkLength;

            foreach (var chunk in chunks) {
                chunk.Value.ParentLayer = textureLayer;
            }

            textureLayer.Chunks = chunks;
            return textureLayer;
        }

        public override void Write(Utf8JsonWriter writer, TextureLayer2D value, JsonSerializerOptions options) {
            writer.WriteStartObject();

            writer.WriteNumber("ChunkLength", value.ChunkLength);
            writer.WritePropertyName("Chunks");
            writer.WriteStartObject();

            foreach (var pair in value.Chunks) {
                string chunkKey = pair.Key.Serialize();
                writer.WritePropertyName(chunkKey);
                JsonSerializer.Serialize(writer, pair.Value, options);
            }

            writer.WriteEndObject();
            writer.WriteEndObject();
        }
    }

    public class TextureChunk2DConverter : JsonConverter<TextureChunk2D> {
        public override TextureChunk2D Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
            reader.Read();

            int chunkLength = 0;
            SQTexture2D texture = null;

            while (reader.TokenType == JsonTokenType.PropertyName) {
                string propertyName = reader.GetString();
                reader.Read();

                if (propertyName == "ChunkLength") {
                    chunkLength = reader.GetInt32();
                } else if (propertyName == "Texture") {
                    texture = JsonSerializer.Deserialize<SQTexture2D>(ref reader, options);
                }
                reader.Read();
            }

            if (texture == null)
                throw new JsonException("Missing Texture in JSON.");

            var chunk = new TextureChunk2D(null, chunkLength) { Texture = texture };
            return chunk;
        }

        public override void Write(Utf8JsonWriter writer, TextureChunk2D value, JsonSerializerOptions options) {
            writer.WriteStartObject();

            writer.WriteNumber("ChunkLength", value.ChunkLength);
            writer.WritePropertyName("Texture");
            JsonSerializer.Serialize(writer, value.Texture, options);

            writer.WriteEndObject();
        }
    }

}
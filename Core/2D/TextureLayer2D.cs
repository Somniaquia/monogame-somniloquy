namespace Somniloquy {
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.IO.Compression;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;

    public partial class TextureLayer2D : Layer2D, IPaintableLayer2D {
        public int ChunkLength = 128;
        public Dictionary<Vector2I, TextureChunk2D> Chunks = new();

        public Vector2I GetChunkPosition(Vector2I canvasPosition) => canvasPosition / ChunkLength;
        public Vector2I GetPositionInChunk(Vector2I canvasPosition) => Util.PosMod(canvasPosition, new Vector2I(ChunkLength, ChunkLength));
        
        public void PaintPixel(Vector2I position, Color color, float opacity, CommandChain chain = null) {
            var (chunkPosition, positionInChunk) = (GetChunkPosition(position), GetPositionInChunk(position));
            if (!Chunks.ContainsKey(chunkPosition)) {
                var chunk = new TextureChunk2D(this, ChunkLength);
                Chunks.Add(chunkPosition, chunk);
                chain?.AddCommand(new TextureChunkSetCommand(this, chunkPosition, null, chunk));
            }
            Chunks[chunkPosition].PaintPixel(positionInChunk, color, opacity, chain);
        }
        
        public Color? GetColor(Vector2I position) {
            var (chunkPosition, positionInChunk) = (GetChunkPosition(position), GetPositionInChunk(position));
            if (!Chunks.ContainsKey(chunkPosition)) return null;
            var color = Chunks[chunkPosition].GetColor(positionInChunk);
            if (color == Color.Transparent) return null;
            return color;
        }

        public override void Update() {

        }

        public override void Draw(Camera2D camera, bool drawOutlines = false) {
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

                    camera.Draw(Chunks[chunkIndex].Texture, (Rectangle)new RectangleF(xLeft, yTop, xRight - xLeft, yBottom - yTop), (Rectangle)new RectangleF(xLeft - chunkPos.X, yTop - chunkPos.Y , xRight - xLeft, yBottom - yTop), Color.White);
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

        public void PaintPixel(Vector2I positionInChunk, Color color, float opacity, CommandChain chain) {
            Texture.PaintPixel(positionInChunk, color, opacity, chain);
        }

        public Color GetColor(Vector2I positionInChunk) {
            return Texture.GetColor(positionInChunk);
        }
    }

    public partial class TextureLayer2D {
        public static byte[] SerializeTextureLayer2D(TextureLayer2D layer) {
            using (var memoryStream = new MemoryStream())
            using (var gzipStream = new GZipStream(memoryStream, CompressionMode.Compress))
            using (var writer = new BinaryWriter(gzipStream)) {
                // Write basic layer data
                writer.Write(layer.ChunkLength);
                writer.Write(layer.Chunks.Count);

                // Serialize each chunk in the dictionary
                foreach (var chunkEntry in layer.Chunks) {
                    var chunkPosition = chunkEntry.Key;
                    var chunk = chunkEntry.Value;

                    writer.Write(chunkPosition.X);
                    writer.Write(chunkPosition.Y);
                    
                    // Write texture data as byte array
                    var chunkData = chunk.Texture.TextureData;
                    var bytes = new byte[chunkData.Length * 4]; // 4 bytes per Color (RGBA)

                    for (int i = 0; i < chunkData.Length; i++) {
                        bytes[i * 4 + 0] = chunkData[i].R;
                        bytes[i * 4 + 1] = chunkData[i].G;
                        bytes[i * 4 + 2] = chunkData[i].B;
                        bytes[i * 4 + 3] = chunkData[i].A;
                    }
                    
                    writer.Write(bytes.Length);
                    writer.Write(bytes);
                }

                writer.Flush();
                return memoryStream.ToArray();
            }
        }

        public static TextureLayer2D DeserializeTextureLayer2D(byte[] data, GraphicsDevice graphicsDevice) {
            using (var memoryStream = new MemoryStream(data))
            using (var gzipStream = new GZipStream(memoryStream, CompressionMode.Decompress))
            using (var reader = new BinaryReader(gzipStream)) {
                // Read basic layer data
                int chunkLength = reader.ReadInt32();
                int chunkCount = reader.ReadInt32();

                // Initialize the texture layer
                var layer = new TextureLayer2D { ChunkLength = chunkLength };

                // Read each chunk
                for (int i = 0; i < chunkCount; i++) {
                    int posX = reader.ReadInt32();
                    int posY = reader.ReadInt32();
                    int dataSize = reader.ReadInt32();

                    var bytes = reader.ReadBytes(dataSize);
                    var colors = new Color[chunkLength * chunkLength];

                    for (int j = 0; j < colors.Length; j++) {
                        byte r = bytes[j * 4 + 0];
                        byte g = bytes[j * 4 + 1];
                        byte b = bytes[j * 4 + 2];
                        byte a = bytes[j * 4 + 3];
                        colors[j] = new Color(r, g, b, a);
                    }

                    var chunk = new TextureChunk2D(layer, chunkLength) {
                        Texture = new SQTexture2D(graphicsDevice, chunkLength, chunkLength) {
                            TextureData = colors
                        }
                    };
                    chunk.Texture.SetData(colors); // Apply data to Texture2D
                    layer.Chunks.Add(new Vector2I(posX, posY), chunk);
                }

                return layer;
            }
        }
    }
}
namespace Somniloquy {
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;

    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;

    public partial class TextureLayer2D : Layer2D, IPaintableLayer2D {
        public int ChunkLength = 2560;
        public Dictionary<Vector2I, TextureChunk2D> Chunks = new();

        public Vector2I GetChunkPosition(Vector2I canvasPosition) => Util.Floor(canvasPosition / ChunkLength);
        public Vector2I GetPositionInChunk(Vector2I canvasPosition) => Util.PosMod(canvasPosition, new Vector2I(ChunkLength, ChunkLength));

        public void PaintPixel(Vector2I position, Color color, float opacity, CommandChain chain = null, bool preview = false) {
            if (preview) {
                SQ.SB.Draw(SQ.SB.Pixel, position, color);
            } else {
                var (chunkPosition, positionInChunk) = (GetChunkPosition(position), GetPositionInChunk(position));
                if (!Chunks.ContainsKey(chunkPosition)) {
                    Chunks.Add(chunkPosition, new TextureChunk2D(this, ChunkLength));
                }
                Chunks[chunkPosition].PaintPixel(positionInChunk, color, opacity, chain);
            }
        }

        public override void Update() {

        }

        public override void Draw(Camera2D camera) {
            Vector2 topLeft = camera.ViewportInWorld.TopLeft();
            Vector2 bottomRight = camera.ViewportInWorld.BottomRight();
            Vector2I topLeftChunk = new((int)(topLeft.X / ChunkLength) - 1, (int)(topLeft.Y / ChunkLength) - 1);
            Vector2I bottomRightChunk = new((int)(bottomRight.X / ChunkLength) + 1, (int)(bottomRight.Y / ChunkLength) + 1);

            for (int y = topLeftChunk.Y; y < bottomRightChunk.Y; y++) {
                for (int x = topLeftChunk.X; x < bottomRightChunk.X; x++) {
                    var chunkPosition = new Vector2I(x, y);
                    if (!Chunks.ContainsKey(chunkPosition)) continue;
                    var xLeft = MathF.Min(MathF.Max(topLeft.X, x * ChunkLength), bottomRight.X);
                    var xRight = MathF.Max(MathF.Min(bottomRight.X, (x + 1) * ChunkLength), topLeft.X);
                    var yTop = MathF.Min(MathF.Max(topLeft.Y, y * ChunkLength), bottomRight.Y);
                    var yBottom = MathF.Max(MathF.Min(bottomRight.Y, (y + 1) * ChunkLength), topLeft.Y); 

                    Camera.Draw(Chunks[chunkPosition].Texture, (Rectangle)new RectangleF(xLeft, yTop, xRight - xLeft, yBottom - yTop), Color.White);
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

        public void PaintPixel(Vector2I positionInChunk, Color color, float opacity, CommandChain chain = null) {
            Texture.PaintPixel(positionInChunk, color, opacity, chain);
        }
    }
}
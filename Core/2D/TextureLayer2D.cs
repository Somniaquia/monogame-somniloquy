namespace Somniloquy {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;

    public partial class TextureLayer2D : PaintableLayer2D {
        [JsonInclude] public int ChunkLength = 256;
        [JsonInclude] public Dictionary<Vector2I, TextureChunk2D> Chunks = new();

        public TextureLayer2D() : base() { }
        public TextureLayer2D(string identifier) : base(identifier) { }

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

        public override void PaintPixel(Vector2I position, Color color, float opacity, CommandChain chain = null) {
            if (GetColor(position) is null && color == Color.Transparent || GetColor(position) == color) return;
            
            var (chunkPosition, positionInChunk) = (GetChunkPosition(position), GetPositionInChunk(position));
            
            if (!Chunks.ContainsKey(chunkPosition)) {
                var chunk = new TextureChunk2D(this, ChunkLength);
                Chunks.Add(chunkPosition, chunk);
                chain?.AddCommand(new TextureChunkSetCommand(this, chunkPosition, null, chunk));
            }

            if (chain.AffectedPixels.ContainsKey(position) && opacity != 1) {
                if (chain.AffectedPixels[position].Item2 >= opacity) return;
                Chunks[chunkPosition].SetPixel(positionInChunk, chain.AffectedPixels[position].Item1.BlendWith(color, opacity), chain);
                chain.AffectedPixels[position] = (chain.AffectedPixels[position].Item1, opacity);
            } else {
                chain.AffectedPixels[position] = (Chunks[chunkPosition].GetColor(positionInChunk), opacity);
                Chunks[chunkPosition].PaintPixel(positionInChunk, color, opacity, chain);
            }
        }
        
        public override Color? GetColor(Vector2I position) {
            var (chunkPosition, positionInChunk) = (GetChunkPosition(position), GetPositionInChunk(position));
            if (!Chunks.ContainsKey(chunkPosition)) return null;
            var color = Chunks[chunkPosition].GetColor(positionInChunk);
            if (color == Color.Transparent) return null;
            return color;
        }

        public override Rectangle GetSelfBounds() {
            RemoveEmptyChunks();
            
            int chunkXMin = Chunks.Min(chunk => chunk.Key.X);
            int chunkXMax = Chunks.Max(chunk => chunk.Key.X);
            int chunkYMin = Chunks.Min(chunk => chunk.Key.Y);
            int chunkYMax = Chunks.Max(chunk => chunk.Key.Y); 
            
            var xMin = chunkXMin * ChunkLength;
            var yMin = chunkYMin * ChunkLength;
            var xMax = (chunkXMax + 1) * ChunkLength;
            var yMax = (chunkYMax + 1) * ChunkLength;

            return new Rectangle(xMin, yMin, xMax - xMin, yMax - yMin);
        }

        public void RemoveEmptyChunks() {
            // TODO: ensure that empty chunks are removed
        }

        public override void Update() { }

        // For image saving
        public void Draw(Vector2I topLeft, Vector2I bottomRight, float opacity = 1f) {
            Vector2I topLeftChunk = new(Util.Round((float)topLeft.X / ChunkLength) - 1, Util.Round((float)topLeft.Y / ChunkLength) - 1);
            Vector2I bottomRightChunk = new(Util.Round((float)bottomRight.X / ChunkLength) + 1, Util.Round((float)bottomRight.Y / ChunkLength) + 1);

            SQ.SB.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Transform);

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

        public override void Draw(Camera2D camera, bool collisionBounds = false) {
            if (Opacity == 0f) { base.Draw(camera); return; }

            Matrix layerView = Transform * camera.Transform;
            var visibleBounds = GetVisisbleBounds(camera);
            Vector2 topLeft = visibleBounds.TopLeft() - Vector2.One;
            Vector2 bottomRight = visibleBounds.BottomRight() + Vector2.One;

            Vector2I topLeftChunk = new Vector2I(Util.Round(topLeft.X / ChunkLength) - 2, Util.Round(topLeft.Y / ChunkLength) - 2); // For some reason subtracting only (1, 1) instead of (2, 2) cuts off leftmost and topmost chunks from the screen
            Vector2I bottomRightChunk = new Vector2I(Util.Round(bottomRight.X / ChunkLength) + 1, Util.Round(bottomRight.Y / ChunkLength) + 1);

            camera.SB.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, layerView);

            for (int y = topLeftChunk.Y; y <= bottomRightChunk.Y; y++) {
                for (int x = topLeftChunk.X; x <= bottomRightChunk.X; x++) {
                    var chunkIndex = new Vector2I(x, y);
                    if (!Chunks.TryGetValue(chunkIndex, out var chunk)) continue;

                    RectangleF chunkBounds = new(x * ChunkLength, y * ChunkLength, ChunkLength, ChunkLength);
                    RectangleF visible = RectangleF.Intersect(new RectangleF(topLeft, bottomRight - topLeft), chunkBounds);

                    if (visible.Width <= 0 || visible.Height <= 0) continue;
                    Rectangle sourceRect = new(Util.Round(visible.X - chunkBounds.X), Util.Round(visible.Y - chunkBounds.Y), Util.Round(visible.Width), Util.Round(visible.Height));

                    camera.SB.Draw(chunk.Texture, new Rectangle(Util.Round(visible.X), Util.Round(visible.Y), Util.Round(visible.Width), Util.Round(visible.Height)), sourceRect, Color.White * Opacity);
                }
            }

            if (collisionBounds) DrawCollisionBounds(camera);
            base.Draw(camera, collisionBounds);
        }

        public void DrawCollisionBounds(Camera2D camera) {
            if (Opacity == 0f) return;
            camera.SB.End();
            List<VertexPositionColor> vertices = new();

            var visibleBounds = GetVisisbleBounds(camera);
            Vector2 topLeft = visibleBounds.TopLeft() - Vector2.One;
            Vector2 bottomRight = visibleBounds.BottomRight() + Vector2.One;

            Vector2I topLeftChunk = new Vector2I(Util.Round(topLeft.X / ChunkLength) - 2, Util.Round(topLeft.Y / ChunkLength) - 2); // For some reason subtracting only (1, 1) instead of (2, 2) cuts off leftmost and topmost chunks from the screen
            Vector2I bottomRightChunk = new Vector2I(Util.Round(bottomRight.X / ChunkLength) + 1, Util.Round(bottomRight.Y / ChunkLength) + 1);

            for (int y = topLeftChunk.Y; y <= bottomRightChunk.Y; y++) {
                for (int x = topLeftChunk.X; x <= bottomRightChunk.X; x++) {
                    var chunkIndex = new Vector2I(x, y);
                    if (!Chunks.TryGetValue(chunkIndex, out var chunk)) continue;
                    
                    var chunkVertices = chunk.CollisionEdges;
                    if (chunkVertices is null || chunkVertices.Count == 0) continue;

                    var offsetX = ChunkLength * x;
                    var offsetY = ChunkLength * y;
                    for (int i = 0; i < chunkVertices.Count; i++) {
                        vertices.Add(new VertexPositionColor(new Vector3(camera.ToScreenPos(ToWorldPos(new Vector2(offsetX + chunkVertices[i].Item1.X, offsetY + chunkVertices[i].Item1.Y))), 0), Color.Tomato * Opacity));
                        vertices.Add(new VertexPositionColor(new Vector3(camera.ToScreenPos(ToWorldPos(new Vector2(offsetX + chunkVertices[i].Item2.X, offsetY + chunkVertices[i].Item2.Y))), 0), Color.Tomato * Opacity));
                    }
                }
            }

            if (vertices.Count == 0) return;
            var verticesArray = vertices.ToArray();

            VertexBuffer vertexBuffer = new(SQ.GD, typeof(VertexPositionColor), verticesArray.Length, BufferUsage.WriteOnly);
            vertexBuffer.SetData(verticesArray);
            SQ.GD.SetVertexBuffer(vertexBuffer);

            foreach (var pass in SQ.BasicEffect.CurrentTechnique.Passes) {
                pass.Apply();
                SQ.GD.DrawPrimitives(PrimitiveType.LineList, 0, verticesArray.Length / 2);
            }
        }
    }
    
    public class TextureChunk2D {
        [JsonIgnore] public TextureLayer2D ParentLayer;
        [JsonInclude] public SQTexture2D Texture;
        [JsonInclude] public int ChunkLength;
        [JsonInclude] public List<(Vector2, Vector2)> CollisionEdges;
        
        public TextureChunk2D() { }

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
}
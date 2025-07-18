﻿namespace Somniloquy {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;

    /// <summary>
    /// A Layer is the building block of a world.
    /// Layers show and hide based on various events, such as player's approachment or interaction with certain objects, 
    /// random variables or internal states.
    /// The dynamic modification of layers in a same world will help form the overall expected nature of the game, triggering
    /// different layouts or looks for the same world in every visit, sometimes connecting a different world or triggering events such as jumpscares
    /// </summary>
    public class TileLayer2D : PaintableLayer2D {
        [JsonInclude] public int ChunkLength = 16;
        [JsonInclude] public int TileLength = 16;
        [JsonInclude] public Dictionary<Vector2I, TileChunk2D> Chunks = new();

        public Vector2I GetChunkPosition(Vector2I tilePosition) => tilePosition / ChunkLength;
        public Vector2I GetPositionInChunk(Vector2I worldPosition) => Util.PosMod(worldPosition, new Vector2I(TileLength * ChunkLength));
        public Vector2I GetTilePositionInChunk(Vector2I tilePosition) => Util.PosMod(tilePosition, new Vector2I(ChunkLength));
        public Vector2I GetTilePosition(Vector2I worldPosition) => worldPosition / TileLength;
        public Vector2I GetPositionInTile(Vector2I worldPosition) => Util.PosMod(worldPosition, new Vector2I(TileLength));

        public TileLayer2D() : base() { }
        public TileLayer2D(string identifier) : base(identifier) { }
        public TileLayer2D(int chunkLength = 16, int tileLength = 16) {
            ChunkLength = chunkLength;
            TileLength = tileLength;
        }

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
            
            var (chunkPosition, positionInChunk) = (GetChunkPosition(GetTilePosition(position)), GetPositionInChunk(position));
            
            if (!Chunks.ContainsKey(chunkPosition)) {
                var chunk = new TileChunk2D(this);
                Chunks.Add(chunkPosition, chunk);
                chain?.AddCommand(new TileChunkSetCommand(this, chunkPosition, null, chunk));
            }

            if (chain.AffectedPixels.ContainsKey(position) && opacity != 1) {
                if (chain.AffectedPixels[position].Item2 >= opacity) return;
                Chunks[chunkPosition].SetPixel(positionInChunk, chain.AffectedPixels[position].Item1.BlendWith(color, opacity), chain);
                chain.AffectedPixels[position] = (chain.AffectedPixels[position].Item1, opacity);
            } else {
                var originalColor = Chunks[chunkPosition].GetColor(positionInChunk);
                originalColor ??= Color.Transparent;
                chain.AffectedPixels[position] = (originalColor.Value, opacity);
                Chunks[chunkPosition].PaintPixel(positionInChunk, color, opacity, chain);
            }
        }

        public override Color? GetColor(Vector2I position) {
            var (chunkPosition, positionInChunk) = (GetChunkPosition(GetTilePosition(position)), GetPositionInChunk(position));
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
            
            var xMin = chunkXMin * ChunkLength * TileLength;
            var yMin = chunkYMin * ChunkLength * TileLength;
            var xMax = (chunkXMax + 1) * ChunkLength * TileLength;
            var yMax = (chunkYMax + 1) * ChunkLength * TileLength;

            return new Rectangle(xMin, yMin, xMax - xMin, yMax - yMin);
        }

        public void RemoveEmptyChunks() {
            
        }

        #region Tile Methods
        public void SetRectangle(Vector2I startPosition, Vector2I endPosition, Tile2D tile, bool filled, CommandChain chain = null) {
            PixelActions.ApplyRectangleAction(startPosition, endPosition, filled, (Vector2I position) => {
                SetTile(position, tile, chain);
            });
        }

        public void SetLine(Vector2I start, Vector2I end, Tile2D tile, int width = 0, CommandChain chain = null) {
            PixelActions.ApplyLineAction(start, end, width, (Vector2I position) => {
                SetTile(position, tile, chain);
            });
        }

        public void SetCircle(Vector2I center, int radius, Tile2D tile, bool filled = true, CommandChain chain = null) {
            PixelActions.ApplyCircleAction(center, radius, filled, (Vector2I position) => {
                SetTile(position, tile, chain);
            });
        }

        public void SetTile(Vector2I tilePosition, Tile2D tile, CommandChain chain = null) {
            if (GetTile(tilePosition) == tile) return;

            var chunkPosition = GetChunkPosition(tilePosition);
            var tilePosInChunk = GetTilePositionInChunk(tilePosition);
            if (!Chunks.ContainsKey(chunkPosition)) AddChunk(chunkPosition);

            Chunks[chunkPosition].SetTile(tilePosInChunk, tile, chain);
        }

        private void AddChunk(Vector2I chunkPosition, CommandChain chain = null) {
            var chunk = new TileChunk2D(this);
            // Array.Fill(chunk, DefaultTile); TODO: Default tile
            Chunks.Add(chunkPosition, chunk);
            chain?.AddCommand(new TileChunkSetCommand(this, chunkPosition, null, chunk));
        }

        /// <summary>
        /// Capable of returning null when tile isn't present at the position!
        /// </summary>
        public Tile2D GetTile(Vector2I tilePosition) {
            var chunkPosition = GetChunkPosition(tilePosition);
            var tilePosInChunk = GetTilePositionInChunk(tilePosition);
            if (!Chunks.ContainsKey(chunkPosition)) return null;
            
            return Chunks[chunkPosition].GetTile(tilePosInChunk);
        }

        public Tile2D[,] GetTiles(Vector2I tilePosition1, Vector2I tilePosition2) {
            var (Vector2I1, Vector2I2) = Vector2Extensions.Rationalize(tilePosition1, tilePosition2);
            int columns = Vector2I2.X - Vector2I1.X + 1;
            int rows = Vector2I2.Y - Vector2I1.Y + 1;

            var tiles = new Tile2D[columns, rows];

            for (int y = 0; y < rows; y++) {
                for (int x = 0; x < columns; x++) {
                    tiles[x, y] = GetTile(Vector2I1 + new Vector2I(x, y));
                }
            }

            return tiles;
        }

        public void FillTile(Vector2I startPos, Tile2D[,] tiles) {
            Tile2D startTile = GetTile(startPos);

            var stack = new Stack<Vector2I>();
            var lookedPositions = new HashSet<Vector2I>();
            var chain = new CommandChain();

            stack.Push(startPos);
            lookedPositions.Add(startPos);

            while (stack.Count > 0 && lookedPositions.Count < 1000000) {
                var pos = stack.Pop();
                var displacement = pos - startPos;
                SetTile(pos, tiles[Util.PosMod(displacement.X, tiles.GetLength(0)), Util.PosMod(displacement.Y, tiles.GetLength(1))], chain);

                foreach (var checkPos in new Vector2I[] { new(1, 0), new(-1, 0), new(0, 1), new(0, -1) }) {
                    var newPos = pos + checkPos;
                    if (!lookedPositions.Contains(newPos) && GetTile(newPos) == startTile) {
                        lookedPositions.Add(newPos);
                        stack.Push(newPos);
                    }
                }
            }

            if (lookedPositions.Count >= 1000000) {
                chain.Undo();
                DebugInfo.AddTempLine(() => "Overload: Cancelled fill operation.", 5);
            } else {
                CommandManager.AddCommandChain(chain);
            }
        }

        #endregion

        public override void Update() {
            // foreach (KeyValuePair<Vector2I, Tile[,]> Vector2ITilePair in Chunks) {
            //     for (int y = 0; y < ChunkLength; y++) {
            //         for (int x = 0; x < ChunkLength; x++) {
            //             Vector2ITilePair.Value[x, y]?.Update();
            //         }
            //     }
            // }
        }

        public override void Draw(Camera2D camera, bool collisionBounds = false) {
            if (Opacity == 0f) { base.Draw(camera); return; }

            var chunkLengthInPixels = ChunkLength * TileLength;

            Matrix layerView = Transform * camera.Transform;
            var visibleBounds = GetVisisbleBounds(camera);
            Vector2 topLeft = visibleBounds.TopLeft() - Vector2.One;
            Vector2 bottomRight = visibleBounds.BottomRight() + Vector2.One;

            Vector2I topLeftChunk = new(Util.Round(topLeft.X / chunkLengthInPixels) - 2, Util.Round(topLeft.Y / chunkLengthInPixels) - 2);
            Vector2I bottomRightChunk = new(Util.Round(bottomRight.X / chunkLengthInPixels) + 1, Util.Round(bottomRight.Y / chunkLengthInPixels) + 1);

            camera.SB.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, layerView);

            for (int y = topLeftChunk.Y; y < bottomRightChunk.Y; y++) {
                for (int x = topLeftChunk.X; x < bottomRightChunk.X; x++) {
                    var chunkIndex = new Vector2I(x, y);
                    if (!Chunks.TryGetValue(chunkIndex, out var chunk)) continue;
                    
                    var chunkPos = chunkIndex * chunkLengthInPixels;

                    RectangleF chunkBounds = new(x * chunkLengthInPixels, y * chunkLengthInPixels, chunkLengthInPixels, chunkLengthInPixels);
                    RectangleF visible = RectangleF.Intersect(new RectangleF(topLeft, bottomRight - topLeft), chunkBounds);

                    if (visible.Width <= 0 || visible.Height <= 0) continue;
                    Rectangle sourceRect = new(Util.Round(visible.X - chunkBounds.X), Util.Round(visible.Y - chunkBounds.Y), Util.Round(visible.Width), Util.Round(visible.Height));

                    var destination = new Rectangle(chunkPos, Vector2I.One * chunkLengthInPixels);
                    for (int tileY = 0; tileY < ChunkLength; tileY++) {
                        for (int tileX = 0; tileX < ChunkLength; tileX++) {
                            chunk.Tiles[tileX, tileY]?.Draw(camera, new Rectangle(new Vector2I(destination.X + tileX * TileLength, destination.Y + tileY * TileLength), new Vector2I(TileLength)), Opacity);
                        }
                    }
                }
            
            }

            if (collisionBounds) DrawCollisionBounds(camera);
            base.Draw(camera, collisionBounds);
        }

        public void DrawCollisionBounds(Camera2D camera) {
            if (Opacity == 0f) return;
            camera.SB.End();
            List<VertexPositionColor> vertices = new();

            var chunkLengthInPixels = ChunkLength * TileLength;
            var visibleBounds = GetVisisbleBounds(camera);
            Vector2 topLeft = visibleBounds.TopLeft() - Vector2.One;
            Vector2 bottomRight = visibleBounds.BottomRight() + Vector2.One;

            Vector2I topLeftChunk = new(Util.Round(topLeft.X / chunkLengthInPixels) - 2, Util.Round(topLeft.Y / chunkLengthInPixels) - 2);
            Vector2I bottomRightChunk = new(Util.Round(bottomRight.X / chunkLengthInPixels) + 1, Util.Round(bottomRight.Y / chunkLengthInPixels) + 1);
        
            for (int y = topLeftChunk.Y; y < bottomRightChunk.Y; y++) {
                for (int x = topLeftChunk.X; x < bottomRightChunk.X; x++) {
                    var chunkIndex = new Vector2I(x, y);
                    if (!Chunks.TryGetValue(chunkIndex, out var chunk)) continue;
                    
                    RectangleF chunkBounds = new(x * chunkLengthInPixels, y * chunkLengthInPixels, chunkLengthInPixels, chunkLengthInPixels);
                    RectangleF visible = RectangleF.Intersect(new RectangleF(topLeft, bottomRight - topLeft), chunkBounds);

                    if (visible.Width <= 0 || visible.Height <= 0) continue;
                    Rectangle sourceRect = new(Util.Round(visible.X - chunkBounds.X), Util.Round(visible.Y - chunkBounds.Y), Util.Round(visible.Width), Util.Round(visible.Height));

                    for (int tileY = 0; tileY < ChunkLength; tileY++) {
                        for (int tileX = 0; tileX < ChunkLength; tileX++) {
                            if (chunk.Tiles[tileX, tileY] is null) continue;
                            var tileVertices = chunk.Tiles[tileX, tileY].CollisionEdges;
                            if (tileVertices is null || tileVertices.Count == 0) continue;

                            var offsetX = chunkLengthInPixels * x + TileLength * tileX;
                            var offsetY = chunkLengthInPixels * y + TileLength * tileY;
                            for (int i = 0; i < tileVertices.Count; i++) {
                                vertices.Add(new VertexPositionColor(new Vector3(camera.ToScreenPos(ToWorldPos(new Vector2(offsetX + tileVertices[i].Item1.X, offsetY + tileVertices[i].Item1.Y))), 0), Color.Tomato * Opacity));
                                vertices.Add(new VertexPositionColor(new Vector3(camera.ToScreenPos(ToWorldPos(new Vector2(offsetX + tileVertices[i].Item2.X, offsetY + tileVertices[i].Item2.Y))), 0), Color.Tomato * Opacity));
                            }
                        }
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

    public class TileChunk2D {
        [JsonInclude] public Tile2D[,] Tiles;
        [JsonInclude] public TileLayer2D Layer;

        public Vector2I GetTilePositionInChunk(Vector2I positionInChunk) => positionInChunk / new Vector2I(Layer.TileLength);
        public Vector2I GetPositionInTile(Vector2I positionInChunk) => Util.PosMod(positionInChunk, new Vector2I(Layer.TileLength));
        
        public TileChunk2D() { }
        public TileChunk2D (TileLayer2D layer) {
            Layer = layer;

            Tiles = new Tile2D[layer.ChunkLength, layer.ChunkLength];
        }

        public void SetTile(Vector2I positionInChunk, Tile2D tile, CommandChain chain = null) {
            chain?.AddCommand(new TileSetCommand(this, positionInChunk, GetTile(positionInChunk), tile));
            Tiles[positionInChunk.X, positionInChunk.Y] = tile;
        }
        
        public Tile2D NewTile(Vector2I positionInChunk, CommandChain chain = null) {
            var tile = new Tile2D(Layer.Section);
            SetTile(positionInChunk, tile, chain);
            return tile;
        }

        public void SetPixel(Vector2I positionInChunk, Color color, CommandChain chain) {
            var (tilePosInChunk, posInTile) = (GetTilePositionInChunk(positionInChunk), GetPositionInTile(positionInChunk));
            var tile = GetTile(tilePosInChunk);
            if (tile is null) {
                tile = NewTile(GetTilePositionInChunk(positionInChunk), chain);
            }
            tile.SetPixel(posInTile, color, chain);
        }

        public void PaintPixel(Vector2I positionInChunk, Color color, float opacity, CommandChain chain) {
            var (tilePosInChunk, posInTile) = (GetTilePositionInChunk(positionInChunk), GetPositionInTile(positionInChunk));
            var tile = GetTile(tilePosInChunk);
            if (tile is null) {
                tile = NewTile(GetTilePositionInChunk(positionInChunk), chain);
            }
            tile.PaintPixel(posInTile, color, opacity, chain);
        }

        /// <summary>
        /// Capable of returning null when tile isn't present at the position!
        /// </summary>
        public Tile2D GetTile(Vector2I positionInChunk) {
            return Tiles[positionInChunk.X, positionInChunk.Y];
        }

        public Color? GetColor(Vector2I positionInChunk) {
            var (tilePosInChunk, posInTile) = (GetTilePositionInChunk(positionInChunk), GetPositionInTile(positionInChunk));
            var tile = GetTile(tilePosInChunk);
            if (tile is null) {
                return null;
            }
            return tile.GetColor(posInTile);
        }
    }

    public class Tile2DArrayConverter : JsonConverter<Tile2D[,]> {
        public override Tile2D[,] Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
            if (reader.TokenType != JsonTokenType.StartArray) {
                throw new JsonException("Expected StartArray token.");
            }

            reader.Read(); int rows = reader.GetInt32();
            reader.Read(); int cols = reader.GetInt32();
            reader.Read(); 

            var tiles = new Tile2D[rows, cols];

            for (int i = 0; i < rows; i++) {
                if (reader.TokenType != JsonTokenType.StartArray) {
                    throw new JsonException("Expected StartArray token for row.");
                }
                reader.Read();

                for (int j = 0; j < cols; j++) {
                    tiles[i, j] = JsonSerializer.Deserialize<Tile2D>(ref reader, options);
                    reader.Read(); // Move to the next element or end of row
                }

                if (reader.TokenType != JsonTokenType.EndArray) {
                    throw new JsonException("Expected EndArray token for row.");
                }
                reader.Read(); // Move to the next row or end of 2D array
            }

            if (reader.TokenType != JsonTokenType.EndArray) {
                throw new JsonException("Expected EndArray token for 2D array.");
            }

            return tiles;
        }

        public override void Write(Utf8JsonWriter writer, Tile2D[,] value, JsonSerializerOptions options) {
            int rows = value.GetLength(0);
            int cols = value.GetLength(1);

            writer.WriteStartArray();
            writer.WriteNumberValue(rows);
            writer.WriteNumberValue(cols);

            for (int i = 0; i < rows; i++) {
                writer.WriteStartArray();
                for (int j = 0; j < cols; j++) {
                    JsonSerializer.Serialize(writer, value[i, j], options);
                }
                writer.WriteEndArray();
            }

            writer.WriteEndArray();
        }
    }
}

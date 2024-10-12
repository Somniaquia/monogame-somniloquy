namespace Somniloquy {
    using System;
    using System.Collections.Generic;

    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;

    /// <summary>
    /// A Layer is the building block of a world.
    /// Layers show and hide based on various events, such as player's approachment or interaction with certain objects, 
    /// random variables or internal states.
    /// The dynamic modification of layers in a same world will help form the overall expected nature of the game, triggering
    /// different layouts or looks for the same world in every visit, sometimes connecting a different world or triggering events such as jumpscares
    /// </summary>
    public class TileLayer2D : Layer2D, IPaintableLayer2D {
        public static int ChunkLength { get; } = 16;
        public static int TileLength { get; } = 16;
        public Dictionary<Vector2I, Tile2D[,]> Chunks { get; set; } = new();

        public Vector2I GetTilePosition(Vector2I worldPosition) => Util.Floor(worldPosition / TileLength);
        public Vector2I GetPositionInTile(Vector2I worldPosition) => Util.PosMod(worldPosition, new Vector2I(TileLength));
        public Vector2I GetChunkPosition(Vector2I tilePosition) => Util.Floor(tilePosition / ChunkLength);
        public Vector2I GetTilePositionInChunk(Vector2I tilePosition) => Util.PosMod(tilePosition, new Vector2I(ChunkLength));

        public void PaintPixel(Vector2I position, Color color, float opacity, CommandChain chain = null, bool preview = false) {
            if (preview) {
                SQ.SB.Draw(SQ.SB.Pixel, position, color);
            } else {
                // if (sync) {
                //     var previousTile = GetTile(tilePosition);
                //     // SetTile(tilePosition, ParentWorld.NewTile(TileLength), command);
                //     SetTile(tilePosition, ParentWorld.NewTile());
                //     PaintPixel(GetTile(tilePosition), previousTile.Sprite.GetFrameColors(), command);
                // }
                
                var tilePosition = GetTilePosition(position);
                var tile = GetTile(tilePosition);

                // TODO: CHUNK CHECK!!!

                tile.PaintPixel(GetPositionInTile(position), color, opacity, chain);
            }
        }

        #region Tile Methods
        public void SetRectangle(Vector2I startPosition, Vector2I endPosition, Tile2D tile, bool filled, CommandChain chain = null, bool preview = false) {
            PixelActions.ApplyRectangleAction(startPosition, endPosition, filled, (Vector2I position) => {
                SetTile(position, tile, chain, preview);
            });
        }

        public void SetLine(Vector2I start, Vector2I end, Tile2D tile, int width = 0, CommandChain chain = null, bool preview = false) {
            PixelActions.ApplyLineAction(start, end, width, (Vector2I position) => {
                SetTile(position, tile, chain, preview);
            });
        }

        public void SetCircle(Vector2I center, int radius, Tile2D tile, bool filled = true, CommandChain chain = null, bool preview = false) {
            PixelActions.ApplyCircleAction(center, radius, filled, (Vector2I position) => {
                SetTile(position, tile, chain, preview);
            });
        }

        public void SetTile(Vector2I tilePosition, Tile2D tile, CommandChain chain = null, bool preview = false) {
            if (GetTile(tilePosition) == tile) return;

            if (preview) {
                tile?.Draw(new Rectangle(tilePosition.X * TileLength, tilePosition.Y * TileLength, TileLength, TileLength), 0.5f);
            } else {
                chain?.AddCommand(new TileSetCommand(this, tilePosition, GetTile(tilePosition), tile));

                var chunkPosition = GetChunkPosition(tilePosition);
                var tilePosInChunk = GetTilePositionInChunk(tilePosition);
                if (!Chunks.ContainsKey(chunkPosition)) AddChunk(chunkPosition);

                Chunks[chunkPosition][tilePosInChunk.X, tilePosInChunk.Y] = tile;

                // TODO: add tile to section tileset
            }
        }

        private void AddChunk(Vector2I chunkPosition, CommandChain chain = null) {
            var chunk = new Tile2D[ChunkLength, ChunkLength];
            // Array.Fill(chunk, DefaultTile); TODO: Default tile
            Chunks.Add(chunkPosition, chunk);
            chain?.AddCommand(new TileChunkSetCommand(this, chunkPosition, null, chunk));
        }

        // TODO: Tile patterns and Tile fill

        public Tile2D GetTile(Vector2I tilePosition) {
            var chunkPosition = GetChunkPosition(tilePosition);
            var posInChunk = GetTilePositionInChunk(tilePosition);
            return Chunks.ContainsKey(chunkPosition) ? Chunks[chunkPosition][posInChunk.X, posInChunk.Y] : null;
        }

        public Tile2D[,] GetTiles(Vector2I tilePosition1, Vector2I tilePosition2) {
            var (Vector2I1, Vector2I2) = Util.SortVector2Is(tilePosition1, tilePosition2);
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

        public void Draw(Camera2D camera, float opacity = 1f) {
            var viewportRect = camera.GetViewportInWorld();

            var startChunkPosition = GetChunkPosition(GetTilePosition((Vector2I)viewportRect.TopLeft()));
            var endChunkPosition = GetChunkPosition(GetTilePosition((Vector2I)viewportRect.BottomRight()));

            for (int chunkY = startChunkPosition.Y; chunkY < endChunkPosition.Y; chunkY++) {
                for (int chunkX = startChunkPosition.X; chunkX < endChunkPosition.X; chunkX++) {
                    if (!Chunks.ContainsKey(new Vector2I(chunkX, chunkY))) continue;

                    var chunk = Chunks[new Vector2I(chunkX, chunkY)];

                    for (int yInChunk = 0; yInChunk < ChunkLength; yInChunk++) {
                        for (int xInChunk = 0; xInChunk < ChunkLength; xInChunk++) {
                            chunk[xInChunk, yInChunk]?.Draw(
                                new Rectangle(
                                    (chunkX * ChunkLength + xInChunk) * TileLength,
                                    (chunkY * ChunkLength + yInChunk) * TileLength,
                                    TileLength, TileLength), opacity);
                        }
                    }
                }
            }
        }

        public void DrawTileBounds(Camera2D camera, float opacity = 1f) {
            var startPos = camera.ViewportInWorld.TopLeft();
            var endChunkPosition = camera.ViewportInWorld.BottomRight();

            
        }
    }
}

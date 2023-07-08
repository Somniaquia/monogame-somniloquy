namespace Somniloquy {
    using System;
    using System.Collections.Generic;

    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using MonoGame.Extended;
    using Newtonsoft.Json;

    /// <summary>
    /// A Layer is the building block of a world.
    /// Layers show and hide based on various events, such as player's approachment or interaction with certain objects, 
    /// random variables or internal states.
    /// The dynamic modification of layers in a same world will help form the overall expected nature of the game, triggering
    /// different layouts or looks for the same world in every visit, sometimes connecting a different world or triggering events such as jumpscares
    /// </summary>
    public class Layer {
        public World ParentWorld { get; set; }
        public Layer ChildLayers { get; set; }
        public Point Dimensions { get; set; }
        [JsonConverter(typeof(ChunksConverter))]
        public Dictionary<Point, Tile[,]> Chunks { get; set; } = new();
        public int ChunkLength { get; } = 16;
        public int TileLength { get; } = 8;

        public Layer(World parentWorld) {
            ParentWorld = parentWorld;
        }

        public Point GetPositionInTile(Point worldPosition) {
            return new Point(MathsHelper.Modulo(worldPosition.X, TileLength), MathsHelper.Modulo(worldPosition.Y, TileLength));
        }

        public Point GetTilePositionOf(Point worldPosition) {
            return new Point(MathsHelper.FloorDivide(worldPosition.X, TileLength), MathsHelper.FloorDivide(worldPosition.Y, TileLength));
        }

        public Point GetChunkPositionOf(Point tilePosition) {
            return new Point(MathsHelper.FloorDivide(tilePosition.X, ChunkLength), MathsHelper.FloorDivide(tilePosition.Y, ChunkLength));
        }

        #region Paint Methods
        public void PaintOnTile(Tile tile, Color?[,] colors, int animationFrame, PaintCommand command = null) {
            var oldTexture = tile.FSprite.GetCurrentAnimation().GetFrameColors(animationFrame);
            tile.FSprite.GetCurrentAnimation().PaintOnFrame(colors, animationFrame);
            command?.Append(tile, oldTexture, tile.FSprite.GetCurrentAnimation().GetFrameColors(animationFrame));
        }

        public void PaintLine(Point point1, Point point2, Color color, int width, PaintCommand command = null) {
            int dx = Math.Abs(point2.X - point1.X);
            int dy = Math.Abs(point2.Y - point1.Y);
            int sx = (point1.X < point2.X) ? 1 : -1;
            int sy = (point1.Y < point2.Y) ? 1 : -1;
            int err = dx - dy;

            while (true) {
                PaintCircle(new Point(point1.X, point1.Y), color, width, command);

                if (point1.X == point2.X && point1.Y == point2.Y)
                    break;

                int err2 = 2 * err;

                if (err2 > -dy) {
                    err -= dy;
                    point1.X += sx;
                }

                if (err2 < dx) {
                    err += dx;
                    point1.Y += sy;
                }
            }
        }

        public void PaintCircle(Point point, Color color, int width, PaintCommand command = null) {
            // TODO: Add brush shape other than a square
            int left = point.X - (width - 1) / 2;
            int right = point.X + width / 2;
            int top = point.Y - (width - 1) / 2;
            int bottom = point.Y + width / 2;

            PaintRectangle(new Rectangle(left, top, right - left, bottom - top), color, command);
        }

        public void PaintRectangle(Rectangle rectangle, Color color, PaintCommand command) {
            int startTileX = MathsHelper.FloorDivide(rectangle.X, TileLength);
            int startTileY = MathsHelper.FloorDivide(rectangle.Y, TileLength);
            int endTileX = MathsHelper.FloorDivide(rectangle.Right, TileLength);
            int endTileY = MathsHelper.FloorDivide(rectangle.Bottom, TileLength);

            for (int tileY = startTileY; tileY <= endTileY; tileY++) {
                for (int tileX = startTileX; tileX <= endTileX; tileX++) {
                    var colors = new Color?[TileLength, TileLength];

                    int startX = Math.Max(rectangle.X - tileX * TileLength, 0);
                    int startY = Math.Max(rectangle.Y - tileY * TileLength, 0);
                    int endX = Math.Min(rectangle.Right - tileX * TileLength, TileLength - 1);
                    int endY = Math.Min(rectangle.Bottom - tileY * TileLength, TileLength - 1);

                    for (int y = startY; y <= endY; y++) {
                        for (int x = startX; x <= endX; x++) {
                            colors[x, y] = color;
                        }
                    }

                    Point position = new(tileX, tileY);
                    if (GetTile(position) is null) {
                        SetTile(position, new Tile(TileLength));
                    }

                    PaintOnTile(GetTile(position), colors, EditorScreen.SelectedAnimationFrame, command);
                }
            }
        }

        public void PaintFill(Point positionInWorld, Color color, PaintCommand command = null) {
            var positionStack = new Stack<Point>();
            positionStack.Push(positionInWorld);

            Dictionary<Tile, Color?[,]> tileColors = new() {
                { GetTile(positionInWorld), GetTile(positionInWorld).FSprite.GetCurrentAnimation().GetFrameColors(EditorScreen.SelectedAnimationFrame) }
            };

            var targetColor = GetTile(GetTilePositionOf(positionInWorld)).GetColorAt(GetPositionInTile(positionInWorld));

            while (positionStack.Count != 0) {
                var position = positionStack.Pop();
                var tile = GetTile(GetTilePositionOf(position));

                foreach (var positionOffset in new Point[] { new Point(-1, 0), new Point(1, 0), new Point(0, -1), new Point(0, 1) }) {
                    var checkingPosition = position + positionOffset;
                    var checkingTile = GetTile(GetTilePositionOf(checkingPosition));
                    
                    if (!tileColors.ContainsKey(tile)) {
                        tileColors.Add(tile, tile.FSprite.GetCurrentAnimation().GetFrameColors(0));
                    }

                    if (tileColors[checkingTile][checkingPosition.X, checkingPosition.Y] == targetColor) {
                        positionStack.Push(checkingPosition);
                    }
                }

                if (positionStack.Count > 10000) {
                    command.Undo();
                    CommandManager.UndoHistory.Pop();
                    return;
                }
            }

            foreach (var pair in tileColors) {
                PaintOnTile(pair.Key, pair.Value, EditorScreen.SelectedAnimationFrame, command);
            }
        }

        #endregion

        #region Set Methods
        public void SetTile(Point position, Tile tile, SetCommand command = null, bool preview = false) {
            if (preview) {
                tile?.Draw(new Rectangle(position.X * TileLength, position.Y * TileLength, TileLength, TileLength), 0.5f);
            } else {
                command?.Append(position, GetTile(position), tile);

                Point chunkPosition = GetChunkPositionOf(position);

                if (!Chunks.ContainsKey(chunkPosition)) {
                    Chunks.Add(chunkPosition, new Tile[ChunkLength, ChunkLength]);
                }

                Chunks[chunkPosition][position.X - chunkPosition.X * ChunkLength, position.Y - chunkPosition.Y * ChunkLength] = tile;
                
                foreach (var existingTile in ParentWorld.Tiles) {
                    if (object.ReferenceEquals(existingTile, tile)) return;
                }

                ParentWorld.Tiles.Add(tile);
            }
        }

        public void SetLine(Point tilePos1, Point tilePos2, Tile[,] tilePattern, Point tilePatternOffset, int width, SetCommand command = null, bool preview = false) {
            Point originalTilePos = tilePos1;

            int dx = Math.Abs(tilePos2.X - tilePos1.X);
            int dy = Math.Abs(tilePos2.Y - tilePos1.Y);
            int sx = (tilePos1.X < tilePos2.X) ? 1 : -1;
            int sy = (tilePos1.Y < tilePos2.Y) ? 1 : -1;
            int err = dx - dy;

            while (true) {
                SetCircle(new Point(tilePos1.X, tilePos1.Y), tilePattern, width, tilePatternOffset + tilePos1 - originalTilePos, command, preview);

                if (tilePos1.X == tilePos2.X && tilePos1.Y == tilePos2.Y)
                    break;

                int err2 = 2 * err;

                if (err2 > -dy) {
                    err -= dy;
                    tilePos1.X += sx;
                }

                if (err2 < dx) {
                    err += dx;
                    tilePos1.Y += sy;
                }
            }
        }

        public void SetRectangle(Point tilePos1, Point tilePos2, Tile[,] tilePattern, Point tilePatternOffset, SetCommand command = null, bool preview = false) {
            var pair = MathsHelper.ValidizePoints(tilePos1, tilePos2);
            tilePos1 = pair.Item1;
            tilePos2 = pair.Item2;

            for (int y = tilePos1.Y; y <= tilePos2.Y; y++) {
                for (int x = tilePos1.X; x <= tilePos2.X; x++) {
                    SetTile(
                        new Point(x, y), 
                        tilePattern[
                            MathsHelper.Modulo(tilePatternOffset.X + x - tilePos1.X, tilePattern.GetLength(0)),
                            MathsHelper.Modulo(tilePatternOffset.Y + y - tilePos1.Y, tilePattern.GetLength(1))
                        ], command, preview
                    );
                }
            }
        }

        public void SetCircle(Point centerPosition, Tile[,] tilePattern, int width, Point tilePatternOffset, SetCommand command = null, bool preview = false) {
            for (int y = -(width - 1) / 2; y <= width / 2; y++) {
                for (int x = -(width - 1) / 2; x <= width / 2; x++) {
                    Point position = new(centerPosition.X + x, centerPosition.Y + y);
                    if (Vector2.Distance(position.ToVector2(), centerPosition.ToVector2()) <= width) {
                        SetTile(
                            new Point(centerPosition.X + x, centerPosition.Y + y),
                            tilePattern[
                                MathsHelper.Modulo(tilePatternOffset.X + x, tilePattern.GetLength(0)),
                                MathsHelper.Modulo(tilePatternOffset.Y + y, tilePattern.GetLength(1))
                            ], command, preview
                        );
                    }
                }
            }   
        }

        public void SetFill(Point tilePosition, Tile[,] tilePattern, SetCommand command = null) {
            var positionStack = new Stack<Point>();
            positionStack.Push(tilePosition);

            while (positionStack.Count != 0) {
                var position = positionStack.Pop();
                SetTile(position, tilePattern[
                        MathsHelper.Modulo(position.X - tilePosition.X, tilePattern.GetLength(0)),
                        MathsHelper.Modulo(position.Y - tilePosition.Y, tilePattern.GetLength(1))
                    ], command
                );

                foreach (var positionOffset in new Point[] { new Point(-1, 0), new Point(1, 0) , new Point(0, -1) , new Point(0, 1) }) {
                    if (GetTile(position + positionOffset) is null) {
                        positionStack.Push(position + positionOffset);
                    }
                }

                if (positionStack.Count > 10000) {
                    command.Undo();
                    CommandManager.UndoHistory.Pop();
                    break;
                }
            }
        }

        public Tile GetTile(Point tilePosition) {
            Point chunkPosition = GetChunkPositionOf(tilePosition);

            if (!Chunks.ContainsKey(chunkPosition)) {
                return null;
            }

            return Chunks[chunkPosition][tilePosition.X - chunkPosition.X * ChunkLength, tilePosition.Y - chunkPosition.Y * ChunkLength];
        }

        public Tile[,] GetTiles(Point tilePosition1, Point tilePosition2) {
            var point1 = MathsHelper.ValidizePoints(tilePosition1, tilePosition2).Item1;
            var point2 = MathsHelper.ValidizePoints(tilePosition1, tilePosition2).Item2;

            int columns = point2.X - point1.X + 1; int rows = point2.Y - point1.Y + 1;

            var tiles = new Tile[columns, rows];

            for (int y = 0; y < rows; y++) {
                for (int x = 0; x < columns; x++) {
                    tiles[x, y] = GetTile(point1 + new Point(x, y));
                }
            }

            return tiles;
        }

        #endregion

        public void Update() {
            // foreach (KeyValuePair<Point, Tile[,]> pointTilePair in Chunks) {
            //     for (int y = 0; y < ChunkLength; y++) {
            //         for (int x = 0; x < ChunkLength; x++) {
            //             pointTilePair.Value[x, y]?.Update();
            //         }
            //     }
            // }
        }

        public void Draw(Camera camera, float opacity = 1f) {
            var cameraBounds = camera.GetCameraBounds();

            var startChunkPosition = GetChunkPositionOf(GetTilePositionOf(MathsHelper.ToPoint(cameraBounds.Item1)));
            var endChunkPosition = GetChunkPositionOf(GetTilePositionOf(MathsHelper.ToPoint(cameraBounds.Item2)));

            for (int chunkY = startChunkPosition.Y; chunkY < endChunkPosition.Y; chunkY++) {
                for (int chunkX = startChunkPosition.X; chunkX < endChunkPosition.X; chunkX++) {
                    if (!Chunks.ContainsKey(new Point(chunkX, chunkY))) {
                        continue;
                    }

                    var chunk = Chunks[new Point(chunkX, chunkY)];

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
    }
}

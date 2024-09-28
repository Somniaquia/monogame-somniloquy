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
    public class TileLayer2D {
        public World ParentWorld { get; set; }
        public TileLayer2D ChildLayers { get; set; }
        public Point Dimensions { get; set; }
        [JsonConverter(typeof(ChunksConverter))]
        public Dictionary<Point, Tile[,]> Chunks { get; set; } = new();
        public static int ChunkLength { get; } = 16;
        public static int TileLength { get; } = 16;

        public TileLayer2D(World parentWorld) {
            ParentWorld = parentWorld;
        }

        public Point GetPositionInTile(Point worldPosition) {
            return new Point(Utils.Modulo(worldPosition.X, TileLength), Utils.Modulo(worldPosition.Y, TileLength));
        }

        public Point GetTilePositionOf(Point worldPosition) {
            return new Point(Utils.FloorDivide(worldPosition.X, TileLength), Utils.FloorDivide(worldPosition.Y, TileLength));
        }

        public Point GetChunkPositionOf(Point tilePosition) {
            return new Point(Utils.FloorDivide(tilePosition.X, ChunkLength), Utils.FloorDivide(tilePosition.Y, ChunkLength));
        }

        #region Paint Methods
        public void PaintOnTile(Point tilePosition, Color?[,] colors, WorldEditCommand command = null, bool sync = false) {
            if (sync) {
                var previousTile = GetTile(tilePosition);
                //SetTile(tilePosition, ParentWorld.NewTile(TileLength), command);
                SetTile(tilePosition, ParentWorld.NewTile());
                PaintOnTile(GetTile(tilePosition), previousTile.Sprite.GetFrameColors(), command);
            }

            var tile = GetTile(tilePosition);
            PaintOnTile(tile, colors, command);
        }

        public void PaintOnTile(Tile tile, Color?[,] colors, WorldEditCommand command = null) {
            var oldTexture = tile.Sprite.GetFrameColors();
            tile.Sprite.PaintOnFrame(colors);
            command?.AppendFrameTextureChanges(tile.Sprite.CurrentAnimation, tile.Sprite.CurrentAnimationFrame, oldTexture, tile.Sprite.GetFrameColors());
        }

        public void PaintLine(Point point1, Point point2, Color color, int width, WorldEditCommand command = null, bool sync = false) {
            int dx = Math.Abs(point2.X - point1.X);
            int dy = Math.Abs(point2.Y - point1.Y);
            int sx = (point1.X < point2.X) ? 1 : -1;
            int sy = (point1.Y < point2.Y) ? 1 : -1;
            int err = dx - dy;

            while (true) {
                PaintCircle(new Point(point1.X, point1.Y), color, width, command, sync);

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

        public void PaintCircle(Point point, Color color, int width, WorldEditCommand command = null, bool sync = false) {
            // TODO: Add brush shape other than a square
            int left = point.X - (width - 1) / 2;
            int right = point.X + width / 2;
            int top = point.Y - (width - 1) / 2;
            int bottom = point.Y + width / 2;

            PaintRectangle(new Rectangle(left, top, right - left, bottom - top), color, command, sync);
        }

        public void PaintRectangle(Rectangle rectangle, Color color, WorldEditCommand command, bool sync = false) {
            int startTileX = Utils.FloorDivide(rectangle.X, TileLength);
            int startTileY = Utils.FloorDivide(rectangle.Y, TileLength);
            int endTileX = Utils.FloorDivide(rectangle.Right, TileLength);
            int endTileY = Utils.FloorDivide(rectangle.Bottom, TileLength);

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
                        SetTile(position, ParentWorld.NewTile());
                    }

                    PaintOnTile(position, colors, command, sync);
                }
            }
        }

        public void PaintFill(Point positionInWorld, Color color, WorldEditCommand command = null, bool sync = false) {
            var positionStack = new Stack<Point>();
            positionStack.Push(positionInWorld);

            var initialTile = GetTile(GetTilePositionOf(positionInWorld));
            Dictionary<Tile, Color?[,]> tileColors = new();
            var targetColor = Color.Transparent;

            if (initialTile is not null) {
                //tileColors.Add(initialTile, initialTile.GetColorAt(EditorScreen.SelectedAnimationFrame));
                targetColor = GetTile(GetTilePositionOf(positionInWorld)).GetColorAt(GetPositionInTile(positionInWorld));
            }

            if (targetColor == color) return;

            while (positionStack.Count != 0) {
                var position = positionStack.Pop();
                var tile = GetTile(GetTilePositionOf(position));
                tileColors[tile][Utils.Modulo(position.X, TileLength), Utils.Modulo(position.Y, TileLength)] = color;

                foreach (var positionOffset in new Point[] { new Point(-1, 0), new Point(1, 0), new Point(0, -1), new Point(0, 1) }) {
                    var checkingPosition = position + positionOffset;
                    var checkingTile = GetTile(GetTilePositionOf(checkingPosition));
                    
                    if (checkingTile is null) {
                        if (targetColor == Color.Transparent) {
                            positionStack.Push(checkingPosition);
                        }
                    } else {
                        if (!tileColors.ContainsKey(checkingTile)) {
                            //tileColors.Add(checkingTile, checkingTile.Sprite.GetCurrentAnimation().GetFrameColors(0));
                        }

                        if (tileColors[checkingTile][Utils.Modulo(checkingPosition.X, TileLength), Utils.Modulo(checkingPosition.Y, TileLength)] == targetColor) {
                            positionStack.Push(checkingPosition);
                        }
                    }
                }

                if (positionStack.Count >= 10000) {
                    command.Undo();
                    CommandManager.UndoHistory.Pop();
                    return;
                }
            }

            foreach (var pair in tileColors) {
                PaintOnTile(pair.Key, pair.Value, command);
            }
        }

        #endregion

        #region Set Methods
        public void SetTile(Point position, Tile tile, WorldEditCommand command = null, bool preview = false) {
            if (preview) {
                tile?.Draw(new Rectangle(position.X * TileLength, position.Y * TileLength, TileLength, TileLength), 0.5f);
            } else {
                command?.AppendTileReferenceChanges(this, position, GetTile(position), tile);

                Point chunkPosition = GetChunkPositionOf(position);

                if (!Chunks.ContainsKey(chunkPosition)) {
                    AddChunk(chunkPosition);
                }

                Chunks[chunkPosition][position.X - chunkPosition.X * ChunkLength, position.Y - chunkPosition.Y * ChunkLength] = tile;
                
                foreach (var existingTile in ParentWorld.Tiles) {
                    if (object.ReferenceEquals(existingTile, tile)) return;
                }

                ParentWorld.Tiles.Add(tile);
            }
        }

        private void AddChunk(Point chunkPosition) {
            var chunk = new Tile[ChunkLength, ChunkLength];
            // for (int y = 0; y < ChunkLength; y++) {
            //     for (int x = 0; x < ChunkLength; x++) {
            //         chunk[x, y] = ParentWorld.DefaultTile;
            //     }
            // }
            Chunks.Add(chunkPosition, chunk);
        }

        public void SetLine(Point tilePos1, Point tilePos2, Tile[,] tilePattern, TileAction action, Point tilePatternOffset, int width, WorldEditCommand command = null, bool preview = false) {
            Point originalTilePos = tilePos1;

            int dx = Math.Abs(tilePos2.X - tilePos1.X);
            int dy = Math.Abs(tilePos2.Y - tilePos1.Y);
            int sx = (tilePos1.X < tilePos2.X) ? 1 : -1;
            int sy = (tilePos1.Y < tilePos2.Y) ? 1 : -1;
            int err = dx - dy;

            while (true) {
                SetCircle(new Point(tilePos1.X, tilePos1.Y), tilePattern, action, width, tilePatternOffset + tilePos1 - originalTilePos, command, preview);

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

        public void SetRectangle(Point tilePos1, Point tilePos2, Tile[,] tilePattern, TileAction action, Point tilePatternOffset, WorldEditCommand command = null, bool preview = false) {
            var pair = Utils.ValidizePoints(tilePos1, tilePos2);
            tilePos1 = pair.Item1;
            tilePos2 = pair.Item2;

            for (int y = tilePos1.Y; y <= tilePos2.Y; y++) {
                for (int x = tilePos1.X; x <= tilePos2.X; x++) {
                    if (action == TileAction.Repeat) {
                        SetTile(
                            new Point(x, y), 
                            tilePattern[
                                Utils.Modulo(tilePatternOffset.X + x - tilePos1.X, tilePattern.GetLength(0)),
                                Utils.Modulo(tilePatternOffset.Y + y - tilePos1.Y, tilePattern.GetLength(1))
                            ], command, preview
                        );
                    } else if (action == TileAction.Random) {
                        SetTile(
                            new Point(x, y),
                            tilePattern[
                                Utils.RandomInteger(0, tilePattern.GetLength(0)),
                                Utils.RandomInteger(0, tilePattern.GetLength(1))
                            ], command, preview
                        );
                        Console.WriteLine(Utils.RandomInteger(0, tilePattern.GetLength(0)));
                    } else if (action == TileAction.Wrap) {
                        int patternX, patternY;

                        if (tilePattern.GetLength(0) <= 2) patternX = Utils.Modulo(tilePatternOffset.X + x - tilePos1.X, tilePattern.GetLength(0));
                        else if (x == tilePos1.X) patternX = 0;
                        else if (x == tilePos2.X) patternX = tilePattern.GetLength(0) - 1;
                        else patternX = Utils.Modulo(tilePatternOffset.X + x - tilePos1.X - 1, tilePattern.GetLength(0) - 2) + 1;

                        if (tilePattern.GetLength(1) <= 2) patternY = Utils.Modulo(tilePatternOffset.Y + y - tilePos1.Y, tilePattern.GetLength(1));
                        else if (y == tilePos1.Y) patternY = 0;
                        else if (y == tilePos2.Y) patternY = tilePattern.GetLength(0) - 1;
                        else patternY = Utils.Modulo(tilePatternOffset.Y + y - tilePos1.Y - 1, tilePattern.GetLength(0) - 2) + 1;

                        SetTile(new Point(x, y), tilePattern[patternX, patternY], command, preview);
                    }
                }
            }
        }

        public void SetCircle(Point centerPosition, Tile[,] tilePattern, TileAction action, int width, Point tilePatternOffset, WorldEditCommand command = null, bool preview = false) {
            for (int y = -(width - 1) / 2; y <= width / 2; y++) {
                for (int x = -(width - 1) / 2; x <= width / 2; x++) {
                    Point position = new(centerPosition.X + x, centerPosition.Y + y);
                    if (Vector2.Distance(position.ToVector2(), centerPosition.ToVector2()) <= width) {
                        if (action == TileAction.Repeat) {
                            SetTile(
                                new Point(centerPosition.X + x, centerPosition.Y + y),
                                tilePattern[
                                    Utils.Modulo(tilePatternOffset.X + x, tilePattern.GetLength(0)),
                                    Utils.Modulo(tilePatternOffset.Y + y, tilePattern.GetLength(1))
                                ], command, preview
                            );
                        } else if (action == TileAction.Random) {
                            SetTile(
                                new Point(centerPosition.X + x, centerPosition.Y + y),
                                tilePattern[
                                    Utils.RandomInteger(0, tilePattern.GetLength(0)),
                                    Utils.RandomInteger(0, tilePattern.GetLength(1))
                                ], command, preview
                            );
                        } else if (action == TileAction.Wrap) {
                            
                        }
                    }
                }
            }   
        }

        public void SetFill(Point tilePosition, Tile[,] tilePattern, TileAction action, WorldEditCommand command = null) {
            var positionStack = new Stack<Point>();
            positionStack.Push(tilePosition);

            var targetTile = GetTile(tilePosition);
            if (ReferenceEquals(targetTile, tilePattern[0, 0])) return;

            while (positionStack.Count != 0) {
                var position = positionStack.Pop();
                SetTile(position, tilePattern[
                        Utils.Modulo(position.X - tilePosition.X, tilePattern.GetLength(0)),
                        Utils.Modulo(position.Y - tilePosition.Y, tilePattern.GetLength(1))
                    ], command
                );

                foreach (var positionOffset in new Point[] { new Point(-1, 0), new Point(1, 0) , new Point(0, -1) , new Point(0, 1) }) {
                    if (ReferenceEquals(GetTile(position + positionOffset), targetTile)) {
                        positionStack.Push(position + positionOffset);
                    }
                }

                if (positionStack.Count >= 10000) {
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
            var point1 = Utils.ValidizePoints(tilePosition1, tilePosition2).Item1;
            var point2 = Utils.ValidizePoints(tilePosition1, tilePosition2).Item2;

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

        public void Draw(Camera2D camera, float opacity = 1f) {
            var cameraBounds = camera.GetCameraBounds();

            var startChunkPosition = GetChunkPositionOf(GetTilePositionOf(Utils.ToPoint(cameraBounds.Item1)));
            var endChunkPosition = GetChunkPositionOf(GetTilePositionOf(Utils.ToPoint(cameraBounds.Item2)));

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
        
        public void DrawCollisionBoundaries(Camera2D camera, float opacity = 1f) {
            var cameraBounds = camera.GetCameraBounds();

            var startChunkPosition = GetChunkPositionOf(GetTilePositionOf(Utils.ToPoint(cameraBounds.Item1)));
            var endChunkPosition = GetChunkPositionOf(GetTilePositionOf(Utils.ToPoint(cameraBounds.Item2)));

            for (int chunkY = startChunkPosition.Y; chunkY < endChunkPosition.Y; chunkY++) {
                for (int chunkX = startChunkPosition.X; chunkX < endChunkPosition.X; chunkX++) {
                    if (!Chunks.ContainsKey(new Point(chunkX, chunkY))) {
                        continue;
                    }

                    var chunk = Chunks[new Point(chunkX, chunkY)];

                    for (int yInChunk = 0; yInChunk < ChunkLength; yInChunk++) {
                        for (int xInChunk = 0; xInChunk < ChunkLength; xInChunk++) {
                            chunk[xInChunk, yInChunk]?.DrawCollisionBoundaries(
                                camera.ApplyTransform(
                                    new Rectangle(
                                        (chunkX * ChunkLength + xInChunk) * TileLength,
                                        (chunkY * ChunkLength + yInChunk) * TileLength,
                                        TileLength, TileLength
                                    )
                                ), opacity
                            );
                        }
                    }
                }
            }
        }
    }
}

namespace Somniloquy {
    using System;
    using System.Collections.Generic;

    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using MonoGame.Extended;

    public class Tile {
        public int ID { get; set; }
        public FunctionalSprite FSprite { get; set; } = new();

        public Tile() {
            Texture2D transparentTexture = new Texture2D(GameManager.GraphicsDeviceManager.GraphicsDevice, Layer.TileLength, Layer.TileLength);
            Color[] data = new Color[Layer.TileLength * Layer.TileLength];
            Array.Fill(data, Color.Transparent);
            transparentTexture.SetData(data);

            var defaultAnimation = FSprite.AddAnimation("Default");
            defaultAnimation.AddFrame(transparentTexture);
        }

        public void PaintOnFrame(Texture2D texture, int frame) {
            FSprite.CurrentAnimation.PaintOnFrame(texture, frame);
        }

        public Color GetColorAt(Point point) {
            Color[] data = new Color[FSprite.CurrentAnimation.SpriteSheet.Width * FSprite.CurrentAnimation.SpriteSheet.Height];
            FSprite.CurrentAnimation.SpriteSheet.GetData(data);

            return data[
                (FSprite.CurrentAnimation.FrameBoundaries[FSprite.FrameInCurrentAnimation].Y + point.Y) * FSprite.CurrentAnimation.SpriteSheet.Width +
                FSprite.CurrentAnimation.FrameBoundaries[FSprite.FrameInCurrentAnimation].X + point.X
            ];
        }

        public void Update() {
            
        }

        public void Draw(Rectangle destination) {
            if (FSprite is null)
                GameManager.DrawFilledRectangle(destination, Color.DarkGray, 0.5f);
            else {
                GameManager.SpriteBatch.Draw(
                    FSprite.CurrentAnimation.SpriteSheet, 
                    destination, 
                    FSprite.CurrentAnimation.FrameBoundaries[FSprite.FrameInCurrentAnimation], 
                    Color.White);
            }
        }
    }

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
        public Dictionary<Point, Tile[,]> Chunks = new();
        public static int ChunkLength { get; } = 16;
        public static int TileLength { get; } = 8;

        public Layer(World parentWorld) {
            ParentWorld = parentWorld;
        }

        public Point GetPositionInTile(Point pixelPosition) {
            return new Point(MathsHelper.Modulo(pixelPosition.X, TileLength), MathsHelper.Modulo(pixelPosition.Y, TileLength));
        }

        public Point GetTilePositionOf(Point pixelPosition) {
            return new Point(MathsHelper.FloorDivide(pixelPosition.X, TileLength), MathsHelper.FloorDivide(pixelPosition.Y, TileLength));
        }

        public Point GetChunkPositionOf(Point tilePosition) {
            return new Point(MathsHelper.FloorDivide(tilePosition.X, ChunkLength), MathsHelper.FloorDivide(tilePosition.Y, ChunkLength));
        }

        #region Paint Methods
        public void PaintOnTile(Tile tile, Texture2D texture, int animationFrame, PaintCommand command = null) {
            if (command is not null) {
                command.Append(tile, tile.FSprite.CurrentAnimation.GetFrameTexture(animationFrame), texture);
            }

            tile.FSprite.CurrentAnimation.PaintOnFrame(texture, animationFrame);
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
                    var tilewiseTexture = new Texture2D(GameManager.GraphicsDeviceManager.GraphicsDevice, Layer.TileLength, Layer.TileLength);
                    Color[] data = new Color[Layer.TileLength * Layer.TileLength];
                    Array.Fill(data, Color.Transparent);

                    int startX = Math.Max(rectangle.X - tileX * TileLength, 0);
                    int startY = Math.Max(rectangle.Y - tileY * TileLength, 0);
                    int endX = Math.Min(rectangle.Right - tileX * TileLength, TileLength - 1);
                    int endY = Math.Min(rectangle.Bottom - tileY * TileLength, TileLength - 1);

                    for (int y = startY; y <= endY; y++) {
                        for (int x = startX; x <= endX; x++) {
                            data[y * TileLength + x] = color;
                        }
                    }

                    tilewiseTexture.SetData(data);

                    Point position = new Point(tileX, tileY);
                    if (GetTile(position) is null) {
                        SetTile(position, new Tile());
                    }

                    PaintOnTile(GetTile(position), tilewiseTexture, EditorScreen.selectedAnimationFrame, command);
                }
            }
        }

        #endregion

        #region Set Methods
        public void SetTile(Point position, Tile tile, SetCommand command = null) {
            if (command is not null) {
                command.Append(position, GetTile(position), tile);
            }

            Point chunkPosition = GetChunkPositionOf(position);

            if (!Chunks.ContainsKey(chunkPosition)) {
                Chunks.Add(chunkPosition, new Tile[ChunkLength, ChunkLength]);
            }

            if (!ParentWorld.Tiles.Contains(tile)) {
                ParentWorld.Tiles.Add(tile);
            }

            Chunks[chunkPosition][position.X - chunkPosition.X * ChunkLength, position.Y - chunkPosition.Y * ChunkLength] = tile;
        }

        public void SetLine(Point tilePos1, Point tilePos2, Tile tile, int width, SetCommand command = null) {
            int dx = Math.Abs(tilePos2.X - tilePos1.X);
            int dy = Math.Abs(tilePos2.Y - tilePos1.Y);
            int sx = (tilePos1.X < tilePos2.X) ? 1 : -1;
            int sy = (tilePos1.Y < tilePos2.Y) ? 1 : -1;
            int err = dx - dy;

            while (true) {
                SetCircle(new Point(tilePos1.X, tilePos1.Y), tile, width, command);

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

        public void SetRectangle(Point tilePos1, Point tilePos2, Tile tile, SetCommand command = null) {
            for (int y = tilePos1.Y; y < tilePos2.Y; y++) {
                for (int x = tilePos1.X; x < tilePos2.X; x++) {
                    SetTile(new Point(x, y), tile, command);
                }
            }
        }

        public void SetCircle(Point centerPosition, Tile tile, int width, SetCommand command = null) {
            for (int y = -(width - 1) / 2; y <= width / 2; y++) {
                for (int x = -(width - 1) / 2; x <= width / 2; x++) {
                    Point position = new Point(centerPosition.X + x, centerPosition.Y + y);
                    if (Vector2.Distance(position.ToVector2(), centerPosition.ToVector2()) <= width) {
                        SetTile(position, tile, command);
                    }
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

        public void Draw(Camera camera) {
            var cameraBounds = camera.GetCameraBounds();

            var startChunkPosition = GetChunkPositionOf(GetTilePositionOf(MathsHelper.ToPoint(cameraBounds.Item1)));
            var endChunkPosition = GetChunkPositionOf(GetTilePositionOf(MathsHelper.ToPoint(cameraBounds.Item2)));

            for (int chunkY = startChunkPosition.Y; chunkY < endChunkPosition.Y; chunkY++) {
                for (int chunkX = startChunkPosition.X; chunkX < endChunkPosition.X; chunkX++) {
                    if (!Chunks.ContainsKey(new Point(chunkX, chunkY))) {
                        continue;
                    }

                    var chunk = Chunks[new Point(chunkX, chunkY)];

                    GameManager.SpriteBatch.DrawRectangle(new Rectangle(
                                    (chunkX * ChunkLength) * TileLength,
                                    (chunkY * ChunkLength) * TileLength,
                                    TileLength * ChunkLength, TileLength * ChunkLength), Color.GhostWhite, layerDepth: 1);
                    for (int yInChunk = 0; yInChunk < ChunkLength; yInChunk++) {
                        for (int xInChunk = 0; xInChunk < ChunkLength; xInChunk++) {
                            chunk[xInChunk, yInChunk]?.Draw(
                                new Rectangle(
                                    (chunkX * ChunkLength + xInChunk) * TileLength,
                                    (chunkY * ChunkLength + yInChunk) * TileLength,
                                    TileLength, TileLength));
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// A World stores multiple map layers, which are the building blocks of a world. 
    /// World includes variables and functionalities of:
    /// - world rules, which determine how the particular world behave in the game
    /// -- this includes camera panning, entity physics, TODO: Think about what would fit world rules
    /// World does not include:
    /// - tile and entity data of the world. Those are saved in layers instead.
    /// To summarize, a world is merely a container for layers that should be grouped for sharing similar themes or behaviors
    /// </summary>
    public class World {
        public string Name { get; set; }
        public List<Layer> Layers { get; set; } = new();
        public List<Tile> Tiles { get; set; } = new();

        public static void Serialize(World world) {
            string serialized = "";

            // List of tiles - {key: ID - value: FSprite }
            // Check for identical tiles and merge them in serialization

            // List of layers - {key: ID - value: ChildLayers, Dimensions, Chunks }

            GameManager.WriteTextToFile(typeof(World), world.Name, serialized);
        }

        public static World Deserialize(string worldName) {
            string serialized = GameManager.ReadTextFromFile(typeof(World), worldName);
            World world = new World();

            // Set world properties

            return world;
        }

        public Layer AddLayer() {
            var layer = new Layer(this);
            Layers.Add(layer);
            return layer;
        }

        public void Update() {
            foreach (var layer in Layers) {
                layer.Update();
            }
        }

        public void Draw(Camera camera) {
            foreach (var layer in Layers) {
                layer.Draw(camera);
            }
        }

        public void DisposeTiles() {
            foreach (var tile in Tiles) {
                tile.FSprite.Dispose();
            }

            Tiles.Clear();
        }
    }

    /// <summary>
    /// WorldLoader handles updating and rendering of multiple worlds.
    /// The WorldLoader handles these functionalities:
    /// - Load in worlds that are currently present and unload those that are not
    /// - Update and draw worlds at are currently loaded
    /// 
    /// The WorldLoader doesn't handle these functionalities:
    /// - Serialization/Deserialization of the worlds - as the World class does them instead
    /// </summary>
    internal static class WorldManager {
        public static Dictionary<string, World> LoadedWorlds { get; private set; }

        public static void LoadWorld(string worldName) {
            LoadedWorlds.Add(worldName, World.Deserialize(worldName));
        }

        public static void UnloadWorld(string worldName) {
            LoadedWorlds.Remove(worldName);
        }

        public static void Update() {
            foreach (var entry in LoadedWorlds) {
                entry.Value.Update();
            }
        }

        public static void Draw(Camera camera) {
            foreach (var entry in LoadedWorlds) {
                entry.Value.Draw(camera);
            }
        }
    }
}

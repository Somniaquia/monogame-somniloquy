namespace Somniloquy {
    using System;
    using System.Collections.Generic;

    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using MonoGame.Extended;

    public class Tile {
        public FunctionalSprite FSprite { get; set; }
        public Color Color;

        public Tile(Color color) {
            Color = color;
        }
        
        public void Update() {
            
        }

        public void Draw(Rectangle destination) {
            if (FSprite is null)
                GameManager.DrawFilledRectangle(destination, Color, 0.5f);
            else {
                GameManager.SpriteBatch.Draw(
                    FSprite.CurrentAnimation.SpriteSheet, 
                    destination, 
                    FSprite.CurrentAnimation.FrameBoundaries[FSprite.FrameInCurrentAnimation], 
                    Color);
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
        public Layer ChildLayers { get; set; }
        public Point Dimensions { get; set; }
        public Dictionary<Point, Tile[,]> Chunks = new();
        public const int ChunkLength = 16;
        public const int TileLength = 16;

        public Point GetTilePositionOf(Point pixelPosition) {
            return new Point(Commons.FloorDivide(pixelPosition.X, TileLength), Commons.FloorDivide(pixelPosition.Y, TileLength));
        }

        public Point GetChunkPositionOf(Point tilePosition) {
            return new Point(Commons.FloorDivide(tilePosition.X, ChunkLength), Commons.FloorDivide(tilePosition.Y, ChunkLength));
        }

        public void SetLine(Point tilePos1, Point tilePos2, Tile tile) {
            int dx = Math.Abs(tilePos2.X - tilePos1.X);
            int dy = Math.Abs(tilePos2.Y - tilePos1.Y);
            int sx = (tilePos1.X < tilePos2.X) ? 1 : -1;
            int sy = (tilePos1.Y < tilePos2.Y) ? 1 : -1;
            int err = dx - dy;

            while (true) {
                SetTile(new Point(tilePos1.X, tilePos1.Y), tile);

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

        public void SetRectangle(Point tilePos1, Point tilePos2, Tile tile) {
            for (int y = tilePos1.Y; y < tilePos2.Y; y++) {
                for (int x = tilePos1.X; x < tilePos2.X; x++) {
                    SetTile(new Point(x, y), tile);
                }
            }
        }

        public void SetTile(Point tilePosition, Tile tile) {
            Point chunkPosition = GetChunkPositionOf(tilePosition);

            if (!Chunks.ContainsKey(chunkPosition)) {
                Chunks.Add(chunkPosition, new Tile[ChunkLength, ChunkLength]);
            }

            Chunks[chunkPosition][tilePosition.X - chunkPosition.X * ChunkLength, tilePosition.Y - chunkPosition.Y * ChunkLength] = tile;
        }

        public Tile GetTile(Point tilePosition) {
            Point chunkPosition = GetChunkPositionOf(tilePosition);
            return Chunks[chunkPosition][tilePosition.X - chunkPosition.X * ChunkLength, tilePosition.Y - chunkPosition.Y * ChunkLength];
        }

        public void Update() {
            foreach (KeyValuePair<Point, Tile[,]> pointTilePair in Chunks) {
                for (int y = 0; y < ChunkLength; y++) {
                    for (int x = 0; x < ChunkLength; x++) {
                        pointTilePair.Value[x, y]?.Update();
                    }
                }
            }
        }

        public void Draw() {
            // TODO: Only render chunks AND blocks on screen
            foreach (KeyValuePair<Point, Tile[,]> pointChunkPair in Chunks) {
                GameManager.SpriteBatch.DrawRectangle(new Rectangle(
                                (pointChunkPair.Key.X * ChunkLength) * TileLength,
                                (pointChunkPair.Key.Y * ChunkLength) * TileLength,
                                TileLength * ChunkLength, TileLength * ChunkLength), Color.FloralWhite, layerDepth:1);
                for (int y = 0; y < ChunkLength; y++) {
                    for (int x = 0; x < ChunkLength; x++) {
                        pointChunkPair.Value[x, y]?.Draw(
                            new Rectangle(
                                (pointChunkPair.Key.X * ChunkLength + x) * TileLength,
                                (pointChunkPair.Key.Y * ChunkLength + y) * TileLength,
                                TileLength, TileLength));
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
            // TODO: Add World Serialization logic
            string serialized = "";
            GameManager.WriteTextToFile(typeof(World), world.Name, serialized);
        }

        public static World Deserialize(string worldName) {
            string serialized = GameManager.ReadTextFromFile(typeof(World), worldName);
            World world = new World();

            // Set world properties

            return world;
        }

        public static World CreateNew() {
            World world = new World();
            world.Layers.Add(new Layer());
            return world;
        }

        public void Update() {
            foreach (var layer in Layers) {
                layer.Update();
            }
        }

        public void Draw() {
            foreach (var layer in Layers) {
                layer.Draw();
            }
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

        public static void Draw() {
            foreach (var entry in LoadedWorlds) {
                entry.Value.Draw();
            }
        }
    }
}

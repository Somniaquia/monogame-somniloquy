namespace Somniloquy {
    using System;
    using System.Collections.Generic;

    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using MonoGame.Extended;

    public class Tile {
        public FunctionalSprite FSprite;
        public void Update() {

        }

        public void Draw(Rectangle destination) {
            ResourceManager.SpriteBatch.DrawRectangle(destination, Color.Azure);
        }
    }

    /// <summary>
    /// A Layer is the building block of a world.
    /// Layers show and hide based on various events, such as player's approachment or interaction with certain objects, 
    /// random variables or internal states.
    /// The dynamic modification of layers in a same world will help form the overall expected nature of the game, triggering
    /// different layouts or looks for the same world in every visit, sometimes connecting a different world or triggering events such as jumpscares
    /// </summary>
    public class Layer
    {
        public Layer ChildLayers { get; set; }
        public Point Dimensions { get; set; }
        public Dictionary<Point, Tile[,]> Chunks = new();
        public const int ChunkLength = 16;
        public const int TileLength = 8;

        public Point GetTilePositionOf(Point pixelPosition) {
            return new Point(Commons.FloorDivide(pixelPosition.X, TileLength), Commons.FloorDivide(pixelPosition.Y, TileLength));
        }

        public Point GetChunkPositionOf(Point tilePosition) {
            return new Point(Commons.FloorDivide(tilePosition.X, ChunkLength), Commons.FloorDivide(tilePosition.Y, ChunkLength));
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
                ResourceManager.SpriteBatch.DrawRectangle(new Rectangle(
                                (pointChunkPair.Key.X * ChunkLength) * TileLength,
                                (pointChunkPair.Key.Y * ChunkLength) * TileLength,
                                TileLength * ChunkLength, TileLength * ChunkLength), Color.Black);
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

        public static void Serialize(World world) {
            // TODO: Add World Serialization logic
            string serialized = "";
            ResourceManager.WriteTextToFile(typeof(World), world.Name, serialized);
        }

        public static World Deserialize(string worldName) {
            string serialized = ResourceManager.ReadTextFromFile(typeof(World), worldName);
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

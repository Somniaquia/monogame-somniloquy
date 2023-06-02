namespace Somniloquy {
    using System;
    using System.Collections.Generic;

    using Microsoft.Xna.Framework;

    public struct Tile {

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
        public Tile[,] Tiles { get; set; }

        public void Update() {

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

        public static void Serialize(World world) {
            // TODO: Add World Serialization logic
            string serialized = "";
            SerializationManager.WriteToFile(typeof(World), world.Name, serialized);
        }

        public static World Deserialize(string worldName) {
            string serialized = SerializationManager.ReadFromFile(typeof(World), worldName);
            World world = new World();

            // Set world properties

            return world;
        }

        public void Update() {

        }

        public void Draw() {

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

namespace Somniloquy {
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal class World {
        public string Name { get; set; }
        public int Level { get; set; }

        public static void Serialize(World world) {
            // TODO: Add World Serialization logic
            string serialized = "";
            SerializationManager.WriteFile(typeof(World), world.Name, serialized);
        }

        public static World Deserialize(string worldName) {
            string serialized = SerializationManager.ReadFile(typeof(World), worldName);
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

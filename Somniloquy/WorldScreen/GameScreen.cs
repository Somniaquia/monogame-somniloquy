namespace Somniloquy {
    using System;
    using System.Collections.Generic;

    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Microsoft.Xna.Framework.Input;

    using MonoGame.Extended;
    using MonoGame.Extended.BitmapFonts;
    using MonoGame.Extended.Screens;

    public class GameScreen : Screen {
        public static Dictionary<string, World> LoadedWorlds { get; private set; }


        public static void LoadWorld(string worldName) {
            LoadedWorlds.Add(worldName, SerializationManager.Deserialize<World>(worldName));
        }

        public static void UnloadWorld(string worldName) {
            LoadedWorlds.Remove(worldName);
        }

        public GameScreen(Rectangle boundaries) : base(boundaries) {

        }

        public override void OnFocus() {
            
        }

        public override void Update() {
            foreach (var entry in LoadedWorlds) {
                entry.Value.Update();
            }
        }

        public override void Draw() {
            GameManager.SpriteBatch.Begin();

            // foreach (var entry in LoadedWorlds) {
            //     entry.Value.Draw(camera);
            // }
        }
    }
}
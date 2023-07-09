namespace Somniloquy {
    using System;
    using System.Collections.Generic;

    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Microsoft.Xna.Framework.Input;
    using Microsoft.Xna.Framework.Media;
    using MonoGame.Extended;
    using MonoGame.Extended.BitmapFonts;

    public class GameScreen : Screen {
        public List<World> LoadedWorlds { get; set; } = new();
        public Camera Camera { get; set; } = new Camera(8.0f);

        float pitch = 1.0f;
        string music = "moseni_fog";

        public void LoadWorld(string worldName) {
            LoadedWorlds.Add(SerializationManager.Deserialize<World>(worldName));
        }

        public void UnloadWorld(World world) {
            LoadedWorlds.Remove(world);
        }


        public GameScreen(Rectangle boundaries) : base(boundaries) {
            SoundManager.StartLoop(music, 2f);
            SoundManager.CenterFrequency = 150f;

        }

        public override void OnFocus() {
            
        }

        public override void Update() {
            if (InputManager.IsKeyPressed(Keys.Enter)) {
                SoundManager.StopLoop(music);
                ScreenManager.ToEditorScreen(this);
            }

            if (InputManager.IsKeyDown(Keys.Left)) {
                if (pitch > 0.1f) pitch -= 0.001f;
                SoundManager.SetPitch(music, pitch);
            } else if (InputManager.IsKeyDown(Keys.Right)) {
                if (pitch < 2f) pitch += 0.001f;
                SoundManager.SetPitch(music, pitch);
            } else if (InputManager.IsKeyDown(Keys.Up)) {
                SoundManager.CenterFrequency *= 1.001f;
            }
            else if (InputManager.IsKeyDown(Keys.Down)) {
                SoundManager.CenterFrequency /= 1.001f;
            }

            foreach (var entry in LoadedWorlds) {
                entry.Update();
            }
        }

        public override void Draw() {
            GameManager.SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, transformMatrix: Camera.Transform);

            foreach (var world in LoadedWorlds) {
                foreach (var layer in world.Layers) {
                    layer.Draw(Camera);
                }
            }

            GameManager.SpriteBatch.End();
        }
    }
}
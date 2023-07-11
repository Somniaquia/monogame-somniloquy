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
        public List<Entity> Entities { get; set; } = new();
        public Camera Camera { get; set; } = new Camera(8.0f);

        float pitch = 0.7f;
        static string music = "moseni_fog";

        public void LoadWorld(string worldName) {
            LoadedWorlds.Add(SerializationManager.Deserialize<World>(worldName));
        }

        public void UnloadWorld(World world) {
            LoadedWorlds.Remove(world);
        }

        public GameScreen(Rectangle boundaries) : base(boundaries) {
            SoundManager.StartLoop(music, 2f);
            SoundManager.CenterFrequency = 150f;
            SoundManager.SetPitch(music, pitch);
        }

        public void AddPlayer() {
            var player = new Player {
                CollisionBounds = new CircleF(Point.Zero, 4f),
                Camera = Camera,
                CurrentLayer = LoadedWorlds[0].Layers[0]
            };
            Entities.Add(player);
        }

        public override void OnFocus() {
            
        }

        public override void Update() {
            if (InputManager.IsKeyDown(Keys.Left)) {
                if (pitch > 0.1f) pitch -= 0.001f;
                SoundManager.SetPitch(music, pitch);
            } else if (InputManager.IsKeyDown(Keys.Right)) {
                if (pitch < 2f) pitch += 0.001f;
                SoundManager.SetPitch(music, pitch);
            } else if (InputManager.IsKeyDown(Keys.Up)) {
                SoundManager.CenterFrequency *= 1.01f;
            }
            else if (InputManager.IsKeyDown(Keys.Down)) {
                SoundManager.CenterFrequency /= 1.01f;
            }            

            if (InputManager.GetNumberKeyPress() != null) {
                if (InputManager.IsKeyPressed(Keys.Enter)) {
                    SoundManager.StopLoop(music);

                    if (InputManager.IsKeyDown(Keys.D1)) music = "moseni_fog";
                    if (InputManager.IsKeyDown(Keys.D2)) music = "moseni_lotus";
                    if (InputManager.IsKeyDown(Keys.D3)) music = "moseni_hydrangea";
                    if (InputManager.IsKeyDown(Keys.D4)) music = "moseni_luna";
                    if (InputManager.IsKeyDown(Keys.D5)) music = "loop_74";
                    if (InputManager.IsKeyDown(Keys.D6)) music = "n3-KtO";
                    if (InputManager.IsKeyDown(Keys.D7)) music = "n3-LI";
                    if (InputManager.IsKeyDown(Keys.D8)) music = "本名_1_small";

                    SoundManager.CenterFrequency = 150f;
                    SoundManager.StartLoop(music);
                    pitch = 0.7f;
                    SoundManager.SetPitch(music, pitch);
                }
            } else if (InputManager.IsKeyPressed(Keys.Enter)) {
                SoundManager.StopLoop(music);
                ScreenManager.ToEditorScreen(this);
            }

            foreach (var world in LoadedWorlds) {
                world.Update();
            }

            foreach (var entity in Entities) {
                entity.Update();
            }

            Camera.UpdateTransformation();
        }

        public override void Draw() {
            GameManager.SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, transformMatrix: Camera.Transform);

            foreach (var world in LoadedWorlds) {
                foreach (var layer in world.Layers) {
                    layer.Draw(Camera);
                }
            }

            foreach (var entity in Entities) {
                entity.Draw();
            }

            GameManager.SpriteBatch.End();
        }
    }
}
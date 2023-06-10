namespace Somniloquy {
    using System;
    using System.Collections.Generic;

    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Microsoft.Xna.Framework.Input;
    using MonoGame.Extended;

    public abstract class Screen {
        public abstract void Update();
        public abstract void Draw();
    }

    public class WorldScreen : Screen {
        public bool EditMode { get; private set; } = true;
        public Camera Camera { get; private set; } = new Camera(4.0f);
        public World ActiveWorld { get; private set; } = new();
        private Texture2D TransitionSnipplet;

        public void ToggleEditMode() {
            EditMode = !EditMode;
        }

        public override void Update() {
            if (EditMode) {
                Vector2 mousePosition = Camera.ApplyInvertTransform(InputManager.GetMousePosition());
                
                if (InputManager.IsLeftButtonDown()) {
                    if (ActiveWorld.Layers.Count == 0) ActiveWorld.Layers.Add(new Layer());
                    ActiveWorld.Layers[0].SetTile(ActiveWorld.Layers[0].GetTilePositionOf(mousePosition.ToPoint()), new Tile());
                }

                if (InputManager.IsKeyDown(Keys.Down)) Camera.Move(new Vector2(0, 1));
                if (InputManager.IsKeyDown(Keys.Up)) Camera.Move(new Vector2(0, -1));
                if (InputManager.IsKeyDown(Keys.Right)) Camera.Move(new Vector2(1, 0));
                if (InputManager.IsKeyDown(Keys.Left)) Camera.Move(new Vector2(-1, 0));

                if (InputManager.IsKeyDown(Keys.Space)) Camera.Zoom *= 1.1f;
                if (InputManager.IsKeyDown(Keys.LeftShift)) Camera.Zoom *= 0.9f;

            } else {

            }
            Camera.UpdateTransformation();
            ActiveWorld?.Update();
        }

        public override void Draw() {
            ResourceManager.SpriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, transformMatrix: Camera.Transform);
            ActiveWorld?.Draw();
            Vector2 mousePosition = Camera.ApplyInvertTransform(InputManager.GetMousePosition());
            ResourceManager.SpriteBatch.DrawPoint(mousePosition.X, mousePosition.Y, Color.Black, 4);
            ResourceManager.SpriteBatch.End();
        }
    }

    public class UIScreen : Screen {
        private ColorChart ColorChart = new();

        public override void Update() {
            ColorChart.GenerateColorChart(16, 16);
        }

        public override void Draw() {
            ResourceManager.SpriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend);
            ResourceManager.SpriteBatch.Draw(ColorChart.Chart,
                new Rectangle(
                    ResourceManager.GraphicsDeviceManager.GraphicsDevice.Viewport.Width - 144,
                    ResourceManager.GraphicsDeviceManager.GraphicsDevice.Viewport.Height - 144, 
                    128, 128), 
                Color.White);
            ResourceManager.SpriteBatch.End();
        }        
    }
}
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
                Vector2 previousMousePosition = Camera.ApplyInvertTransform(InputManager.GetPreviousMousePosition());

                if (InputManager.IsLeftButtonDown()) {
                    Tile tile = null;
                    if (!InputManager.IsKeyDown(Keys.LeftControl))
                        tile = new Tile();

                    if (ActiveWorld.Layers.Count == 0) ActiveWorld.Layers.Add(new Layer());
                    if (InputManager.IsLeftButtonClicked())
                        ActiveWorld.Layers[0].SetTile(ActiveWorld.Layers[0].GetTilePositionOf(mousePosition.ToPoint()), tile);
                    else
                        ActiveWorld.Layers[0].SetLine(
                            ActiveWorld.Layers[0].GetTilePositionOf(previousMousePosition.ToPoint()),
                            ActiveWorld.Layers[0].GetTilePositionOf(mousePosition.ToPoint()),
                            tile
                        );
                }

                if (InputManager.IsKeyPressed(Keys.Delete)) ActiveWorld.Layers[0] = new Layer();

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
        public ColorChart ColorChart;

        public UIScreen() {
            ColorChart = new ColorChart(new Rectangle(
                    ResourceManager.GraphicsDeviceManager.GraphicsDevice.Viewport.Width - 144,
                    ResourceManager.GraphicsDeviceManager.GraphicsDevice.Viewport.Height - 144,
                    128, 128));
        }

        public override void Update() {
            ColorChart.UpdateChart();
        }

        public override void Draw() {
            ResourceManager.SpriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend);

            ColorChart.Draw();

            ResourceManager.SpriteBatch.DrawString(ResourceManager.Misaki, "color palette", 
                new Vector2(ResourceManager.GraphicsDeviceManager.GraphicsDevice.Viewport.Width - 144,
                    ResourceManager.GraphicsDeviceManager.GraphicsDevice.Viewport.Height - 160), Color.White
            );

            ResourceManager.SpriteBatch.End();
        }        
    }
}
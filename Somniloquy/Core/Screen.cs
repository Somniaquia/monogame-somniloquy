namespace Somniloquy {
    using System;
    using System.Collections.Generic;

    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Microsoft.Xna.Framework.Input;
    using MonoGame.Extended;

    public abstract class Screen {
        public Rectangle Boundaries { get; set; }
        public Matrix TransformMatrix { get; protected set; }
        public List<Screen> ChildScreens { get; set; } = new();
        public bool Focusable { get; protected set; } = true;

        public Screen(Rectangle boundaries) {
            Boundaries = boundaries;
            TransformMatrix = Matrix.CreateTranslation(-boundaries.X, -boundaries.Y, 0f)
            * Matrix.CreateScale(1f / boundaries.Width, 1f / boundaries.Height, 1f);
        }

        public abstract void OnFocus();

        public virtual void Update() {
            if (Focusable) {
                if (Commons.IsWithinBoundaries(InputManager.GetMousePosition().ToPoint(), Boundaries)) {
                    if (InputManager.IsLeftButtonClicked()) {
                        InputManager.Focus = this;
                    }
                }
            }

            foreach (var child in ChildScreens) {
                child.Update();
            }

            if (InputManager.Focus == this) {
                // System.Console.WriteLine(this);
                OnFocus();
            }
        }

        public virtual void Draw() {
            foreach (var child in ChildScreens) {
                child.Draw();
            }
        }
    }

    public class EditorScreen : Screen {
        public Camera Camera { get; private set; } = new Camera(4.0f);
        public World ActiveWorld { get; private set; } = new();
        public static Color EditorColor { get; set; } = Color.AliceBlue;

        public EditorScreen(Rectangle boundaries) : base(boundaries) {
            ColorChart colorChart = new ColorChart(new Rectangle(boundaries.Width - 144, boundaries.Height - 144, 128, 128));
            colorChart.UpdateChart();
            ChildScreens.Add(colorChart);
        }

        public override void OnFocus() {
            Vector2 mousePosition = Camera.ApplyInvertTransform(InputManager.GetMousePosition());
            Vector2 previousMousePosition = Camera.ApplyInvertTransform(InputManager.GetPreviousMousePosition());

            if (InputManager.IsLeftButtonDown()) {
                Tile tile = null;
                if (!InputManager.IsKeyDown(Keys.LeftControl)) {
                    tile = new Tile();
                    tile.Color = EditorColor;
                }

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

            if (InputManager.IsKeyDown(Keys.S)) Camera.Move(new Vector2(0, 1));
            if (InputManager.IsKeyDown(Keys.W)) Camera.Move(new Vector2(0, -1));
            if (InputManager.IsKeyDown(Keys.D)) Camera.Move(new Vector2(1, 0));
            if (InputManager.IsKeyDown(Keys.A)) Camera.Move(new Vector2(-1, 0));

            if (InputManager.IsKeyDown(Keys.Space)) Camera.Zoom *= 1.1f;
            if (InputManager.IsKeyDown(Keys.LeftShift)) Camera.Zoom *= 0.9f;
        }

        public override void Update() {
            Camera.UpdateTransformation();
            ActiveWorld?.Update();
            base.Update();
        }

        public override void Draw() {
            GameManager.SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, transformMatrix: Camera.Transform);
            ActiveWorld?.Draw();

            Vector2 mousePosition = Camera.ApplyInvertTransform(InputManager.GetMousePosition());
            GameManager.SpriteBatch.DrawPoint(mousePosition.X, mousePosition.Y, Color.Black, 10 * InputManager.GetPenPressure());
            GameManager.SpriteBatch.End();

            GameManager.SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
            base.Draw();

            // TODO: Move to Text UI class
            GameManager.SpriteBatch.DrawString(GameManager.Misaki, "color palette", new Vector2(Boundaries.Width - 144, Boundaries.Height - 160), Color.White);
            GameManager.SpriteBatch.End();
        }
    }
}
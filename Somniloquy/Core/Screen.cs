namespace Somniloquy {
    using System;
    using System.Collections.Generic;

    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Microsoft.Xna.Framework.Input;
    using MonoGame.Extended;
    using MonoGame.Extended.BitmapFonts;

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
                if (MathsHelper.IsWithinBoundaries(InputManager.GetMousePosition().ToPoint(), Boundaries)) {
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

    public enum EditorState { TexturePaintMode, MaterialSetMode, LayerEditMode, SpriteEditMode }

    public class EditorScreen : Screen {
        public EditorState EditorState = EditorState.TexturePaintMode;

        public Camera Camera { get; private set; } = new Camera(8.0f);
        public World ActiveWorld { get; private set; } = new();
        public static Color SelectedColor { get; set; } = Color.AliceBlue;
        public static Tile SelectedTile { get; set; } = null;
        public ColorChart ColorChart { get; private set; }
        public int animationFrame { get; set; } = 0;

        public EditorScreen(Rectangle boundaries) : base(boundaries) {
            ColorChart = new ColorChart(new Rectangle(boundaries.Width - 144, boundaries.Height - 144, 128, 128));
            ColorChart.UpdateChart();
            ChildScreens.Add(ColorChart);
        }

        public override void OnFocus() {
            if (InputManager.IsKeyPressed(Keys.F1)) {
                EditorState = EditorState.TexturePaintMode; 
            } else if (InputManager.IsKeyPressed(Keys.F2)) {
                EditorState = EditorState.MaterialSetMode;
            } else if (InputManager.IsKeyPressed(Keys.F3)) {
                EditorState = EditorState.LayerEditMode;
            }

            Vector2 mousePosition = Camera.ApplyInvertTransform(InputManager.GetMousePosition());
            Vector2 previousMousePosition = Camera.ApplyInvertTransform(InputManager.GetPreviousMousePosition());

            if (ActiveWorld.Layers.Count == 0) ActiveWorld.Layers.Add(new Layer());
            var layer = ActiveWorld.Layers[0];

            if (EditorState == EditorState.TexturePaintMode) {
                if (InputManager.IsLeftButtonDown()) {
                    var color = SelectedColor;

                    if (InputManager.IsKeyDown(Keys.LeftControl)) {
                        color = Color.Transparent;
                    }

                    if (InputManager.IsLeftButtonClicked()) {
                        if (layer.GetTile(layer.GetTilePositionOf(mousePosition.ToPoint())) is null) {
                            layer.SetTile(layer.GetTilePositionOf(mousePosition.ToPoint()), new Tile(Color.White * 0.2f), Math.Max(1, (int)(InputManager.PenPressure * 5)));
                        }

                        layer.PaintPixel(mousePosition.ToPoint(), color, Math.Max(1, (int)(InputManager.PenPressure * 5)));
                    } else {
                        layer.PaintLine(previousMousePosition.ToPoint(), mousePosition.ToPoint(), color, Math.Max(1, (int)(InputManager.PenPressure * 5)));
                    }
                }

                if (InputManager.IsRightButtonDown()) {
                    if (layer.GetTile(layer.GetTilePositionOf(mousePosition.ToPoint())) is not null) {
                        ColorChart.FetchPositionAndHueFromColor(layer.GetTile(layer.GetTilePositionOf(mousePosition.ToPoint())).GetColorAt(layer.GetPositionInTile(mousePosition.ToPoint())));
                    }
                }

            } else if (EditorState == EditorState.MaterialSetMode) {
                if (InputManager.IsLeftButtonDown()) {
                    Tile tile = null;
                    if (!InputManager.IsKeyDown(Keys.LeftControl)) {
                        tile = SelectedTile;
                    }

                    if (InputManager.IsLeftButtonClicked()) {
                        layer.SetTile(layer.GetTilePositionOf(mousePosition.ToPoint()), tile, Math.Max(1, (int) (InputManager.PenPressure * 5)));
                    }

                    else {
                        layer.SetLine(
                            layer.GetTilePositionOf(previousMousePosition.ToPoint()),
                            layer.GetTilePositionOf(mousePosition.ToPoint()),
                            tile, Math.Max(1, (int) (5 * InputManager.PenPressure))
                        );
                    }
                }

                if (InputManager.IsRightButtonDown()) {
                    if (layer.GetTile(layer.GetTilePositionOf(mousePosition.ToPoint())) is not null) {
                        SelectedTile = layer.GetTile(layer.GetTilePositionOf(mousePosition.ToPoint()));
                    }
                }
            }

            if (InputManager.IsKeyPressed(Keys.Delete)) layer = new Layer();

            if (InputManager.IsKeyDown(Keys.S)) Camera.Move(new Vector2(0, 1));
            if (InputManager.IsKeyDown(Keys.W)) Camera.Move(new Vector2(0, -1));
            if (InputManager.IsKeyDown(Keys.D)) Camera.Move(new Vector2(1, 0));
            if (InputManager.IsKeyDown(Keys.A)) Camera.Move(new Vector2(-1, 0));
            if (InputManager.IsKeyDown(Keys.Q)) Camera.Rotation += 0.1f;
            if (InputManager.IsKeyDown(Keys.E)) Camera.Rotation -= 0.1f;

            if (InputManager.IsKeyDown(Keys.Space)) Camera.Zoom *= 1.1f;
            if (InputManager.IsKeyDown(Keys.LeftShift)) Camera.Zoom *= 0.9f;
        }

        public override void Update() {
            Camera.UpdateTransformation();
            ActiveWorld?.Update();
            base.Update();
        }

        public override void Draw() {
            GameManager.SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, transformMatrix: Camera.Transform);
            ActiveWorld?.Draw(Camera);

            Vector2 mousePosition = Camera.ApplyInvertTransform(InputManager.GetMousePosition());
            GameManager.SpriteBatch.DrawPoint(mousePosition.X, mousePosition.Y, Color.Black);
            GameManager.SpriteBatch.End();

            GameManager.SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp);
            base.Draw();

            //GameManager.SpriteBatch.DrawString(GameManager.Misaki, "こんにちは", new Vector2(Boundaries.Width - 144, Boundaries.Height - 160), Color.White);
            GameManager.SpriteBatch.End();
        }
    }
}
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
                if (MathsHelper.IsWithinBoundaries(MathsHelper.ToPoint(InputManager.GetMousePosition()), Boundaries)) {
                    if (InputManager.IsLeftButtonClicked()) {
                        InputManager.Focus = this;
                    }
                }
            }

            foreach (var child in ChildScreens) {
                child.Update();
            }

            if (InputManager.Focus == this) {
                OnFocus();
            }
        }

        public virtual void Draw() {
            foreach (var child in ChildScreens) {
                child.Draw();
            }
        }
    }

    public enum EditorState { PaintMode, TileMode, LayerEditMode, SpriteEditMode }

    /// <summary>
    /// EditorScreen is a screen for... well... editing the worlds you create!
    /// It has several states to allow easier world creations, and here are the list of the actions of each states:
    /// 
    /// Universal:
    /// - W, A, S, D: Move around the world, as in play mode
    /// - Space + Left Mouse Button: Move around the world, probably a more preferred way for artists of conventional art programs 
    /// - Q, E: Zoom in and out to get a better view of what you're drawing
    /// - Tab: Toggle PaintMode/TileMode
    /// - Hold Alt: Select the topmost layer beneath the mouse
    /// - +/-: Progress animation frames
    /// - Ctrl + Z/ Ctrl + Shift + Z: Undo/Redo
    /// 
    /// PaintMode:
    /// - [PaintCommand] Left Mouse Button: Draw pixels directly on tiles
    /// - Alt + Left Mouse Button: Get the uppermost color beneath the mouse
    /// - [PaintRectangleCommand] Shift + Left Mouse Button: Draw rectangles
    /// - [PaintLineCommand] Ctrl + Left Mouse Button: Draw Lines (either 0, pi/6, pi/4, pi/4, pi/2 (and inversed) angles)
    /// - [PaintFreeLineCommand] Ctrl + Shift + Left Mouse Button: Draw lines (free angle)
    /// - Hold Z: When drawing, you erase instead
    /// - I, J, K, L: Move around the color palette
    /// - U, O: Change color palette's hue
    /// -  
    /// 
    /// TileMode:
    /// - [SetCommand] Left Mouse Button: Set tiles / tile patterns
    /// - Alt + Left Mouse Button: Get uppermost tile beneath the mouse
    /// - Alt + Shift + Left Mouse Button: Pick up tiles within specified rectangular boundary, get tile pattern
    /// - [SetRectangleCommand] Shift + Left Mouse Button: Draw rectangles of tiles / tile patterns
    /// - [SetCommand] Ctrl + Left Mouse Button: Draw lines of tiles (either 0, pi/6, pi/4, pi/4, pi/2 (and inversed) angles)
    /// - Hold Z: When setting tiles, you remove the tile from the position instead
    /// - 
    /// 
    /// </summary>
    public class EditorScreen : Screen {
        public EditorState EditorState = EditorState.PaintMode;

        public World SelectedWorld { get; private set; } = new();
        public Layer SelectedLayer { get; private set; } = null;
        public static Color SelectedColor { get; set; } = Color.AliceBlue;
        public static Tile SelectedTile { get; set; } = null;
        public static ICommand ActiveCommand { get; set; } = null;
        public static int selectedAnimationFrame { get; set; } = 0;
        
        public Camera Camera { get; private set; } = new Camera(8.0f);
        public ColorChart ColorChart { get; private set; } = null;
        private Point mousePositionInWorld;

        public EditorScreen(Rectangle boundaries) : base(boundaries) {
            ColorChart = new ColorChart(new Rectangle(boundaries.Width - 144, boundaries.Height - 144, 128, 128));
            ColorChart.UpdateChart();
            ChildScreens.Add(ColorChart);
        }

        public override void OnFocus() {
            if (InputManager.IsKeyPressed(Keys.F1)) {
                EditorState = EditorState.PaintMode; 
            } else if (InputManager.IsKeyPressed(Keys.F2)) {
                EditorState = EditorState.TileMode;
            } else if (InputManager.IsKeyPressed(Keys.F3)) {
                EditorState = EditorState.LayerEditMode;
            }

            Point previousMousePositionInWorld = mousePositionInWorld;
            mousePositionInWorld = MathsHelper.ToPoint(Camera.ApplyInvertTransform(InputManager.GetMousePosition()));
            
            if (SelectedWorld.Layers.Count == 0) SelectedLayer = SelectedWorld.AddLayer();

            if (InputManager.IsKeyDown(Keys.S)) Camera.Move(new Vector2(0, 1));
            if (InputManager.IsKeyDown(Keys.W)) Camera.Move(new Vector2(0, -1));
            if (InputManager.IsKeyDown(Keys.D)) Camera.Move(new Vector2(1, 0));
            if (InputManager.IsKeyDown(Keys.A)) Camera.Move(new Vector2(-1, 0));
            if (InputManager.IsKeyDown(Keys.Q)) Camera.Zoom(-0.1f);
            if (InputManager.IsKeyDown(Keys.E)) Camera.Zoom(0.1f);

            if (InputManager.IsKeyPressed(Keys.Delete)) {
                // TODO: This is not a proper implementation - work multi-layer support!!
                CommandManager.Clear();
                SelectedWorld.Layers.Clear();
                SelectedWorld.DisposeTiles();
            }

            if (InputManager.IsKeyDown(Keys.LeftControl) && InputManager.IsKeyPressed(Keys.Z)) {
                if (!InputManager.IsKeyDown(Keys.LeftShift)) {
                    CommandManager.Undo();
                } else {
                    CommandManager.Redo();
                }
            }
        
            if (EditorState == EditorState.PaintMode) {
                if (InputManager.IsLeftButtonDown()) {
                    var color = SelectedColor;

                    if (InputManager.IsKeyDown(Keys.LeftAlt)) {
                        if (SelectedLayer.GetTile(SelectedLayer.GetTilePositionOf(mousePositionInWorld)) is not null) {
                            ColorChart.FetchPositionAndHueFromColor(SelectedLayer.GetTile(SelectedLayer.GetTilePositionOf(mousePositionInWorld)).GetColorAt(SelectedLayer.GetPositionInTile(mousePositionInWorld)));
                        }
                    }

                    if (InputManager.IsKeyDown(Keys.Z)) {
                        color = Color.Transparent;
                    }

                    if (InputManager.IsLeftButtonClicked()) {
                        ActiveCommand = new PaintCommand(selectedAnimationFrame);
                        CommandManager.Push(ActiveCommand);
                        SelectedLayer.PaintCircle(
                            mousePositionInWorld, 
                            color, Math.Max(1, (int)(InputManager.PenPressure * 5)), 
                            (PaintCommand)ActiveCommand
                        );
                    } else {
                        if (ActiveCommand is PaintCommand) {
                            SelectedLayer.PaintLine(
                                previousMousePositionInWorld, mousePositionInWorld, 
                                color, Math.Max(1, (int)(InputManager.PenPressure * 5)), 
                                (PaintCommand)ActiveCommand
                            );
                        }
                    }
                }

            } else if (EditorState == EditorState.TileMode) {
                if (InputManager.IsLeftButtonDown()) {
                    Tile tile = null;

                    if (InputManager.IsKeyDown(Keys.LeftAlt)) {
                        if (SelectedLayer.GetTile(SelectedLayer.GetTilePositionOf(mousePositionInWorld)) is not null) {
                            SelectedTile = SelectedLayer.GetTile(SelectedLayer.GetTilePositionOf(mousePositionInWorld));
                        }
                    }

                    if (!InputManager.IsKeyDown(Keys.Z)) {
                        tile = SelectedTile;
                    }

                    if (InputManager.IsLeftButtonClicked()) {
                        ActiveCommand = new SetCommand(SelectedLayer);
                        CommandManager.Push(ActiveCommand);
                        SelectedLayer.SetCircle(
                            SelectedLayer.GetTilePositionOf(mousePositionInWorld), 
                            tile, Math.Max(1, (int) (InputManager.PenPressure * 5)), 
                            (SetCommand)ActiveCommand
                        );
                    }

                    else {
                        if (ActiveCommand is SetCommand) {
                            SelectedLayer.SetLine(
                                SelectedLayer.GetTilePositionOf(previousMousePositionInWorld),
                                SelectedLayer.GetTilePositionOf(mousePositionInWorld),
                                tile, Math.Max(1, (int) (5 * InputManager.PenPressure)),
                                (SetCommand)ActiveCommand
                            );
                        }
                    }
                }
            }
        }

        public override void Update() {
            Camera.UpdateTransformation();
            SelectedWorld?.Update();
            base.Update();
        }

        public override void Draw() {
            GameManager.SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, transformMatrix: Camera.Transform);
            SelectedWorld?.Draw(Camera);

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
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
            TransformMatrix = 
                Matrix.CreateTranslation(-boundaries.X, -boundaries.Y, 0f) * 
                Matrix.CreateScale(1f / boundaries.Width, 1f / boundaries.Height, 1f);
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
    public enum EditorAction { PaintIdle, PaintRectangle, PaintLine, TileIdle, TileSelection, TileRectangle, TileLine }

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
    /// - [PaintCommand] Shift + Left Mouse Button: Draw rectangles
    /// - [PaintCommand] Ctrl + Shift + Left Mouse Button: Draw Lines (either 0, pi/6, pi/4, pi/4, pi/2 (and inversed) angles)
    /// - [PaintCommand] Ctrl + Left Mouse Button: Draw lines (free angle)
    /// - Hold Z: When drawing, you erase instead
    /// - I, J, K, L: Move around the color palette
    /// - U, O: Change color palette's hue
    /// -  
    /// 
    /// TileMode:
    /// - [SetCommand] Left Mouse Button: Set tiles / tile patterns
    /// - Alt + Left Mouse Button: Get uppermost tile beneath the mouse
    /// - Alt + Shift + Left Mouse Button: Pick up tiles within specified rectangular boundary, get tile pattern
    /// - [SetCommand] Shift + Left Mouse Button: Draw rectangles of tiles / tile patterns
    /// - [SetCommand] Ctrl + Left Mouse Button: Draw lines of tiles (either 0, pi/6, pi/4, pi/4, pi/2 (and inversed) angles)
    /// - Hold Z: When setting tiles, you remove the tile from the position instead
    /// - 
    /// 
    /// </summary>
    public class EditorScreen : Screen
    {
        public EditorState EditorState = EditorState.PaintMode;
        public EditorAction EditorAction = EditorAction.PaintIdle;

        public World SelectedWorld { get; private set; } = new();
        public Layer SelectedLayer { get; private set; } = null;
        public static Color SelectedColor { get; set; } = Color.AliceBlue;
        public static Tile[,] TilePattern { get; set; } = new Tile[1, 1] { { null } };
        public static ICommand ActiveCommand { get; set; } = null;
        public static int SelectedAnimationFrame { get; set; } = 0;
        
        public Camera Camera { get; private set; } = new Camera(8.0f);
        public ColorChart ColorChart { get; private set; } = null;
        private Point previousMousePositionInWorld;
        private Point mousePositionInWorld;
        private Point previousMouseTilePosition;
        private Point mouseTilePosition;
        private Point tilePatternOrigin = Point.Zero;

        private Point? firstPositionInWorld = null;

        public EditorScreen(Rectangle boundaries) : base(boundaries) {
            ColorChart = new ColorChart(new Rectangle(boundaries.Width - 144, boundaries.Height - 144, 128, 128));
            ColorChart.UpdateChart();
            ChildScreens.Add(ColorChart);
            InputManager.Focus = this;
        }

        public override void OnFocus() {
            if (SelectedWorld.Layers.Count == 0) SelectedLayer = SelectedWorld.AddLayer();

            if (InputManager.IsKeyPressed(Keys.F1)) {
                EditorState = EditorState.PaintMode;
                EditorAction = EditorAction.PaintIdle;
            } else if (InputManager.IsKeyPressed(Keys.F2)) {
                EditorState = EditorState.TileMode;
                EditorAction = EditorAction.TileIdle;
            }

            previousMousePositionInWorld = mousePositionInWorld;
            mousePositionInWorld = MathsHelper.ToPoint(Camera.ApplyInvertTransform(InputManager.GetMousePosition()));

            previousMouseTilePosition = SelectedLayer.GetTilePositionOf(previousMousePositionInWorld);
            mouseTilePosition = SelectedLayer.GetTilePositionOf(mousePositionInWorld);

            if (InputManager.IsKeyDown(Keys.S)) Camera.Move(new Vector2(0, 1));
            if (InputManager.IsKeyDown(Keys.W)) Camera.Move(new Vector2(0, -1));
            if (InputManager.IsKeyDown(Keys.D)) Camera.Move(new Vector2(1, 0));
            if (InputManager.IsKeyDown(Keys.A)) Camera.Move(new Vector2(-1, 0));
            if (InputManager.IsKeyDown(Keys.Q)) Camera.Zoom(-0.1f);
            if (InputManager.IsKeyDown(Keys.E)) Camera.Zoom(0.1f);

            if (InputManager.IsKeyDown(Keys.LeftControl) && InputManager.IsKeyPressed(Keys.S)) {
                SerializationManager.Serialize<World>(SelectedWorld, "world1.txt");
            }

            if (InputManager.IsKeyDown(Keys.LeftControl) && InputManager.IsKeyPressed(Keys.L)) {
                CommandManager.Clear();
                SelectedWorld.Layers.Clear();
                SelectedWorld.DisposeTiles();
                SelectedWorld = SerializationManager.Deserialize<World>("world1.txt");
            }

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

                if (InputManager.IsKeyDown(Keys.LeftControl)) {
                    EditorAction = EditorAction.PaintLine;
                } else if (InputManager.IsKeyDown(Keys.LeftShift)) {
                    EditorAction = EditorAction.PaintRectangle;
                } else {
                    EditorAction = EditorAction.PaintIdle;
                    firstPositionInWorld = null;
                }

                if (EditorAction == EditorAction.PaintRectangle) {
                    if (!firstPositionInWorld.HasValue) {
                        firstPositionInWorld = mousePositionInWorld;
                    }

                    if (InputManager.IsLeftButtonClicked()) {
                        ActiveCommand = new PaintCommand(SelectedAnimationFrame);
                        CommandManager.Push(ActiveCommand);

                        SelectedLayer.PaintRectangle(
                            MathsHelper.ValidizeRectangle(new Rectangle(firstPositionInWorld.Value, previousMousePositionInWorld - firstPositionInWorld.Value)),
                            SelectedColor, (PaintCommand)ActiveCommand
                        );
                        firstPositionInWorld = mousePositionInWorld;
                    }
                } else if (EditorAction == EditorAction.PaintLine) {
                    if (!firstPositionInWorld.HasValue) {
                        firstPositionInWorld = mousePositionInWorld;
                    }

                    if (InputManager.IsLeftButtonClicked()) {
                        ActiveCommand = new PaintCommand(SelectedAnimationFrame);
                        CommandManager.Push(ActiveCommand);
                        if (InputManager.IsKeyDown(Keys.LeftShift)) {
                            SelectedLayer.PaintLine(
                                firstPositionInWorld.Value, MathsHelper.AnchorPoint(mousePositionInWorld, firstPositionInWorld.Value),
                                SelectedColor, 1, (PaintCommand)ActiveCommand
                            );
                            firstPositionInWorld = MathsHelper.AnchorPoint(mousePositionInWorld, firstPositionInWorld.Value);
                        } else {
                            SelectedLayer.PaintLine(
                                firstPositionInWorld.Value, mousePositionInWorld,
                                SelectedColor, 1, (PaintCommand)ActiveCommand
                            );
                            firstPositionInWorld = mousePositionInWorld;
                        }
                    }
                } else if (EditorAction == EditorAction.PaintIdle) {
                
                    if (InputManager.IsKeyDown(Keys.LeftAlt)) {
                        if (InputManager.IsLeftButtonDown()) {
                            if (SelectedLayer.GetTile(mouseTilePosition) is not null) {
                                ColorChart.FetchPositionAndHueFromColor(SelectedLayer.GetTile(mouseTilePosition).GetColorAt(SelectedLayer.GetPositionInTile(mousePositionInWorld)));
                            }
                        }
                    } else {
                        if (InputManager.IsLeftButtonDown()) {
                            var color = SelectedColor;

                            if (InputManager.IsLeftButtonClicked()) {
                                ActiveCommand = new PaintCommand(SelectedAnimationFrame);
                                CommandManager.Push(ActiveCommand);

                                SelectedLayer.PaintCircle(
                                    mousePositionInWorld, 
                                    color, Math.Max(1, (int)(InputManager.PenPressure * 5)), 
                                    (PaintCommand)ActiveCommand
                                );
                            } else {
                                if (ActiveCommand is PaintCommand command) {
                                    SelectedLayer.PaintLine(
                                        previousMousePositionInWorld, mousePositionInWorld, 
                                        color, Math.Max(1, (int)(InputManager.PenPressure * 5)), 
                                        command
                                    );
                                }
                            }
                        }
                    }
                }
            } else if (EditorState == EditorState.TileMode) {

                if (InputManager.IsKeyDown(Keys.LeftAlt) && InputManager.IsKeyDown(Keys.LeftShift)) {
                    EditorAction = EditorAction.TileSelection;
                } else if (InputManager.IsKeyDown(Keys.LeftControl)) {
                    EditorAction = EditorAction.TileLine;
                } else if (InputManager.IsKeyDown(Keys.LeftShift)) {
                    EditorAction = EditorAction.TileRectangle;
                } else {
                    if (EditorAction == EditorAction.TileSelection) {
                        TilePattern = SelectedLayer.GetTiles(firstPositionInWorld.Value, mouseTilePosition);
                    }

                    EditorAction = EditorAction.TileIdle;
                    firstPositionInWorld = null;
                }

                if (EditorAction == EditorAction.TileSelection) {
                    if (!firstPositionInWorld.HasValue) {
                        firstPositionInWorld = mouseTilePosition;
                    }

                } if (EditorAction == EditorAction.TileRectangle) {
                    if (!firstPositionInWorld.HasValue) {
                        firstPositionInWorld = mouseTilePosition;
                    }

                    if (InputManager.IsLeftButtonClicked()) {
                        ActiveCommand = new SetCommand(SelectedLayer);
                        CommandManager.Push(ActiveCommand);

                        SelectedLayer.SetRectangle(
                            firstPositionInWorld.Value, mouseTilePosition,
                            TilePattern, Point.Zero, (SetCommand)ActiveCommand
                        );
                        firstPositionInWorld = mouseTilePosition;
                    }
                } else if (EditorAction == EditorAction.TileLine) {
                    if (!firstPositionInWorld.HasValue) {
                        firstPositionInWorld = mouseTilePosition;
                    }

                    if (InputManager.IsLeftButtonClicked()) {
                        ActiveCommand = new SetCommand(SelectedLayer);
                        CommandManager.Push(ActiveCommand);
                        if (InputManager.IsKeyDown(Keys.LeftShift)) {
                            SelectedLayer.SetLine(
                                firstPositionInWorld.Value, MathsHelper.AnchorPoint(mouseTilePosition, firstPositionInWorld.Value),
                                TilePattern, Point.Zero, 1, (SetCommand)ActiveCommand
                            );
                            tilePatternOrigin = MathsHelper.AnchorPoint(mouseTilePosition, firstPositionInWorld.Value) - firstPositionInWorld.Value;
                            firstPositionInWorld = MathsHelper.AnchorPoint(mousePositionInWorld, firstPositionInWorld.Value);
                        } else {
                            SelectedLayer.SetLine(
                                firstPositionInWorld.Value, mouseTilePosition,
                                TilePattern, Point.Zero, 1, (SetCommand)ActiveCommand
                            );
                            tilePatternOrigin = mouseTilePosition - firstPositionInWorld.Value;
                            firstPositionInWorld = mouseTilePosition;
                        }
                    }
                } else if (EditorAction == EditorAction.TileIdle) {
                
                    if (InputManager.IsKeyDown(Keys.LeftAlt)) {
                        if (InputManager.IsLeftButtonDown()) {
                            if (SelectedLayer.GetTile(mouseTilePosition) is not null) {
                                TilePattern = new Tile[1, 1];
                                TilePattern[0, 0] = SelectedLayer.GetTile(mouseTilePosition);
                            }
                        }
                    } else {
                        if (InputManager.IsLeftButtonDown()) {
                            if (InputManager.IsLeftButtonClicked()) {
                                ActiveCommand = new SetCommand(SelectedLayer);
                                CommandManager.Push(ActiveCommand);

                                tilePatternOrigin = mouseTilePosition;
                                SelectedLayer.SetCircle(
                                    mouseTilePosition,
                                    TilePattern, Math.Max(1, (int)(InputManager.PenPressure * 5)), Point.Zero,
                                    (SetCommand)ActiveCommand
                                );
                            } else {
                                if (ActiveCommand is SetCommand command) {
                                    SelectedLayer.SetLine(
                                        previousMouseTilePosition,
                                        mouseTilePosition,
                                        TilePattern, previousMouseTilePosition - tilePatternOrigin, Math.Max(1, (int)(5 * InputManager.PenPressure)),
                                        command
                                    );
                                }
                            }
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

        private void DrawPreviews() {
            switch (EditorAction) {
                case EditorAction.PaintIdle:
                    GameManager.SpriteBatch.DrawPoint(mousePositionInWorld.ToVector2(), SelectedColor * 0.5f);
                    break;
                case EditorAction.PaintRectangle:
                    GameManager.DrawFilledRectangle(MathsHelper.ValidizeRectangle(new Rectangle(firstPositionInWorld.Value, mousePositionInWorld - firstPositionInWorld.Value + new Point(1, 1))), SelectedColor * 0.5f);
                    break;
                case EditorAction.PaintLine:
                    if (InputManager.IsKeyDown(Keys.LeftShift)) {
                        GameManager.DrawPixelizedLine(firstPositionInWorld.Value, MathsHelper.AnchorPoint(mousePositionInWorld, firstPositionInWorld.Value), SelectedColor * 0.5f);
                    } else {
                        GameManager.DrawPixelizedLine(firstPositionInWorld.Value, mousePositionInWorld, SelectedColor * 0.5f);
                    }
                    break;
                case EditorAction.TileIdle:
                    if (!InputManager.IsLeftButtonDown())
                        SelectedLayer.SetCircle(mouseTilePosition, TilePattern, 1, Point.Zero, preview: true);
                    break;
                case EditorAction.TileRectangle:
                    SelectedLayer.SetRectangle(firstPositionInWorld.Value, mouseTilePosition, TilePattern, Point.Zero, preview: true);
                    break;
                case EditorAction.TileLine:
                    SelectedLayer.SetLine(firstPositionInWorld.Value, mouseTilePosition, TilePattern, Point.Zero, 1, preview: true);
                    break;
            }
        }

        public void DrawGrids() {
            Point tilePosition = mouseTilePosition;
            Point chunkPosition = SelectedLayer.GetChunkPositionOf(tilePosition);
            GameManager.SpriteBatch.DrawRectangle(
                Camera.ApplyTransform(
                    new Rectangle(
                        chunkPosition.X * Layer.ChunkLength * Layer.TileLength,
                        chunkPosition.Y * Layer.ChunkLength * Layer.TileLength,
                        Layer.ChunkLength * Layer.TileLength, Layer.ChunkLength * Layer.TileLength
                    )
                ), Color.White * 0.5f
            );
            
            if (EditorAction == EditorAction.TileSelection) {
                var rectangle = MathsHelper.ValidizeRectangle(new Rectangle(
                    tilePosition.X * Layer.TileLength,
                    tilePosition.Y * Layer.TileLength,
                    (firstPositionInWorld.Value.X - tilePosition.X) * Layer.TileLength,
                    (firstPositionInWorld.Value.Y - tilePosition.Y) * Layer.TileLength
                ));

                rectangle.Width += Layer.TileLength; rectangle.Height += Layer.TileLength;

                GameManager.SpriteBatch.DrawRectangle(Camera.ApplyTransform(rectangle), Color.Red * 0.5f);
            } else {
                GameManager.SpriteBatch.DrawRectangle(
                    Camera.ApplyTransform(
                        new Rectangle(
                            tilePosition.X * Layer.TileLength,
                            tilePosition.Y * Layer.TileLength,
                            Layer.TileLength, Layer.TileLength
                        )
                    ), Color.Red * 0.5f
                );
            }
        }

        public override void Draw() {
            GameManager.SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, transformMatrix: Camera.Transform);
            SelectedWorld?.Draw(Camera);
            DrawPreviews();
            GameManager.SpriteBatch.End();

            GameManager.SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp);
            DrawGrids();
            base.Draw();
            GameManager.SpriteBatch.End();
        }
    }
}
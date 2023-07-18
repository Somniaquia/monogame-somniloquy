namespace Somniloquy {
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Microsoft.Xna.Framework.Input;

    using MonoGame.Extended;
    using MonoGame.Extended.BitmapFonts;

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
    ///
    /// - PropertiesMode:
    /// - Ctrl + Left Mouse Button: Draw boundaries
    /// - Shift + Left Mouse Button: Draw rectangular boundaries (shortcut for above approach when creating simple rectangles)
    /// - Left Mouse Button: Select boundary
    /// - Alt + Left Mouse Button: Select all connecting boundaries of same type
    /// </summary>
    public class EditorScreen : Screen {
        public enum EditorState { PaintMode, TileMode, PropertiesMode }
        public enum EditorAction { PaintIdle, PaintRectangle, PaintLine, TileIdle, TileSelection, TileRectangle, TileLine, PropertiesIdle }

        public static EditorState CurrrentEditorState = EditorState.PaintMode;
        public static EditorAction CurrrentEditorAction = EditorAction.PaintIdle;
        public static bool Sync = false;

        public World LoadedWorld { get; set; } = new();
        public Layer SelectedLayer { get; set; } = null;
        public static Color SelectedColor { get; set; } = Color.AliceBlue;
        public static Tile[,] TilePattern { get; set; } = new Tile[1, 1] { { null } };
        public static ICommand ActiveCommand { get; set; } = null;
        public static int SelectedAnimationFrame { get; set; } = 0;
        
        public Camera Camera { get; set; } = new Camera(8.0f);
        public ColorChart ColorChart { get; private set; } = null;
        private Point previousWorldPosition;
        private Point mouseWorldPosition;
        private Point previousTilePosition;
        private Point mouseTilePosition;
        private Point previousPositionInTile;
        private Point mousePositionInTile;

        private Point tilePatternOrigin = Point.Zero;

        private Point? firstPositionInWorld = null;

        public EditorScreen(Rectangle boundaries, World loadedWorld = null) : base(boundaries) {
            ColorChart = new ColorChart(new Rectangle(boundaries.Width - 144, boundaries.Height - 144, 128, 128));
            ColorChart.UpdateChart();
            ChildScreens.Add(ColorChart);
            InputManager.Focus = this;

            if (loadedWorld is not null) LoadedWorld = loadedWorld;
        }

        public override void OnFocus() {
            if (LoadedWorld.Layers.Count == 0) SelectedLayer = LoadedWorld.NewLayer();
            SelectedLayer ??= LoadedWorld.Layers[^1];

            if (InputManager.IsKeyPressed(Keys.F1)) {
                CurrrentEditorState = EditorState.PaintMode;
                CurrrentEditorAction = EditorAction.PaintIdle;
            } else if (InputManager.IsKeyPressed(Keys.F2)) {
                CurrrentEditorState = EditorState.TileMode;
                CurrrentEditorAction = EditorAction.TileIdle;
            } else if (InputManager.IsKeyPressed(Keys.F3)) {
                CurrrentEditorState = EditorState.PropertiesMode;
                CurrrentEditorAction = EditorAction.PropertiesIdle;
            }

            previousWorldPosition = mouseWorldPosition;
            mouseWorldPosition = MathsHelper.ToPoint(Camera.ApplyInvertTransform(InputManager.GetMousePosition()));

            previousTilePosition = SelectedLayer.GetTilePositionOf(previousWorldPosition);
            mouseTilePosition = SelectedLayer.GetTilePositionOf(mouseWorldPosition);

            previousPositionInTile = SelectedLayer.GetPositionInTile(previousWorldPosition);
            mousePositionInTile = SelectedLayer.GetPositionInTile(mouseWorldPosition);

            if (InputManager.IsKeyDown(Keys.S)) Camera.Move(new Vector2(0, 1));
            if (InputManager.IsKeyDown(Keys.W)) Camera.Move(new Vector2(0, -1));
            if (InputManager.IsKeyDown(Keys.D)) Camera.Move(new Vector2(1, 0));
            if (InputManager.IsKeyDown(Keys.A)) Camera.Move(new Vector2(-1, 0));
            if (InputManager.IsKeyDown(Keys.Q)) Camera.Zoom(-0.1f);
            if (InputManager.IsKeyDown(Keys.E)) Camera.Zoom(0.1f);

            if (InputManager.GetNumberKeyPress() is not null) {
                if (InputManager.IsKeyDown(Keys.LeftControl) && InputManager.IsKeyPressed(Keys.Enter)) {
                    if (File.Exists($"Worlds/world{InputManager.GetNumberKeyPress()}.txt")) {
                        CommandManager.Clear();
                        LoadedWorld.Layers.Clear();
                        LoadedWorld.DisposeTiles();
                        LoadedWorld = SerializationManager.Deserialize<World>($"world{InputManager.GetNumberKeyPress()}.txt");
                        SelectedLayer = LoadedWorld.Layers[0];
                    }
                } else if (InputManager.IsKeyPressed(Keys.Enter)) {
                    SerializationManager.Serialize<World>(LoadedWorld, $"world{InputManager.GetNumberKeyPress()}.txt");
                }
            } else {
                if (InputManager.IsKeyPressed(Keys.Enter)) {
                    ScreenManager.ToGameScreen(this);
                }
            }

            if (InputManager.IsKeyPressed(Keys.Tab)) {
                Sync = !Sync;
            }

            if (InputManager.IsKeyDown(Keys.LeftControl) && InputManager.IsKeyPressed(Keys.N)) {
                SelectedLayer = LoadedWorld.NewLayer();
            }

            if (InputManager.IsKeyDown(Keys.LeftAlt)) {
                GetTopmostLayerBeneathMouse();
            }

            if (InputManager.IsKeyPressed(Keys.Delete)) {
                // TODO: This is not a proper implementation - work multi-layer support!!
                CommandManager.Clear();
                LoadedWorld.Layers.Clear();
                LoadedWorld.DisposeTiles();
                TilePattern = new Tile[1, 1] { { null } };
            }

            if (InputManager.IsKeyPressed(Keys.Back)) {
                CommandManager.Clear();
            }

            if (InputManager.IsKeyDown(Keys.LeftControl) && InputManager.IsKeyPressed(Keys.Z)) {
                if (!InputManager.IsKeyDown(Keys.LeftShift)) {
                    CommandManager.Undo();
                } else {
                    CommandManager.Redo();
                }
            }
        
            if (CurrrentEditorState == EditorState.PaintMode) {

                if (InputManager.IsKeyDown(Keys.LeftControl)) {
                    CurrrentEditorAction = EditorAction.PaintLine;
                } else if (InputManager.IsKeyDown(Keys.LeftShift)) {
                    CurrrentEditorAction = EditorAction.PaintRectangle;
                } else {
                    CurrrentEditorAction = EditorAction.PaintIdle;
                    firstPositionInWorld = null;
                }

                if (CurrrentEditorAction == EditorAction.PaintRectangle) {
                    if (!firstPositionInWorld.HasValue) {
                        firstPositionInWorld = mouseWorldPosition;
                    }

                    if (InputManager.IsLeftButtonClicked()) {
                        ActiveCommand = new PaintCommand(SelectedAnimationFrame);
                        CommandManager.Push(ActiveCommand);

                        SelectedLayer.PaintRectangle(
                            MathsHelper.ValidizeRectangle(new Rectangle(firstPositionInWorld.Value, previousWorldPosition - firstPositionInWorld.Value)),
                            SelectedColor, (PaintCommand)ActiveCommand, Sync
                        );
                        firstPositionInWorld = mouseWorldPosition;
                    }
                } else if (CurrrentEditorAction == EditorAction.PaintLine) {
                    if (!firstPositionInWorld.HasValue) {
                        firstPositionInWorld = mouseWorldPosition;
                    }

                    if (InputManager.IsLeftButtonClicked()) {
                        ActiveCommand = new PaintCommand(SelectedAnimationFrame);
                        CommandManager.Push(ActiveCommand);
                        if (InputManager.IsKeyDown(Keys.LeftShift)) {
                            SelectedLayer.PaintLine(
                                firstPositionInWorld.Value, MathsHelper.AnchorPoint(mouseWorldPosition, firstPositionInWorld.Value),
                                SelectedColor, 1, (PaintCommand)ActiveCommand, Sync
                            );
                            firstPositionInWorld = MathsHelper.AnchorPoint(mouseWorldPosition, firstPositionInWorld.Value);
                        } else {
                            SelectedLayer.PaintLine(
                                firstPositionInWorld.Value, mouseWorldPosition,
                                SelectedColor, 1, (PaintCommand)ActiveCommand, Sync
                            );
                            firstPositionInWorld = mouseWorldPosition;
                        }
                    }
                } else if (CurrrentEditorAction == EditorAction.PaintIdle) {
                
                    if (InputManager.IsKeyDown(Keys.LeftAlt)) {
                        if (InputManager.IsLeftButtonDown() && SelectedLayer.GetTile(mouseTilePosition) is not null) {
                            ColorChart.FetchPositionAndHueFromColor(SelectedLayer.GetTile(mouseTilePosition).GetColorAt(mousePositionInTile));
                        }
                    } else if (InputManager.IsKeyDown(Keys.F)) {
                        if (InputManager.IsLeftButtonClicked()) {
                            ActiveCommand = new PaintCommand(SelectedAnimationFrame);
                            CommandManager.Push(ActiveCommand);

                            SelectedLayer.PaintFill(mouseWorldPosition, SelectedColor, (PaintCommand)ActiveCommand, Sync);
                        }
                    } else {
                        if (InputManager.IsLeftButtonDown()) {
                            var color = SelectedColor;

                            if (InputManager.IsLeftButtonClicked()) {
                                ActiveCommand = new PaintCommand(SelectedAnimationFrame);
                                CommandManager.Push(ActiveCommand);

                                SelectedLayer.PaintCircle(
                                    mouseWorldPosition, 
                                    color, Math.Max(1, (int)InputManager.GetPenPressure()), 
                                    (PaintCommand)ActiveCommand, Sync
                                );
                            } else {
                                if (ActiveCommand is PaintCommand command) {
                                    SelectedLayer.PaintLine(
                                        previousWorldPosition, mouseWorldPosition, 
                                        color, Math.Max(1, (int)InputManager.GetPenPressure()), 
                                        command, Sync
                                    );
                                }
                            }
                        }
                    }
                }
            } else if (CurrrentEditorState == EditorState.TileMode) {

                if (InputManager.IsKeyDown(Keys.LeftAlt) && InputManager.IsKeyDown(Keys.LeftShift)) {
                    CurrrentEditorAction = EditorAction.TileSelection;
                } else if (InputManager.IsKeyDown(Keys.LeftControl)) {
                    CurrrentEditorAction = EditorAction.TileLine;
                } else if (InputManager.IsKeyDown(Keys.LeftShift)) {
                    CurrrentEditorAction = EditorAction.TileRectangle;
                } else {
                    if (CurrrentEditorAction == EditorAction.TileSelection) {
                        TilePattern = SelectedLayer.GetTiles(firstPositionInWorld.Value, mouseTilePosition);
                    }

                    CurrrentEditorAction = EditorAction.TileIdle;
                    firstPositionInWorld = null;
                }

                if (CurrrentEditorAction == EditorAction.TileSelection) {
                    if (!firstPositionInWorld.HasValue) {
                        firstPositionInWorld = mouseTilePosition;
                    }

                } if (CurrrentEditorAction == EditorAction.TileRectangle) {
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
                } else if (CurrrentEditorAction == EditorAction.TileLine) {
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
                            firstPositionInWorld = MathsHelper.AnchorPoint(mouseWorldPosition, firstPositionInWorld.Value);
                        } else {
                            SelectedLayer.SetLine(
                                firstPositionInWorld.Value, mouseTilePosition,
                                TilePattern, Point.Zero, 1, (SetCommand)ActiveCommand
                            );
                            tilePatternOrigin = mouseTilePosition - firstPositionInWorld.Value;
                            firstPositionInWorld = mouseTilePosition;
                        }
                    }
                } else if (CurrrentEditorAction == EditorAction.TileIdle) {

                    if (InputManager.IsKeyDown(Keys.LeftAlt)) {
                        if (InputManager.IsLeftButtonDown()) {
                            if (SelectedLayer.GetTile(mouseTilePosition) is not null) {
                                TilePattern = new Tile[1, 1];
                                TilePattern[0, 0] = SelectedLayer.GetTile(mouseTilePosition);
                            }
                        }
                    } else if (InputManager.IsKeyDown(Keys.F)) {
                        if (InputManager.IsLeftButtonClicked()) {
                            ActiveCommand = new SetCommand(SelectedLayer);
                            CommandManager.Push(ActiveCommand);

                            SelectedLayer.SetFill(mouseTilePosition, TilePattern, (SetCommand)ActiveCommand);
                        }
                    } else {
                        if (InputManager.IsLeftButtonDown()) {
                            if (InputManager.IsLeftButtonClicked()) {
                                ActiveCommand = new SetCommand(SelectedLayer);
                                CommandManager.Push(ActiveCommand);

                                tilePatternOrigin = mouseTilePosition;
                                SelectedLayer.SetCircle(
                                    mouseTilePosition,
                                    TilePattern, Math.Max(1, (int)InputManager.GetPenPressure()), Point.Zero,
                                    (SetCommand)ActiveCommand
                                );
                            } else {
                                if (ActiveCommand is SetCommand command) {
                                    SelectedLayer.SetLine(
                                        previousTilePosition,
                                        mouseTilePosition,
                                        TilePattern, previousTilePosition - tilePatternOrigin, Math.Max(1, (int)InputManager.GetPenPressure()),
                                        command
                                    );
                                }
                            }
                        }
                    }
                }
            } else if (CurrrentEditorState == EditorState.PropertiesMode) {
                var tile = SelectedLayer.GetTile(SelectedLayer.GetTilePositionOf(mouseWorldPosition));
                if (tile is null) return;
                
                if (InputManager.GetNumberKeyPress() is not null) {
                    if (InputManager.IsLeftButtonClicked()) {
                        if (tile.CollisionVertices.Length < InputManager.GetNumberKeyPress().Value) return;
                        tile.CollisionVertices[InputManager.GetNumberKeyPress().Value - 1] = mousePositionInTile;
                    }
                } else {
                    if (InputManager.IsLeftButtonClicked()) {
                        if (tile.CollisionVertices.Contains(mousePositionInTile)) return;
                        tile.CollisionVertices = tile.CollisionVertices.Concat(new[] { mousePositionInTile }).ToArray();
                    } else if (InputManager.IsRightButtonClicked()) {
                        if (tile.CollisionVertices.Length == 0) return;
                        var newArray = new Point[tile.CollisionVertices.Length - 1];
                        Array.Copy(tile.CollisionVertices, newArray, newArray.Length);
                        tile.CollisionVertices = newArray;
                    }
                }
            }
        }

        private void GetTopmostLayerBeneathMouse() {
            for (int i = LoadedWorld.Layers.Count - 1; i >= 0; i--) {
                var layer = LoadedWorld.Layers[i];
                if (layer.GetTile(mouseTilePosition) is null || layer.GetTile(mouseTilePosition).GetColorAt(mousePositionInTile) == Color.Transparent) {
                    continue;
                } else {
                    SelectedLayer = LoadedWorld.Layers[i];
                    break;
                }
            }
        }

        public override void Update() {
            Camera.UpdateTransformation();
            LoadedWorld?.Update();
            base.Update();
        }

        private void DrawPreviews() {
            switch (CurrrentEditorAction) {
                case EditorAction.PaintIdle:
                    GameManager.SpriteBatch.DrawPoint(mouseWorldPosition.ToVector2(), SelectedColor * 0.5f);
                    break;
                case EditorAction.PaintRectangle:
                    GameManager.DrawFilledRectangle(MathsHelper.ValidizeRectangle(new Rectangle(firstPositionInWorld.Value, mouseWorldPosition - firstPositionInWorld.Value + new Point(1, 1))), SelectedColor * 0.5f);
                    break;
                case EditorAction.PaintLine:
                    if (InputManager.IsKeyDown(Keys.LeftShift)) {
                        GameManager.DrawPixelizedLine(firstPositionInWorld.Value, MathsHelper.AnchorPoint(mouseWorldPosition, firstPositionInWorld.Value), SelectedColor * 0.5f);
                    } else {
                        GameManager.DrawPixelizedLine(firstPositionInWorld.Value, mouseWorldPosition, SelectedColor * 0.5f);
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
                case EditorAction.PropertiesIdle:
                    GameManager.SpriteBatch.DrawPoint(mouseWorldPosition.ToVector2(), Color.Blue * 0.5f);
                    break;
            }
        }

        public void DrawGrids() {
            Point tilePosition = mouseTilePosition;
            Point chunkPosition = SelectedLayer.GetChunkPositionOf(tilePosition);
            GameManager.SpriteBatch.DrawRectangle(
                Camera.ApplyTransform(
                    new Rectangle(
                        chunkPosition.X * SelectedLayer.ChunkLength * SelectedLayer.TileLength,
                        chunkPosition.Y * SelectedLayer.ChunkLength * SelectedLayer.TileLength,
                        SelectedLayer.ChunkLength * SelectedLayer.TileLength, SelectedLayer.ChunkLength * SelectedLayer.TileLength
                    )
                ), Color.White * 0.5f
            );
            
            if (CurrrentEditorAction == EditorAction.TileSelection) {
                var rectangle = MathsHelper.ValidizeRectangle(new Rectangle(
                    tilePosition.X * SelectedLayer.TileLength,
                    tilePosition.Y * SelectedLayer.TileLength,
                    (firstPositionInWorld.Value.X - tilePosition.X) * SelectedLayer.TileLength,
                    (firstPositionInWorld.Value.Y - tilePosition.Y) * SelectedLayer.TileLength
                ));

                rectangle.Width += SelectedLayer.TileLength; rectangle.Height += SelectedLayer.TileLength;

                GameManager.SpriteBatch.DrawRectangle(Camera.ApplyTransform(rectangle), Color.Red * 0.5f);
            } else {
                GameManager.SpriteBatch.DrawRectangle(
                    Camera.ApplyTransform(
                        new Rectangle(
                            tilePosition.X * SelectedLayer.TileLength,
                            tilePosition.Y * SelectedLayer.TileLength,
                            SelectedLayer.TileLength, SelectedLayer.TileLength
                        )
                    ), Color.Red * 0.5f
                );
            }
        }

        public override void Draw() {
            GameManager.SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, transformMatrix: Camera.Transform);
            foreach (var layer in LoadedWorld.Layers) {
                float opacity = ReferenceEquals(layer, SelectedLayer) ? 1f : 0.5f;
                bool drawCollisionBounds = CurrrentEditorState == EditorState.PropertiesMode;
                
                layer.Draw(Camera, opacity, drawCollisionBounds);
            }
            if (SelectedLayer is not null) DrawPreviews();

            GameManager.SpriteBatch.End();

            GameManager.SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp);
            if (SelectedLayer is not null) DrawGrids();
            base.Draw();
            GameManager.SpriteBatch.End();
        }
    }
}
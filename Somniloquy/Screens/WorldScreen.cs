namespace Somniloquy {
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Microsoft.Xna.Framework.Input;

    using MonoGame.Extended;

    public class WorldScreen : Screen {
        public World LoadedWorld { get; set; } = new();
        public EditorScreen EditorScreen { get; set; } = null;

        public Layer SelectedLayer { get; set; } = null;
        public Tile[,] TilePattern { get; set; } = new Tile[1, 1] { { null } };
        public Camera Camera { get; set; } = new Camera(8.0f);

        private Point previousWorldPosition;
        private Point mouseWorldPosition;
        private Point previousTilePosition;
        private Point mouseTilePosition;
        private Point previousPositionInTile;
        private Point mousePositionInTile;

        private Point? firstPositionInWorld = null;
        private Point tilePatternOrigin = Point.Zero;

        public WorldScreen(Rectangle boundaries, EditorScreen editorScreen = null) : base(boundaries) {
            InputManager.Focus = this;
            EditorScreen = editorScreen;
        }

        public override void Update() {
            Camera.UpdateTransformation();
            LoadedWorld?.Update();
            base.Update();
        }

        public override void OnFocus() {
            if (EditorScreen is not null) {
                EditModeFunctions();
            }
        }

        private void EditModeFunctions() {
            if (LoadedWorld.Layers.Count == 0) SelectedLayer = LoadedWorld.NewLayer();
            SelectedLayer ??= LoadedWorld.Layers[^1];

            previousWorldPosition = mouseWorldPosition;
            mouseWorldPosition = Utils.ToPoint(Camera.ApplyInvertTransform(InputManager.GetMousePosition()));

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
                        LoadedWorld = SerializationManager.Deserialize<World>($"world{InputManager.GetNumberKeyPress()}.txt");
                        SelectedLayer = LoadedWorld.Layers[0];
                    }
                } else if (InputManager.IsKeyPressed(Keys.Enter)) {
                    SerializationManager.Serialize<World>(LoadedWorld, $"world{InputManager.GetNumberKeyPress()}.txt");
                }
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
        
            if (EditorScreen.CurrentEditorState == EditorState.PaintMode) {

                if (InputManager.IsKeyDown(Keys.LeftControl)) {
                    EditorScreen.CurrentEditorAction = EditorAction.PaintLine;
                } else if (InputManager.IsKeyDown(Keys.LeftShift)) {
                    EditorScreen.CurrentEditorAction = EditorAction.PaintRectangle;
                } else {
                    EditorScreen.CurrentEditorAction = EditorAction.PaintIdle;
                    firstPositionInWorld = null;
                }

                if (EditorScreen.CurrentEditorAction == EditorAction.PaintRectangle) {
                    if (!firstPositionInWorld.HasValue) {
                        firstPositionInWorld = mouseWorldPosition;
                    }

                    if (InputManager.IsLeftButtonClicked()) {
                        EditorScreen.ActiveCommand = new PaintCommand(EditorScreen.SelectedAnimationFrame);
                        CommandManager.Push(EditorScreen.ActiveCommand);

                        SelectedLayer.PaintRectangle(
                            Utils.ValidizeRectangle(new Rectangle(firstPositionInWorld.Value, previousWorldPosition - firstPositionInWorld.Value)),
                            EditorScreen.SelectedColor, (PaintCommand)EditorScreen.ActiveCommand, EditorScreen.Sync
                        );
                        firstPositionInWorld = mouseWorldPosition;
                    }
                } else if (EditorScreen.CurrentEditorAction == EditorAction.PaintLine) {
                    if (!firstPositionInWorld.HasValue) {
                        firstPositionInWorld = mouseWorldPosition;
                    }

                    if (InputManager.IsLeftButtonClicked()) {
                        EditorScreen.ActiveCommand = new PaintCommand(EditorScreen.SelectedAnimationFrame);
                        CommandManager.Push(EditorScreen.ActiveCommand);
                        if (InputManager.IsKeyDown(Keys.LeftShift)) {
                            SelectedLayer.PaintLine(
                                firstPositionInWorld.Value, Utils.AnchorPoint(mouseWorldPosition, firstPositionInWorld.Value),
                                EditorScreen.SelectedColor, 1, (PaintCommand)EditorScreen.ActiveCommand, EditorScreen.Sync
                            );
                            firstPositionInWorld = Utils.AnchorPoint(mouseWorldPosition, firstPositionInWorld.Value);
                        } else {
                            SelectedLayer.PaintLine(
                                firstPositionInWorld.Value, mouseWorldPosition,
                                EditorScreen.SelectedColor, 1, (PaintCommand)EditorScreen.ActiveCommand, EditorScreen.Sync
                            );
                            firstPositionInWorld = mouseWorldPosition;
                        }
                    }
                } else if (EditorScreen.CurrentEditorAction == EditorAction.PaintIdle) {
                
                    if (InputManager.IsKeyDown(Keys.LeftAlt)) {
                        if (InputManager.IsLeftButtonDown() && SelectedLayer.GetTile(mouseTilePosition) is not null) {
                            EditorScreen.ColorChart.FetchPositionAndHueFromColor(SelectedLayer.GetTile(mouseTilePosition).GetColorAt(mousePositionInTile));
                        }
                    } else if (InputManager.IsKeyDown(Keys.F)) {
                        if (InputManager.IsLeftButtonClicked()) {
                            EditorScreen.ActiveCommand = new PaintCommand(EditorScreen.SelectedAnimationFrame);
                            CommandManager.Push(EditorScreen.ActiveCommand);

                            SelectedLayer.PaintFill(mouseWorldPosition, EditorScreen.SelectedColor, (PaintCommand)EditorScreen.ActiveCommand, EditorScreen.Sync);
                        }
                    } else {
                        if (InputManager.IsLeftButtonDown()) {
                            var color = EditorScreen.SelectedColor;

                            if (InputManager.IsLeftButtonClicked()) {
                                EditorScreen.ActiveCommand = new PaintCommand(EditorScreen.SelectedAnimationFrame);
                                CommandManager.Push(EditorScreen.ActiveCommand);

                                SelectedLayer.PaintCircle(
                                    mouseWorldPosition, 
                                    color, Math.Max(1, (int)InputManager.GetPenPressure()), 
                                    (PaintCommand)EditorScreen.ActiveCommand, EditorScreen.Sync
                                );
                            } else {
                                if (EditorScreen.ActiveCommand is PaintCommand command) {
                                    SelectedLayer.PaintLine(
                                        previousWorldPosition, mouseWorldPosition, 
                                        color, Math.Max(1, (int)InputManager.GetPenPressure()), 
                                        command, EditorScreen.Sync
                                    );
                                }
                            }
                        }
                    }
                }
            } else if (EditorScreen.CurrentEditorState == EditorState.TileMode) {

                if (InputManager.IsKeyDown(Keys.LeftAlt) && InputManager.IsKeyDown(Keys.LeftShift)) {
                    EditorScreen.CurrentEditorAction = EditorAction.TileSelection;
                } else if (InputManager.IsKeyDown(Keys.LeftControl)) {
                    EditorScreen.CurrentEditorAction = EditorAction.TileLine;
                } else if (InputManager.IsKeyDown(Keys.LeftShift)) {
                    EditorScreen.CurrentEditorAction = EditorAction.TileRectangle;
                } else {
                    if (EditorScreen.CurrentEditorAction == EditorAction.TileSelection) {
                        TilePattern = SelectedLayer.GetTiles(firstPositionInWorld.Value, mouseTilePosition);
                    }

                    EditorScreen.CurrentEditorAction = EditorAction.TileIdle;
                    firstPositionInWorld = null;
                }

                if (EditorScreen.CurrentEditorAction == EditorAction.TileSelection) {
                    if (!firstPositionInWorld.HasValue) {
                        firstPositionInWorld = mouseTilePosition;
                    }

                } if (EditorScreen.CurrentEditorAction == EditorAction.TileRectangle) {
                    if (!firstPositionInWorld.HasValue) {
                        firstPositionInWorld = mouseTilePosition;
                    }

                    if (InputManager.IsLeftButtonClicked()) {
                        EditorScreen.ActiveCommand = new SetCommand(SelectedLayer);
                        CommandManager.Push(EditorScreen.ActiveCommand);

                        SelectedLayer.SetRectangle(
                            firstPositionInWorld.Value, mouseTilePosition,
                            TilePattern, Point.Zero, (SetCommand)EditorScreen.ActiveCommand
                        );
                        firstPositionInWorld = mouseTilePosition;
                    }
                } else if (EditorScreen.CurrentEditorAction == EditorAction.TileLine) {
                    if (!firstPositionInWorld.HasValue) {
                        firstPositionInWorld = mouseTilePosition;
                    }

                    if (InputManager.IsLeftButtonClicked()) {
                        EditorScreen.ActiveCommand = new SetCommand(SelectedLayer);
                        CommandManager.Push(EditorScreen.ActiveCommand);
                        if (InputManager.IsKeyDown(Keys.LeftShift)) {
                            SelectedLayer.SetLine(
                                firstPositionInWorld.Value, Utils.AnchorPoint(mouseTilePosition, firstPositionInWorld.Value),
                                TilePattern, Point.Zero, 1, (SetCommand)EditorScreen.ActiveCommand
                            );
                            tilePatternOrigin = Utils.AnchorPoint(mouseTilePosition, firstPositionInWorld.Value) - firstPositionInWorld.Value;
                            firstPositionInWorld = Utils.AnchorPoint(mouseWorldPosition, firstPositionInWorld.Value);
                        } else {
                            SelectedLayer.SetLine(
                                firstPositionInWorld.Value, mouseTilePosition,
                                TilePattern, Point.Zero, 1, (SetCommand)EditorScreen.ActiveCommand
                            );
                            tilePatternOrigin = mouseTilePosition - firstPositionInWorld.Value;
                            firstPositionInWorld = mouseTilePosition;
                        }
                    }
                } else if (EditorScreen.CurrentEditorAction == EditorAction.TileIdle) {

                    if (InputManager.IsKeyDown(Keys.LeftAlt)) {
                        if (InputManager.IsLeftButtonDown()) {
                            if (SelectedLayer.GetTile(mouseTilePosition) is not null) {
                                TilePattern = new Tile[1, 1];
                                TilePattern[0, 0] = SelectedLayer.GetTile(mouseTilePosition);
                            }
                        }
                    } else if (InputManager.IsKeyDown(Keys.F)) {
                        if (InputManager.IsLeftButtonClicked()) {
                            EditorScreen.ActiveCommand = new SetCommand(SelectedLayer);
                            CommandManager.Push(EditorScreen.ActiveCommand);

                            SelectedLayer.SetFill(mouseTilePosition, TilePattern, (SetCommand)EditorScreen.ActiveCommand);
                        }
                    } else {
                        if (InputManager.IsLeftButtonDown()) {
                            if (InputManager.IsLeftButtonClicked()) {
                                EditorScreen.ActiveCommand = new SetCommand(SelectedLayer);
                                CommandManager.Push(EditorScreen.ActiveCommand);

                                tilePatternOrigin = mouseTilePosition;
                                SelectedLayer.SetCircle(
                                    mouseTilePosition,
                                    TilePattern, Math.Max(1, (int)InputManager.GetPenPressure()), Point.Zero,
                                    (SetCommand)EditorScreen.ActiveCommand
                                );
                            } else {
                                if (EditorScreen.ActiveCommand is SetCommand command) {
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
            } else if (EditorScreen.CurrentEditorState == EditorState.PropertiesMode) {
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

        private void DrawPreviews() {
            switch (EditorScreen.CurrentEditorAction) {
                case EditorAction.PaintIdle:
                    GameManager.SpriteBatch.DrawPoint(mouseWorldPosition.ToVector2(), EditorScreen.SelectedColor * 0.5f);
                    break;
                case EditorAction.PaintRectangle:
                    GameManager.DrawFilledRectangle(Utils.ValidizeRectangle(new Rectangle(firstPositionInWorld.Value, mouseWorldPosition - firstPositionInWorld.Value + new Point(1, 1))), EditorScreen.SelectedColor * 0.5f);
                    break;
                case EditorAction.PaintLine:
                    if (InputManager.IsKeyDown(Keys.LeftShift)) {
                        GameManager.DrawPixelizedLine(firstPositionInWorld.Value, Utils.AnchorPoint(mouseWorldPosition, firstPositionInWorld.Value), EditorScreen.SelectedColor * 0.5f);
                    } else {
                        GameManager.DrawPixelizedLine(firstPositionInWorld.Value, mouseWorldPosition, EditorScreen.SelectedColor * 0.5f);
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
            
            if (EditorScreen.CurrentEditorAction == EditorAction.TileSelection) {
                var rectangle = Utils.ValidizeRectangle(new Rectangle(
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
                bool drawCollisionBounds = EditorScreen.CurrentEditorState == EditorState.PropertiesMode;
                
                layer.Draw(Camera, opacity, drawCollisionBounds);
            }
            if (SelectedLayer is not null && InputManager.Focus == this) DrawPreviews();

            GameManager.SpriteBatch.End();

            GameManager.SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp);
            // TODO: Move layer properties (tile size, chunk size) to static
            if (SelectedLayer is not null && InputManager.Focus == this) DrawGrids();
            base.Draw();
            GameManager.SpriteBatch.End();
        }
    }
}
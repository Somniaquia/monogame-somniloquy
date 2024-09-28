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
        public EditorScreen EditorScreen { get; set; } = null;

        public TileLayer2D SelectedLayer { get; set; } = null;
        public Tile[,] TilePattern { get; set; } = new Tile[1, 1] { { null } };
        public Camera2D Camera { get; set; } = new Camera2D(8.0f);

        private Point previousWorldPosition;
        private Point mouseWorldPosition;
        private Point previousTilePosition;
        private Point mouseTilePosition;
        private Point previousPositionInTile;
        private Point mousePositionInTile;

        private Point? firstPositionInWorld = null;
        private Point tilePatternOrigin = Point.Zero;

        public WorldScreen(Rectangle boundaries, EditorScreen editorScreen = null) : base(boundaries) {
            InputManager.FocusedScreen = this;
            EditorScreen = editorScreen;
        }

        public override void Update() {
            Camera.UpdateTransformation();
            EditorScreen.LoadedWorld?.Update();
            base.Update();
        }

        public override void OnFocus() {
            if (EditorScreen is not null) {
                EditModeFunctions();
            }
        }

        private void EditModeFunctions() {
            if (EditorScreen.LoadedWorld.Layers.Count == 0) SelectedLayer = EditorScreen.LoadedWorld.NewLayer();
            SelectedLayer ??= EditorScreen.LoadedWorld.Layers[^1];

            previousWorldPosition = mouseWorldPosition;
            mouseWorldPosition = Utils.ToPoint(Camera.ApplyInvertTransform(InputManager.GetMousePosition()));

            previousTilePosition = SelectedLayer.GetTilePositionOf(previousWorldPosition);
            mouseTilePosition = SelectedLayer.GetTilePositionOf(mouseWorldPosition);

            previousPositionInTile = SelectedLayer.GetPositionInTile(previousWorldPosition);
            mousePositionInTile = SelectedLayer.GetPositionInTile(mouseWorldPosition);

            if (InputManager.IsKeyDown(Keys.S)) Camera.MoveCamera(new Vector2(0, 1));
            if (InputManager.IsKeyDown(Keys.W)) Camera.MoveCamera(new Vector2(0, -1));
            if (InputManager.IsKeyDown(Keys.D)) Camera.MoveCamera(new Vector2(1, 0));
            if (InputManager.IsKeyDown(Keys.A)) Camera.MoveCamera(new Vector2(-1, 0));
            if (InputManager.IsKeyDown(Keys.Q)) Camera.ZoomCamera(-0.1f);
            if (InputManager.IsKeyDown(Keys.E)) Camera.ZoomCamera(0.1f);

            if (InputManager.IsKeyDown(Keys.LeftControl) && InputManager.IsKeyPressed(Keys.N)) {
                SelectedLayer = EditorScreen.LoadedWorld.NewLayer();
            }

            if (InputManager.IsKeyDown(Keys.LeftAlt)) {
                GetTopmostLayerBeneathMouse();
            }

            if (InputManager.IsKeyPressed(Keys.Delete)) {
                // TODO: This is not a proper implementation - work multi-layer support!!
                CommandManager.Clear();
                EditorScreen.LoadedWorld = new World();
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
                PaintModeFunctions();
            } else if (EditorScreen.CurrentEditorState == EditorState.TileMode) {
                TileModeFunctions();
            } else if (EditorScreen.CurrentEditorState == EditorState.PropertiesMode) {
                PropertiesModeFunctions();
            }
        }

        private void PropertiesModeFunctions() {
            // var tile = SelectedLayer.GetTile(SelectedLayer.GetTilePositionOf(mouseWorldPosition));
            // if (tile is null) return;

            // if (InputManager.GetNumberKeyPress() is not null) {
            //     if (InputManager.IsMouseButtonPressed(MouseButtons.LeftButton)) {
            //         if (tile.CollisionVertices.Length < InputManager.GetNumberKeyPress().Value) return;
            //         tile.CollisionVertices[InputManager.GetNumberKeyPress().Value - 1] = mousePositionInTile;
            //     }
            // }
            // else {
            //     if (InputManager.IsMouseButtonPressed(MouseButtons.LeftButton)) {
            //         if (tile.CollisionVertices.Contains(mousePositionInTile)) return;
            //         tile.CollisionVertices = tile.CollisionVertices.Concat(new[] { mousePositionInTile }).ToArray();
            //     }
            //     else if (InputManager.IsRightButtonClicked()) {
            //         if (tile.CollisionVertices.Length == 0) return;
            //         var newArray = new Point[tile.CollisionVertices.Length - 1];
            //         Array.Copy(tile.CollisionVertices, newArray, newArray.Length);
            //         tile.CollisionVertices = newArray;
            //     }
            // }
        }

        private void TileModeFunctions() {
            var tilePattern = InputManager.IsKeyDown(Keys.Z) ? new Tile[1, 1] { { null } } : TilePattern;

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

                if (InputManager.IsMouseButtonPressed(MouseButtons.LeftButton)) {
                    EditorScreen.ActiveCommand = new WorldEditCommand();
                    CommandManager.Push(EditorScreen.ActiveCommand);

                    SelectedLayer.SetRectangle(
                        firstPositionInWorld.Value, mouseTilePosition,
                        tilePattern, EditorScreen.TileAction, Point.Zero, EditorScreen.ActiveCommand
                    );
                    firstPositionInWorld = mouseTilePosition;
                }
            }
            else if (EditorScreen.CurrentEditorAction == EditorAction.TileLine) {
                if (!firstPositionInWorld.HasValue) {
                    firstPositionInWorld = mouseTilePosition;
                }

                if (InputManager.IsMouseButtonPressed(MouseButtons.LeftButton)) {
                    EditorScreen.ActiveCommand = new WorldEditCommand();
                    CommandManager.Push(EditorScreen.ActiveCommand);
                    if (InputManager.IsKeyDown(Keys.LeftShift)) {
                        SelectedLayer.SetLine(
                            firstPositionInWorld.Value, Utils.AnchorPoint(mouseTilePosition, firstPositionInWorld.Value),
                            tilePattern, EditorScreen.TileAction, Point.Zero, 1, EditorScreen.ActiveCommand
                        );
                        tilePatternOrigin = Utils.AnchorPoint(mouseTilePosition, firstPositionInWorld.Value) - firstPositionInWorld.Value;
                        firstPositionInWorld = Utils.AnchorPoint(mouseWorldPosition, firstPositionInWorld.Value);
                    }
                    else {
                        SelectedLayer.SetLine(
                            firstPositionInWorld.Value, mouseTilePosition,
                            tilePattern, EditorScreen.TileAction, Point.Zero, 1, EditorScreen.ActiveCommand
                        );
                        tilePatternOrigin = mouseTilePosition - firstPositionInWorld.Value;
                        firstPositionInWorld = mouseTilePosition;
                    }
                }
            }
            else if (EditorScreen.CurrentEditorAction == EditorAction.TileIdle) {

                if (InputManager.IsKeyDown(Keys.LeftAlt)) {
                    if (InputManager.IsMouseButtonDown(MouseButtons.LeftButton)) {
                        if (SelectedLayer.GetTile(mouseTilePosition) is not null) {
                            TilePattern = new Tile[1, 1];
                            TilePattern[0, 0] = SelectedLayer.GetTile(mouseTilePosition);
                        }
                    }
                }
                else if (InputManager.IsKeyDown(Keys.F)) {
                    if (InputManager.IsMouseButtonPressed(MouseButtons.LeftButton)) {
                        EditorScreen.ActiveCommand = new WorldEditCommand(); ;
                        CommandManager.Push(EditorScreen.ActiveCommand);

                        SelectedLayer.SetFill(mouseTilePosition, tilePattern, EditorScreen.TileAction, EditorScreen.ActiveCommand);
                    }
                }
                else {
                    if (InputManager.IsMouseButtonDown(MouseButtons.LeftButton)) {
                        if (InputManager.IsMouseButtonPressed(MouseButtons.LeftButton)) {
                            EditorScreen.ActiveCommand = new WorldEditCommand();
                            CommandManager.Push(EditorScreen.ActiveCommand);

                            tilePatternOrigin = mouseTilePosition;
                            SelectedLayer.SetCircle(
                                mouseTilePosition,
                                tilePattern, EditorScreen.TileAction, Math.Max(1, (int)InputManager.GetPenPressure()), Point.Zero,
                                EditorScreen.ActiveCommand
                            );
                        }
                        else {
                            SelectedLayer.SetLine(
                                previousTilePosition,
                                mouseTilePosition,
                                tilePattern, EditorScreen.TileAction, previousTilePosition - tilePatternOrigin, Math.Max(1, (int)InputManager.GetPenPressure()),
                                EditorScreen.ActiveCommand
                            );
                        }
                    }
                }
            }
        }

        private void PaintModeFunctions() {
            var color = InputManager.IsKeyDown(Keys.Z) ? Color.Transparent : EditorScreen.SelectedColor;

            if (InputManager.IsKeyDown(Keys.LeftControl)) {
                EditorScreen.CurrentEditorAction = EditorAction.PaintLine;
            } else if (InputManager.IsKeyDown(Keys.LeftShift)) {
                EditorScreen.CurrentEditorAction = EditorAction.PaintRectangle;
            }
            else {
                EditorScreen.CurrentEditorAction = EditorAction.PaintIdle;
                firstPositionInWorld = null;
            }

            if (EditorScreen.CurrentEditorAction == EditorAction.PaintRectangle) {
                if (!firstPositionInWorld.HasValue) {
                    firstPositionInWorld = mouseWorldPosition;
                }

                if (InputManager.IsMouseButtonPressed(MouseButtons.LeftButton)) {
                    EditorScreen.ActiveCommand = new WorldEditCommand();
                    CommandManager.Push(EditorScreen.ActiveCommand);

                    SelectedLayer.PaintRectangle(
                        Utils.ValidizeRectangle(new Rectangle(firstPositionInWorld.Value, previousWorldPosition - firstPositionInWorld.Value)),
                        color, EditorScreen.ActiveCommand, EditorScreen.Sync
                    );
                    firstPositionInWorld = mouseWorldPosition;
                }
            }
            else if (EditorScreen.CurrentEditorAction == EditorAction.PaintLine) {
                if (!firstPositionInWorld.HasValue) {
                    firstPositionInWorld = mouseWorldPosition;
                }

                if (InputManager.IsMouseButtonPressed(MouseButtons.LeftButton)) {
                    EditorScreen.ActiveCommand = new WorldEditCommand();
                    CommandManager.Push(EditorScreen.ActiveCommand);
                    if (InputManager.IsKeyDown(Keys.LeftShift)) {
                        SelectedLayer.PaintLine(
                            firstPositionInWorld.Value, Utils.AnchorPoint(mouseWorldPosition, firstPositionInWorld.Value),
                            color, 1, EditorScreen.ActiveCommand, EditorScreen.Sync
                        );
                        firstPositionInWorld = Utils.AnchorPoint(mouseWorldPosition, firstPositionInWorld.Value);
                    } else {
                        SelectedLayer.PaintLine(
                            firstPositionInWorld.Value, mouseWorldPosition,
                            color, 1, EditorScreen.ActiveCommand, EditorScreen.Sync
                        );
                        firstPositionInWorld = mouseWorldPosition;
                    }
                }
            }
            else if (EditorScreen.CurrentEditorAction == EditorAction.PaintIdle) {

                if (InputManager.IsKeyDown(Keys.LeftAlt)) {
                    if (InputManager.IsMouseButtonDown(MouseButtons.LeftButton) && SelectedLayer.GetTile(mouseTilePosition) is not null) {
                        EditorScreen.ColorChart.FetchPositionAndHueFromColor(SelectedLayer.GetTile(mouseTilePosition).GetColorAt(mousePositionInTile));
                    }
                } else if (InputManager.IsKeyDown(Keys.F)) {
                    if (InputManager.IsMouseButtonPressed(MouseButtons.LeftButton)) {
                        EditorScreen.ActiveCommand = new WorldEditCommand();
                        CommandManager.Push(EditorScreen.ActiveCommand);

                        SelectedLayer.PaintFill(mouseWorldPosition, color, EditorScreen.ActiveCommand, EditorScreen.Sync);
                    }
                } else {
                    if (InputManager.IsMouseButtonDown(MouseButtons.LeftButton)) {
                        if (InputManager.IsMouseButtonPressed(MouseButtons.LeftButton)) {
                            EditorScreen.ActiveCommand = new WorldEditCommand();
                            CommandManager.Push(EditorScreen.ActiveCommand);

                            SelectedLayer.PaintCircle(
                                mouseWorldPosition,
                                color, Math.Max(1, (int)InputManager.GetPenPressure()),
                                EditorScreen.ActiveCommand, EditorScreen.Sync
                            );
                        }
                        else {
                            SelectedLayer.PaintLine(
                                previousWorldPosition, mouseWorldPosition,
                                color, Math.Max(1, (int)InputManager.GetPenPressure()),
                                EditorScreen.ActiveCommand, EditorScreen.Sync
                            );
                        }
                    }
                }
            }
        }

        public void LoadToWorld(Texture2D image) {
            EditorScreen.ActiveCommand = new WorldEditCommand();
            CommandManager.Push(EditorScreen.ActiveCommand);

            int sheetWidth = image.Width / TileLayer2D.TileLength;
            int sheetHeight = image.Height / TileLayer2D.TileLength;

            for (int y = 0; y < sheetHeight; y++) {
                for (int x = 0; x < sheetWidth; x++) {
                    var margin = new Rectangle(x * TileLayer2D.TileLength, y * TileLayer2D.TileLength, TileLayer2D.TileLength, TileLayer2D.TileLength);
                    Color[] retrievedColors = new Color[margin.Width * margin.Height];
                    image.GetData(0, margin, retrievedColors, 0, retrievedColors.Length);

                    var colors = Utils.ToNullableColors(Utils.ConvertTo2D(retrievedColors, TileLayer2D.TileLength));

                    int frame = EditorScreen.LoadedWorld.SpriteSheet.NewFrame();
                    EditorScreen.LoadedWorld.SpriteSheet.PaintOnFrame(colors, frame);

                    var tile = EditorScreen.LoadedWorld.NewTile(false);

                    SelectedLayer.SetTile(mouseTilePosition + new Point(x, y), tile, EditorScreen.ActiveCommand);
                }
            }
        }

        private void GetTopmostLayerBeneathMouse() {
            for (int i = EditorScreen.LoadedWorld.Layers.Count - 1; i >= 0; i--) {
                var layer = EditorScreen.LoadedWorld.Layers[i];
                if (layer.GetTile(mouseTilePosition) is null || layer.GetTile(mouseTilePosition).GetColorAt(mousePositionInTile) == Color.Transparent) {
                    continue;
                } else {
                    SelectedLayer = EditorScreen.LoadedWorld.Layers[i];
                    break;
                }
            }
        }

        private void DrawPreviews() {
            switch (EditorScreen.CurrentEditorAction) {
                case EditorAction.PaintIdle:
                    SQ.SB.DrawPoint(mouseWorldPosition.ToVector2(), EditorScreen.SelectedColor * 0.5f);
                    break;
                case EditorAction.PaintRectangle:
                    SQ.SB.DrawFilledRectangle(Utils.ValidizeRectangle(new Rectangle(firstPositionInWorld.Value, mouseWorldPosition - firstPositionInWorld.Value + new Point(1, 1))), EditorScreen.SelectedColor * 0.5f);
                    break;
                case EditorAction.PaintLine:
                    if (InputManager.IsKeyDown(Keys.LeftShift)) {
                        SQ.SB.DrawPixelizedLine(firstPositionInWorld.Value, Utils.AnchorPoint(mouseWorldPosition, firstPositionInWorld.Value), EditorScreen.SelectedColor * 0.5f);
                    } else {
                        SQ.SB.DrawPixelizedLine(firstPositionInWorld.Value, mouseWorldPosition, EditorScreen.SelectedColor * 0.5f);
                    }
                    break;
                case EditorAction.TileIdle:
                    if (!InputManager.IsMouseButtonDown(MouseButtons.LeftButton))
                        SelectedLayer.SetCircle(mouseTilePosition, TilePattern, EditorScreen.TileAction, 1, Point.Zero, preview: true);
                    break;
                case EditorAction.TileRectangle:
                    SelectedLayer.SetRectangle(firstPositionInWorld.Value, mouseTilePosition, TilePattern, EditorScreen.TileAction, Point.Zero, preview: true);
                    break;
                case EditorAction.TileLine:
                    SelectedLayer.SetLine(firstPositionInWorld.Value, mouseTilePosition, TilePattern, EditorScreen.TileAction, Point.Zero, 1, preview: true);
                    break;
                case EditorAction.PropertiesIdle:
                    SQ.SB.DrawPoint(mouseWorldPosition.ToVector2(), Color.Blue * 0.5f);
                    break;
            }
        }

        public void DrawGrids() {
            Point tilePosition = mouseTilePosition;

            var pair = Camera.GetCameraBounds();
            Point topLeft = SelectedLayer.GetTilePositionOf(Utils.ToPoint(pair.Item1));
            Point bottomRight = SelectedLayer.GetTilePositionOf(Utils.ToPoint(pair.Item2));

            for (int x = topLeft.X; x <= bottomRight.X; x++) {
                var xPos = Camera.ApplyTransform(new Vector2(x * TileLayer2D.TileLength, 0)).X;
                SQ.SB.DrawLine(xPos, 0, xPos, SQ.WindowSize.Y, Color.Black * 0.5f);
            }

            for (int y = topLeft.Y; y <= bottomRight.Y; y++) {
                var yPos = Camera.ApplyTransform(new Vector2(0, y * TileLayer2D.TileLength)).Y;
                SQ.SB.DrawLine(0, yPos, SQ.WindowSize.X, yPos, Color.Black * 0.5f);
            }
            if (InputManager.FocusedScreen == this) {
                if (EditorScreen.CurrentEditorAction == EditorAction.TileSelection) {
                    var rectangle = Utils.ValidizeRectangle(new Rectangle(
                        tilePosition.X * TileLayer2D.TileLength,
                        tilePosition.Y * TileLayer2D.TileLength,
                        (firstPositionInWorld.Value.X - tilePosition.X) * TileLayer2D.TileLength,
                        (firstPositionInWorld.Value.Y - tilePosition.Y) * TileLayer2D.TileLength
                    ));

                    rectangle.Width += TileLayer2D.TileLength; rectangle.Height += TileLayer2D.TileLength;

                    SQ.SB.DrawRectangle(Camera.ApplyTransform(rectangle), Color.Red * 0.5f);
                } else {
                    SQ.SB.DrawRectangle(
                        Camera.ApplyTransform(
                            new Rectangle(
                                tilePosition.X * TileLayer2D.TileLength,
                                tilePosition.Y * TileLayer2D.TileLength,
                                TileLayer2D.TileLength, TileLayer2D.TileLength
                            )
                        ), Color.Red * 0.5f
                    );
                }
            } 
        }

        public override void Draw() {
            SQ.SB.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, transformMatrix: Camera.Transform);
            foreach (var layer in EditorScreen.LoadedWorld.Layers) {
                float opacity = ReferenceEquals(layer, SelectedLayer) ? 1f : 0.5f;
                layer.Draw(Camera, opacity);
            }
            if (SelectedLayer is not null && InputManager.FocusedScreen == this) DrawPreviews();

            SQ.SB.End();

            SQ.SB.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);

            if (SelectedLayer is not null) DrawGrids();
            
            if (EditorScreen.CurrentEditorState == EditorState.PropertiesMode) {
                foreach (var layer in EditorScreen.LoadedWorld.Layers) {
                    float opacity = ReferenceEquals(layer, SelectedLayer) ? 1f : 0.5f;
                    layer.DrawCollisionBoundaries(Camera, opacity);
                }
            }
            base.Draw();

            SQ.SB.DrawString(SQ.Misaki, EditorScreen.CurrentEditorAction.ToString(), Vector2.Zero, Color.AliceBlue);
            
            if (EditorScreen.CurrentEditorState == EditorState.PaintMode) {
                SQ.SB.DrawString(SQ.Misaki, "Sync identical tiles: " + EditorScreen.Sync.ToString(), new Vector2(0, 16), Color.AliceBlue);
            } else if (EditorScreen.CurrentEditorState == EditorState.TileMode) {
                SQ.SB.DrawString(SQ.Misaki, "Tile place mode: " + EditorScreen.TileAction.ToString(), new Vector2(0, 16), Color.AliceBlue);
            }

            SQ.SB.End();
        }
    }
}
namespace Somniloquy {
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Microsoft.Xna.Framework.Input;

    public enum EditorMode { PaintMode, TileMode, ScriptingMode }
    public enum PaintModeState { Idle, Rectangle, Line, Select }

    public class Section2DEditor : BoxUI {
        public Section2DScreen Screen;
        public EditorMode EditorMode = EditorMode.PaintMode;

        public ColorPicker ColorPicker;
        public Color SelectedColor = Color.White;
        public Layer2D SelectedLayer;

        private int currentBrushIndex;
        public Brush Brush => Brush.BrushTypes[currentBrushIndex];

        public PaintModeState PaintModeState = PaintModeState.Idle;
        public Vector2? PreviousGlobalMousePos;

        public List<Keybind> GlobalKeybinds = new();

        public Section2DEditor(Section2DScreen screen) : base() {
            Screen = screen;
            Boundaries = screen.Boundaries;

            GlobalKeybinds.Add(InputManager.RegisterKeybind(Keys.W, _ => MoveScreen(new Vector2(0, -1)), TriggerOnce.False));
            GlobalKeybinds.Add(InputManager.RegisterKeybind(Keys.A, _ => MoveScreen(new Vector2(-1, 0)), TriggerOnce.False));
			GlobalKeybinds.Add(InputManager.RegisterKeybind(Keys.S, _ => MoveScreen(new Vector2(0, 1)), TriggerOnce.False));
            GlobalKeybinds.Add(InputManager.RegisterKeybind(Keys.D, _ => MoveScreen(new Vector2(1, 0)), TriggerOnce.False));

            GlobalKeybinds.Add(InputManager.RegisterKeybind(Keys.Q, _ => ZoomScreen(-0.05f), TriggerOnce.False));
            GlobalKeybinds.Add(InputManager.RegisterKeybind(Keys.E, _ => ZoomScreen(0.05f), TriggerOnce.False));
            GlobalKeybinds.Add(InputManager.RegisterKeybind(Keys.OemPipe, _ => Screen.Camera.TargetRotation = 0, TriggerOnce.True));
            GlobalKeybinds.Add(InputManager.RegisterKeybind(Keys.OemOpenBrackets, _ => RotateScreen(-0.05f), TriggerOnce.False));
            GlobalKeybinds.Add(InputManager.RegisterKeybind(Keys.OemCloseBrackets, _ => RotateScreen(0.05f), TriggerOnce.False));
            
            GlobalKeybinds.Add(InputManager.RegisterKeybind(Keys.LeftAlt, _ => SelectLayerUnderMouse(), TriggerOnce.False));
            GlobalKeybinds.Add(InputManager.RegisterKeybind(new object[] { Keys.LeftShift }, new object[] { Keys.LeftControl }, _ => DrawRectangle(), _ => PostRectangle(), TriggerOnce.False));
            GlobalKeybinds.Add(InputManager.RegisterKeybind(new object[] { Keys.LeftControl}, new object[] { }, _ => DrawLine(), _ => PostLine(), TriggerOnce.False));

            GlobalKeybinds.Add(InputManager.RegisterKeybind(new object[] { MouseButtons.LeftButton }, new object[] { Keys.LeftShift, Keys.LeftControl }, _ => HandleLeftClick(), TriggerOnce.False));
            GlobalKeybinds.Add(InputManager.RegisterKeybind(new object[] { MouseButtons.RightButton }, new object[] { Keys.LeftShift, Keys.LeftControl }, _ => HandleRightClick(), TriggerOnce.False));

            GlobalKeybinds.Add(InputManager.RegisterKeybind(new object[] {Keys.LeftControl, Keys.Z}, new object[] {Keys.LeftShift, MouseButtons.LeftButton}, _ => CommandManager.Undo(), TriggerOnce.Block, true));
            GlobalKeybinds.Add(InputManager.RegisterKeybind(new object[] {Keys.LeftControl, Keys.LeftShift, Keys.Z}, new object[] {MouseButtons.LeftButton}, _ => CommandManager.Redo(), TriggerOnce.Block, true));

            ColorPicker = new ColorPicker(new Rectangle(SQ.WindowSize.X - 264, SQ.WindowSize.Y - 264, 256, 256), this);
            LayerTable.Initialize();

            DebugInfo.Subscribe(() => $"Selected Layer: {SelectedLayer} - {SelectedLayer.Identifier}");
            DebugInfo.Subscribe(() => $"Undo: {CommandManager.UndoHistory.Count} Redo: {CommandManager.RedoHistory.Count}");
            DebugInfo.Subscribe(() => $"Selected Brush: {Brush}");
            DebugInfo.Subscribe(() => $"Selected Color: {SelectedColor}");
            DebugInfo.Subscribe(() => $"Pen Pressure: {InputManager.AveragePenPressure} Tilt: {InputManager.PenTilt}");
            SelectedLayer = Screen.Section.Root.Layers[0];
        }

        public override void LoadContent() {
            ColorPicker.LoadContent();
            LayerTable.BuildUI();

            for (int i = 0; i < Brush.BrushTypes.Count; i++) {
                int currentIndex = i;
                InputManager.RegisterKeybind((Keys)(49 + currentIndex), _ => {
                    if (Focused) currentBrushIndex = currentIndex;
                }, TriggerOnce.True);
            }
        }

        public override void Update() {
            base.Update();

            ColorPicker.Update();
            // ZoomScreen(InputManager.ScrollWheelDelta * 0.001f);
        }

        public void UnregisterEditorGlobalKeybinds() {
            foreach (var keybind in GlobalKeybinds) {
                InputManager.UnregisterKeybind(keybind);
            }
        }

        public void MoveScreen(params object[] parameters) {
            if (!Focused) return;
            if (parameters.Length == 1 && parameters[0] is Vector2 direction) {
                Screen.Camera.MoveCamera(direction * 0.75f);
            }
        }

        public void ZoomScreen(params object[] parameters) {
            if (!Focused) return;
            if (parameters.Length == 1 && parameters[0] is float ratio) {
                Screen.Camera.ZoomCamera(ratio * 0.75f);
            }
        }

        public void RotateScreen(params object[] parameters) {
            if (!Focused) return;
            if (parameters.Length == 1 && parameters[0] is float ratio) {
                Screen.Camera.RotateCamera(ratio);
            }
        }

        public void DrawRectangle() {
            if (PreviousGlobalMousePos is null) PreviousGlobalMousePos = Screen.Camera.GlobalMousePos;
            PaintModeState = PaintModeState.Rectangle;

            if (InputManager.IsMouseButtonPressed(MouseButtons.LeftButton)) {
                if (PreviousGlobalMousePos is null) return;
                ((PaintableLayer2D)SelectedLayer).PaintRectangle((Vector2I)PreviousGlobalMousePos, (Vector2I)Screen.Camera.GlobalMousePos, SelectedColor, 1f, true, CommandManager.AddCommandChain(new CommandChain()));
                PreviousGlobalMousePos = Screen.Camera.GlobalMousePos;
            }
        }

        public void DrawLine() {
            if (PreviousGlobalMousePos is null) PreviousGlobalMousePos = Screen.Camera.GlobalMousePos;
            PaintModeState = PaintModeState.Line;

            if (InputManager.IsMouseButtonPressed(MouseButtons.LeftButton)) {
                if (PreviousGlobalMousePos is null) return;
                if (InputManager.IsKeyDown(Keys.LeftShift)) {
                    ((PaintableLayer2D)SelectedLayer).PaintSnappedLine((Vector2I)PreviousGlobalMousePos, (Vector2I)Screen.Camera.GlobalMousePos, SelectedColor, 1f, 0, CommandManager.AddCommandChain(new CommandChain()));
                    PreviousGlobalMousePos = PixelActions.ApplySnappedLineAction((Vector2I)PreviousGlobalMousePos, (Vector2I)Screen.Camera.GlobalMousePos, 0, _ => { });
                } else {
                    ((PaintableLayer2D)SelectedLayer).PaintLine((Vector2I)PreviousGlobalMousePos, (Vector2I)Screen.Camera.GlobalMousePos, SelectedColor, 1f, 0, CommandManager.AddCommandChain(new CommandChain()));
                    PreviousGlobalMousePos = Screen.Camera.GlobalMousePos;
                }
            }
        }

        public void PostRectangle() {
            PreviousGlobalMousePos = null;
            PaintModeState = PaintModeState.Idle;
        }

        public void PostLine() {
            PreviousGlobalMousePos = null;
            PaintModeState = PaintModeState.Idle;
        }

        public void HandleLeftClick() {
            if (!Focused) return;
            
            if (InputManager.IsKeyDown(Keys.LeftAlt)) {
                if (SelectedLayer is PaintableLayer2D paintableLayer) {
                    var color = paintableLayer.GetColor((Vector2I)Screen.Camera.GlobalMousePos.Value);
                    if (color != null) {
                        SelectedColor = color.Value;
                        ColorPicker.SetColor(color.Value);
                    }
                }
            } else {
                if (SelectedLayer is PaintableLayer2D paintableLayer) {
                    Brush.Paint(paintableLayer, InputManager.IsMouseButtonPressed(MouseButtons.LeftButton), SelectedColor, Screen.Camera);
                }
            }
        }

        public void HandleRightClick() {
            if (!Focused) return;

            if (InputManager.IsKeyDown(Keys.LeftAlt)) {

            } else {
                if (SelectedLayer is PaintableLayer2D paintableLayer) {
                    Brush.Paint(paintableLayer, InputManager.IsMouseButtonPressed(MouseButtons.RightButton), Color.Transparent, Screen.Camera);
                }
            }
        }

        public void SelectLayerUnderMouse(List<Layer2D> iter = null) {
            if (!Focused) return;

            iter ??= Screen.Section.Root.Layers;
            foreach (var layer in iter) {
                if (layer.Enabled && layer is PaintableLayer2D paintableLayer) {
                    Color? color = paintableLayer.GetColor((Vector2I)Screen.Camera.GlobalMousePos.Value);
                    if (color != null && color.Value.A != 0) {
                        SelectedLayer = layer;
                    }
                }
                if (layer.HasChildren()) {
                    SelectLayerUnderMouse(layer.Layers);
                }
            }
        }

        public void Save() { // TODO resolve ScreenManager algorithm that determines focused screen
            if (FileExplorer.Active) return;
            if (!string.IsNullOrEmpty(Screen.Section.Identifier)) {
                FileExplorer.SaveSection(Screen.Section.Identifier.Split(".")[0]);
            }  else {
                FileExplorer.BuildUI();
                FileExplorer.OpenDirectory("c:\\Somnia\\Projects\\monogame-somniloquy\\Assets");
            }
        }

        public override void Draw() {
            foreach (var layer in Screen.Section.Root.Layers) {
                if (layer.Enabled) layer.Draw(Screen.Camera);
            }

            if (Focused) Screen.Camera.DrawPoint((Vector2I)Screen.Camera.GlobalMousePos, SelectedColor * 0.5f);
            
            if (PreviousGlobalMousePos is not null) {
                if (PaintModeState == PaintModeState.Line) {
                    if (InputManager.IsKeyDown(Keys.LeftShift)) {
                        PixelActions.ApplySnappedLineAction((Vector2I)PreviousGlobalMousePos, (Vector2I)Screen.Camera.GlobalMousePos, 0, (pos) => Screen.Camera.SB.Draw(SQ.SB.Pixel, pos, SelectedColor * 0.5f));
                    } else {
                        PixelActions.ApplyLineAction((Vector2I)PreviousGlobalMousePos, (Vector2I)Screen.Camera.GlobalMousePos, 0, (pos) => Screen.Camera.SB.Draw(SQ.SB.Pixel, pos, SelectedColor * 0.5f));
                    }
                } else if (PaintModeState == PaintModeState.Rectangle) {
                    PixelActions.ApplyRectangleAction((Vector2I)PreviousGlobalMousePos, (Vector2I)Screen.Camera.GlobalMousePos, true, (pos) => Screen.Camera.SB.Draw(SQ.SB.Pixel, pos, SelectedColor * 0.5f));
                }
            }

            Screen.Camera.SB.End();
            
            if (SelectedLayer is TileLayer2D tileLayer) {
                DrawGrids(tileLayer.TileLength, Color.White * MathF.Min(Screen.Camera.Zoom / 16.0f, 0.25f));
                DrawGrids(tileLayer.ChunkLength * tileLayer.TileLength, Color.White * MathF.Min(Screen.Camera.Zoom / 4.0f, 0.5f));
            } else if (SelectedLayer is TextureLayer2D textureLayer) {
                DrawGrids(textureLayer.ChunkLength, Color.White * MathF.Min(Screen.Camera.Zoom / 4.0f, 0.5f));
            }
        }

        private void DrawGrids(int spacing, Color color) {
            if (color.A < 5) return;

            List<VertexPositionColor> vertices = new();
            var bounds = Screen.Camera.VisibleBounds;

            for (float y = MathF.Floor(bounds.Top / spacing) * spacing; y <= bounds.Bottom; y += spacing) {
                Vector2 start = Screen.Camera.ToScreenPos(new Vector2(bounds.Left, y));
                Vector2 end = Screen.Camera.ToScreenPos(new Vector2(bounds.Right, y));
                vertices.Add(new VertexPositionColor(new Vector3(start, 0), color));
                vertices.Add(new VertexPositionColor(new Vector3(end, 0), color));
            }

            for (float x = MathF.Floor(bounds.Left / spacing) * spacing; x <= bounds.Right; x += spacing) {
                Vector2 start = Screen.Camera.ToScreenPos(new Vector2(x, bounds.Top));
                Vector2 end = Screen.Camera.ToScreenPos(new Vector2(x, bounds.Bottom));
                vertices.Add(new VertexPositionColor(new Vector3(start, 0), color));
                vertices.Add(new VertexPositionColor(new Vector3(end, 0), color));
            }

            var verticesArray = vertices.ToArray();

            VertexBuffer vertexBuffer = new(SQ.GD, typeof(VertexPositionColor), verticesArray.Length, BufferUsage.WriteOnly);
            vertexBuffer.SetData(verticesArray);
            SQ.GD.SetVertexBuffer(vertexBuffer);

            foreach (var pass in SQ.BasicEffect.CurrentTechnique.Passes) {
                pass.Apply();
                SQ.GD.DrawPrimitives(PrimitiveType.LineList, 0, verticesArray.Length / 2);
            }
        }
    }
}
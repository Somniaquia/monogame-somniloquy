namespace Somniloquy {
    using System;
    using System.Collections.Generic;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Input;
    using Microsoft.Xna.Framework.Graphics;
    using System.Linq;

    public class Section2DScreen : BoxUI {
        public Section2D Section;
        public Camera2D Camera = new();
        public Section2DEditor Editor;

        public Section2DScreen(Rectangle boundaries, Section2D section = null) : base(boundaries) {
            Section = section;

            if (Section is null) { // temp
                Section = new();
                Section.LayerGroups.Add(0, new LayerGroup2D());
                Section.LayerGroups[0].Layers.Add(0, new TileLayer2D(16, 16));
            }

            Editor = new(this);
            Camera.MaxZoom = 16f;
            Camera.MinZoom = 1 / 4f;
        }

        public override void LoadContent() {
            Camera.LoadContent();
            Editor.LoadContent();
        }

        public override void Update() {
            base.Update();
            Camera.Update();
            Editor?.Update();
            Section?.Update();
        }

        public override void Draw() {
            Camera.SB.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, transformMatrix: Camera.Transform);
            Editor?.Draw();
            Camera.SB.End();
        }
    }
    
    public enum EditorMode { PaintMode, TileMode, }
    public enum PaintModeState { Idle, Rectangle, Line, Select }

    public class Section2DEditor : BoxUI {
        public Section2DScreen Screen;
        public Layer2D SelectedLayer;
        public ColorPicker ColorPicker;
        public LayerTable LayerTable;

        public Color SelectedColor = Color.White;
        public EditorMode EditorMode = EditorMode.PaintMode;
        private int currentBrushIndex;
        public Brush Brush => Brush.BrushTypes[currentBrushIndex];

        public List<Keybind> GlobalKeybinds = new();

        public Section2DEditor(Section2DScreen screen) : base(screen) {
            Screen = screen;
            Boundaries = screen.Boundaries;

            GlobalKeybinds.Add(InputManager.RegisterKeybind(Keys.W, (parameters) => MoveScreen(new Vector2(0, -1)), TriggerOnce.False));
            GlobalKeybinds.Add(InputManager.RegisterKeybind(Keys.A, (parameters) => MoveScreen(new Vector2(-1, 0)), TriggerOnce.False));
			GlobalKeybinds.Add(InputManager.RegisterKeybind(Keys.S, (parameters) => MoveScreen(new Vector2(0, 1)), TriggerOnce.False));
            GlobalKeybinds.Add(InputManager.RegisterKeybind(Keys.D, (parameters) => MoveScreen(new Vector2(1, 0)), TriggerOnce.False));

            GlobalKeybinds.Add(InputManager.RegisterKeybind(Keys.Q, (parameters) => ZoomScreen(-0.05f), TriggerOnce.False));
            GlobalKeybinds.Add(InputManager.RegisterKeybind(Keys.E, (parameters) => ZoomScreen(0.05f), TriggerOnce.False));
            GlobalKeybinds.Add(InputManager.RegisterKeybind(Keys.OemPipe, (parameters) => Screen.Camera.TargetRotation = 0, TriggerOnce.True));
            GlobalKeybinds.Add(InputManager.RegisterKeybind(Keys.OemOpenBrackets, (parameters) => RotateScreen(-0.05f), TriggerOnce.False));
            GlobalKeybinds.Add(InputManager.RegisterKeybind(Keys.OemCloseBrackets, (parameters) => RotateScreen(0.05f), TriggerOnce.False));
            
            GlobalKeybinds.Add(InputManager.RegisterKeybind(Keys.LeftAlt, (parameters) => SelectLayerUnderMouse(), TriggerOnce.False));

            GlobalKeybinds.Add(InputManager.RegisterKeybind(new object[] { MouseButtons.LeftButton }, (parameters) => HandleLeftClick(), TriggerOnce.False));
            GlobalKeybinds.Add(InputManager.RegisterKeybind(new object[] { MouseButtons.RightButton }, (parameters) => HandleRightClick(), TriggerOnce.False));

            GlobalKeybinds.Add(InputManager.RegisterKeybind(new object[] { Keys.B }, new object[] { MouseButtons.LeftButton, MouseButtons.RightButton }, (parameters) => SelectNextBrush(), TriggerOnce.True, true));
            DebugInfo.Subscribe(() => $"Selected Brush: {Brush}");

            GlobalKeybinds.Add(InputManager.RegisterKeybind(new object[] {Keys.LeftControl, Keys.Z}, new object[] {Keys.LeftShift, MouseButtons.LeftButton}, (parameters) => CommandManager.Undo(), TriggerOnce.True, true));
            GlobalKeybinds.Add(InputManager.RegisterKeybind(new object[] {Keys.LeftControl, Keys.LeftShift, Keys.Z}, new object[] {MouseButtons.LeftButton}, (parameters) => CommandManager.Redo(), TriggerOnce.True, true));

            GlobalKeybinds.Add(InputManager.RegisterKeybind(new object[] {Keys.LeftControl, Keys.S}, (parameters) => Save(), TriggerOnce.True, true));

            ColorPicker = new ColorPicker(new Rectangle(SQ.WindowSize.X - 264, SQ.WindowSize.Y - 264, 256, 256), this);
            LayerTable = new LayerTable(Screen);

            DebugInfo.Subscribe(() => $"Pen Pressure: {InputManager.AveragePenPressure}");
            DebugInfo.Subscribe(() => $"Pen Tilt: {InputManager.PenTilt}");
            DebugInfo.Subscribe(() => $"Undo History: {CommandManager.UndoHistory.Count}");
            DebugInfo.Subscribe(() => $"Redo History: {CommandManager.RedoHistory.Count}");
            DebugInfo.Subscribe(() => $"Selected Color: {SelectedColor}");
            SelectedLayer = Screen.Section.LayerGroups.First().Value.Layers.Values.OfType<TileLayer2D>().FirstOrDefault();
        }

        public override void LoadContent() {
            ColorPicker.LoadContent();
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

        public void HandleLeftClick() {
            if (!Focused) return;
            
            if (InputManager.IsKeyDown(Keys.LeftAlt)) {
                if (SelectedLayer is IPaintableLayer2D paintableLayer) {
                    var color = paintableLayer.GetColor((Vector2I)Screen.Camera.GlobalMousePos.Value);
                    if (color != null) {
                        SelectedColor = color.Value;
                        ColorPicker.SetColor(color.Value);
                    }
                }
            } else {
                if (SelectedLayer is IPaintableLayer2D paintableLayer) {
                    Brush.Paint(paintableLayer, InputManager.IsMouseButtonPressed(MouseButtons.LeftButton), SelectedColor, Screen.Camera);
                }
            }
        }

        public void HandleRightClick() {
            if (!Focused) return;

            if (InputManager.IsKeyDown(Keys.LeftAlt)) {

            } else {
                if (SelectedLayer is IPaintableLayer2D paintableLayer) {
                    Brush.Paint(paintableLayer, InputManager.IsMouseButtonPressed(MouseButtons.RightButton), Color.Transparent, Screen.Camera);
                }
            }
        }

        public void SelectLayerUnderMouse() {
            if (!Focused) return;

            foreach (var pair in Screen.Section.LayerGroups) {
                var layerGroup = pair.Value;
                foreach (var layer in layerGroup.Layers) {
                    if (layer.Value is IPaintableLayer2D paintableLayer) {
                        Color? color = paintableLayer.GetColor((Vector2I)Screen.Camera.GlobalMousePos.Value);
                        if (color != null) {
                            if (color.Value.A != 0) {
                                SelectedLayer = layer.Value;
                            }
                        }
                    }
                }
            }
        }

        public void SelectNextBrush() {
            if (!Focused) return;
            currentBrushIndex = (currentBrushIndex + 1) % Brush.BrushTypes.Count;
        }

        public void Save() { // TODO resolve ScreenManager algorithm that determines focused screen
            if (FileExplorer.Active) return;
            if (!string.IsNullOrEmpty(Screen.Section.Identifier)) {
                FileExplorer.Save(Screen.Section.Identifier.Split(".")[0]);
            }  else {
                FileExplorer.BuildUI();
                FileExplorer.OpenDirectory("c:\\Somnia\\Projects\\monogame-somniloquy\\Assets");
            }
        }

        public override void Draw() {
            Screen.Camera.DrawPoint((Vector2I)Screen.Camera.GlobalMousePos, SelectedColor * 0.5f);
            
            foreach (var layerGroup in Screen.Section.LayerGroups) {
                foreach (var layer in layerGroup.Value.Layers) {
                    if (layer.Value == SelectedLayer) {
                        layer.Value.Draw(Screen.Camera, 1f);
                        // layer.Value.Draw(Screen.Camera, 1f, 1f);
                    } else if (layerGroup.Value.Layers.ContainsValue(SelectedLayer)){
                        layer.Value.Draw(Screen.Camera, 0.8f);
                    } else {
                        layer.Value.Draw(Screen.Camera, 0.2f);
                    }
                }
            }

            DrawGridLines();

            LayerTable.Draw();
        }

        private void AddGridVertices(ref List<VertexPositionColor> vertices, int spacing, Color color) {
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

            vertices.Add(new VertexPositionColor(new Vector3(-1, 0, 0), Color.Red)); // Left
            vertices.Add(new VertexPositionColor(new Vector3(1, 0, 0), Color.Red));  // Right

            vertices.Add(new VertexPositionColor(new Vector3(0, -1, 0), Color.Green)); // Bottom
            vertices.Add(new VertexPositionColor(new Vector3(0, 1, 0), Color.Green));  // Top
        }

        public void DrawGridLines() {
            var vertices = new List<VertexPositionColor>();

            if (SelectedLayer is TileLayer2D tileLayer) {
                AddGridVertices(ref vertices, tileLayer.TileLength, Color.White * 0.5f);
                AddGridVertices(ref vertices, tileLayer.ChunkLength, Color.White * 0.5f);
            } else if (SelectedLayer is TextureLayer2D textureLayer) {
                AddGridVertices(ref vertices, textureLayer.ChunkLength, Color.White * 0.5f);
            }

            var verticesArray = vertices.ToArray();
            var rasterizerState = new RasterizerState { CullMode = CullMode.None };
            SQ.GD.RasterizerState = rasterizerState;
            SQ.GD.BlendState = BlendState.AlphaBlend;

            VertexBuffer vertexBuffer = new(SQ.GD, typeof(VertexPositionColor), verticesArray.Length, BufferUsage.WriteOnly);
            vertexBuffer.SetData(verticesArray);
            SQ.GD.SetVertexBuffer(vertexBuffer);

            var basicEffect = new BasicEffect(SQ.GD) {
                VertexColorEnabled = true,
                Projection = Matrix.CreateOrthographicOffCenter(0, SQ.WindowSize.X, SQ.WindowSize.Y, 0, 0, 1),
                View = Matrix.Identity,
                World = Matrix.Identity
            };
            foreach (var pass in basicEffect.CurrentTechnique.Passes) {
                pass.Apply();
                SQ.GD.DrawPrimitives(PrimitiveType.LineList, 0, verticesArray.Length / 2);
            }
        }
    }
}
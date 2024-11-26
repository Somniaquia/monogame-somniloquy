namespace Somniloquy {
    using System;
    using System.Collections.Generic;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Input;
    using Microsoft.Xna.Framework.Graphics;
    using System.Linq;

    public class Section2DScreen : Screen {
        public Section2D Section;
        public Camera2D Camera = new();
        public Section2DEditor Editor;

        public Section2DScreen(Rectangle boundaries, Section2D section = null) : base(boundaries) {
            Section = section;

            if (Section is null) { // temp
                Section = new();
                Section.LayerGroups.Add(0, new LayerGroup2D());
                Section.LayerGroups[0].Layers.Add(0, new TextureLayer2D());
            }

            Editor = new(boundaries, this);
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
            SQ.SB.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
            Camera.SB.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, transformMatrix: Camera.Transform);
            Editor?.Draw();
            Camera.SB.End();
            SQ.SB.End();
        }
    }
    
    public enum EditorState { PaintMode, TileMode, }

    public class Section2DEditor : Screen {
        public Section2DScreen Screen;
        public Layer2D SelectedLayer;
        public ColorPicker ColorPicker;
        public LayerTable LayerTable;

        public Color SelectedColor = Color.White;
        public EditorState EditorState = EditorState.PaintMode;
        private int currentBrushIndex;
        public Brush Brush => Brush.BrushTypes[currentBrushIndex];

        public List<Keybind> GlobalKeybinds = new();

        public Section2DEditor(Rectangle boundaries, Section2DScreen screen) : base(boundaries) {
            Screen = screen;

            GlobalKeybinds.Add(InputManager.RegisterKeybind(Keys.W, (parameters) => MoveScreen(new Vector2(0, -1)), TriggerOnce.False));
            GlobalKeybinds.Add(InputManager.RegisterKeybind(Keys.A, (parameters) => MoveScreen(new Vector2(-1, 0)), TriggerOnce.False));
			GlobalKeybinds.Add(InputManager.RegisterKeybind(Keys.S, (parameters) => MoveScreen(new Vector2(0, 1)), TriggerOnce.False));
            GlobalKeybinds.Add(InputManager.RegisterKeybind(Keys.D, (parameters) => MoveScreen(new Vector2(1, 0)), TriggerOnce.False));

            GlobalKeybinds.Add(InputManager.RegisterKeybind(Keys.Q, (parameters) => ZoomScreen(-0.05f), TriggerOnce.False));
            GlobalKeybinds.Add(InputManager.RegisterKeybind(Keys.E, (parameters) => ZoomScreen(0.05f), TriggerOnce.False));
            GlobalKeybinds.Add(InputManager.RegisterKeybind(Keys.OemPipe, (parameters) => Screen.Camera.TargetRotation = 0, TriggerOnce.True));
            GlobalKeybinds.Add(InputManager.RegisterKeybind(Keys.OemOpenBrackets, (parameters) => RotateScreen(-0.05f), TriggerOnce.False));
            GlobalKeybinds.Add(InputManager.RegisterKeybind(Keys.OemCloseBrackets, (parameters) => RotateScreen(0.05f), TriggerOnce.False));
            // GlobalKeybinds.Add(InputManager.RegisterKeybind(Keys.U, (parameters) => ShiftHue(-0.005f), TriggerOnce.False));
            // GlobalKeybinds.Add(InputManager.RegisterKeybind(Keys.O, (parameters) => ShiftHue(0.005f), TriggerOnce.False));

            GlobalKeybinds.Add(InputManager.RegisterKeybind(Keys.LeftAlt, (parameters) => SelectLayerUnderMouse(), TriggerOnce.False));

            GlobalKeybinds.Add(InputManager.RegisterKeybind(new object[] { MouseButtons.LeftButton }, (parameters) => HandleLeftClick(), TriggerOnce.False));
            GlobalKeybinds.Add(InputManager.RegisterKeybind(new object[] { MouseButtons.RightButton }, (parameters) => HandleRightClick(), TriggerOnce.False));

            GlobalKeybinds.Add(InputManager.RegisterKeybind(new object[] { Keys.B }, new object[] { MouseButtons.LeftButton, MouseButtons.RightButton }, (parameters) => SelectNextBrush(), TriggerOnce.True, true));
            DebugInfo.Subscribe(() => $"Selected Brush: {Brush}");

            GlobalKeybinds.Add(InputManager.RegisterKeybind(new object[] {Keys.LeftControl, Keys.Z}, new object[] {Keys.LeftShift, MouseButtons.LeftButton}, (parameters) => CommandManager.Undo(), TriggerOnce.True, true));
            GlobalKeybinds.Add(InputManager.RegisterKeybind(new object[] {Keys.LeftControl, Keys.LeftShift, Keys.Z}, new object[] {MouseButtons.LeftButton}, (parameters) => CommandManager.Redo(), TriggerOnce.True, true));

            ColorPicker = new ColorPicker(new Rectangle(SQ.WindowSize.X - 264, SQ.WindowSize.Y - 264, 256, 256), this);
            LayerTable = new LayerTable(Screen);

            DebugInfo.Subscribe(() => $"Pen Pressure: {InputManager.PenPressure}");
            DebugInfo.Subscribe(() => $"Pen Tilt: {InputManager.PenTilt}");
            DebugInfo.Subscribe(() => $"Undo History: {CommandManager.UndoHistory.Count}");
            DebugInfo.Subscribe(() => $"Redo History: {CommandManager.RedoHistory.Count}");
            DebugInfo.Subscribe(() => $"Selected Color: {SelectedColor}");
            SelectedLayer = Screen.Section.LayerGroups.First().Value.Layers.Values.OfType<TextureLayer2D>().FirstOrDefault();
        }

        public override void LoadContent() {
            ColorPicker.LoadContent();
        }

        public override void Update() {
            base.Update();

            ColorPicker.Update();
            ZoomScreen(InputManager.ScrollWheelDelta * 0.001f);
        }

        public void UnregisterEditorGlobalKeybinds() {
            foreach (var keybind in GlobalKeybinds) {
                InputManager.UnregisterKeybind(keybind);
            }
        }

        public void MoveScreen(params object[] parameters) {
            if (parameters.Length == 1 && parameters[0] is Vector2 direction) {
                Screen.Camera.MoveCamera(direction * 0.75f);
            }
        }

        public void ZoomScreen(params object[] parameters) {
            if (parameters.Length == 1 && parameters[0] is float ratio) {
                Screen.Camera.ZoomCamera(ratio * 0.75f);
            }
        }

        public void RotateScreen(params object[] parameters) {
            if (parameters.Length == 1 && parameters[0] is float ratio) {
                Screen.Camera.RotateCamera(ratio);
            }
        }

        public void HandleLeftClick() {
            if (ScreenManager.FocusedScreen != this) return;
            
            if (InputManager.IsKeyDown(Keys.LeftAlt)) {
                if (SelectedLayer is IPaintableLayer2D paintableLayer) {
                    var color = paintableLayer.GetColor((Vector2I)Screen.Camera.GlobalMousePos.Value);
                    if (color != null) SelectedColor = color.Value;
                    ColorPicker.Hue = SelectedColor.ToOkHSL().H;
                    ColorPicker.CreateChartTexture();
                }
            } else {
                if (SelectedLayer is IPaintableLayer2D paintableLayer) {
                    Brush.Paint(paintableLayer, InputManager.IsMouseButtonPressed(MouseButtons.LeftButton), SelectedColor, Screen.Camera);
                }
            }
        }

        public void HandleRightClick() {
            if (ScreenManager.FocusedScreen != this) return;

            if (InputManager.IsKeyDown(Keys.LeftAlt)) {

            } else {
                if (SelectedLayer is IPaintableLayer2D paintableLayer) {
                    Brush.Paint(paintableLayer, InputManager.IsMouseButtonPressed(MouseButtons.RightButton), Color.Transparent, Screen.Camera);
                }
            }
        }

        public void SelectLayerUnderMouse() {
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
            currentBrushIndex = (currentBrushIndex + 1) % Brush.BrushTypes.Count;
        }

        public override void Draw() {
            Screen.Camera.DrawPoint((Vector2I)Screen.Camera.GlobalMousePos, SelectedColor * 0.5f);
            
            foreach (var layerGroup in Screen.Section.LayerGroups) {
                foreach (var layer in layerGroup.Value.Layers) {
                    if (layer.Value == SelectedLayer) {
                        layer.Value.Draw(Screen.Camera, true);
                    } else {
                        layer.Value.Draw(Screen.Camera, false, 0.5f);
                    }
                }
            }

            ColorPicker.Draw();
            LayerTable.Draw();
        }
    }
}
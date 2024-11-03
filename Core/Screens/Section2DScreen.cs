namespace Somniloquy {
    using System;
    using System.Collections.Generic;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Input;
    using Microsoft.Xna.Framework.Graphics;

    public class Section2DScreen : Screen {
        public Section2D Section;
        public Camera2D Camera = new();
        public Section2DEditor Editor;

        public Section2DScreen(Rectangle boundaries, Section2D section = null) : base(boundaries) {
            Section = section;

            if (Section is null) { // temp
                Section = new();
                Section.AddLayerGroup("group1");
            }

            Editor = new(boundaries, this);
        }

        public Section2D LoadSection() {
            return null;
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
    
    public enum EditorState { PaintMode, TileMode, }

    public class Section2DEditor : Screen {
        public Section2DScreen Screen;
        public Layer2D SelectedLayer;
        public ColorPicker ColorPicker;

        public Color SelectedColor = Color.White;
        public EditorState EditorState = EditorState.PaintMode;

        public List<Keybind> GlobalKeybinds = new();

        public CommandChain CurrentCommandChain;

        public Section2DEditor(Rectangle boundaries, Section2DScreen screen) : base(boundaries) {
            Screen = screen;

            GlobalKeybinds.Add(InputManager.RegisterKeybind(Keys.W, (parameters) => MoveScreen(new Vector2(0, -1)), false));
            GlobalKeybinds.Add(InputManager.RegisterKeybind(Keys.A, (parameters) => MoveScreen(new Vector2(-1, 0)), false));
			GlobalKeybinds.Add(InputManager.RegisterKeybind(Keys.S, (parameters) => MoveScreen(new Vector2(0, 1)), false));
            GlobalKeybinds.Add(InputManager.RegisterKeybind(Keys.D, (parameters) => MoveScreen(new Vector2(1, 0)), false));

            GlobalKeybinds.Add(InputManager.RegisterKeybind(Keys.Q, (parameters) => ZoomScreen(-0.05f), false));
            GlobalKeybinds.Add(InputManager.RegisterKeybind(Keys.E, (parameters) => ZoomScreen(0.05f), false));
            GlobalKeybinds.Add(InputManager.RegisterKeybind(Keys.OemPipe, (parameters) => Screen.Camera.TargetRotation = 0, true));
            GlobalKeybinds.Add(InputManager.RegisterKeybind(Keys.OemOpenBrackets, (parameters) => RotateScreen(-0.05f), false));
            GlobalKeybinds.Add(InputManager.RegisterKeybind(Keys.OemCloseBrackets, (parameters) => RotateScreen(0.05f), false));
            // GlobalKeybinds.Add(InputManager.RegisterKeybind(Keys.U, (parameters) => ShiftHue(-0.005f), false));
            // GlobalKeybinds.Add(InputManager.RegisterKeybind(Keys.O, (parameters) => ShiftHue(0.005f), false));

            GlobalKeybinds.Add(InputManager.RegisterKeybind(Keys.LeftAlt, (parameters) => SelectLayer(), false));

            GlobalKeybinds.Add(InputManager.RegisterKeybind(new object[] { MouseButtons.LeftButton }, (parameters) => HandleLeftClick(), false));
            GlobalKeybinds.Add(InputManager.RegisterKeybind(new object[] { MouseButtons.RightButton }, (parameters) => HandleRightClick(), false));

            GlobalKeybinds.Add(InputManager.RegisterKeybind(new object[] {Keys.LeftControl, Keys.Z}, new object[] {Keys.LeftShift, MouseButtons.LeftButton}, (parameters) => CommandManager.Undo(), true, true));
            GlobalKeybinds.Add(InputManager.RegisterKeybind(new object[] {Keys.LeftControl, Keys.LeftShift, Keys.Z}, new object[] {MouseButtons.LeftButton}, (parameters) => CommandManager.Redo(), true, true));

            SelectedLayer = new TileLayer2D();
            Screen.Section.LayerGroups["group1"].AddLayer(SelectedLayer);
            ColorPicker = new ColorPicker(new Rectangle(SQ.WindowSize.X - 264, SQ.WindowSize.Y - 264, 256, 256), this);

            DebugInfo.Subscribe(() => $"Selected Color: {SelectedColor}");
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
            if (InputManager.IsKeyDown(Keys.LeftAlt)) {
                if (SelectedLayer is IPaintableLayer2D paintableLayer) {
                    var color = paintableLayer.GetColor((Vector2I)Screen.Camera.GlobalMousePos.Value);
                    if (color != null) SelectedColor = color.Value;
                    ColorPicker.Hue = SelectedColor.ToOkHSL().H;
                    ColorPicker.CreateChartTexture();
                }
            } else {
                Paint(InputManager.IsMouseButtonPressed(MouseButtons.LeftButton), SelectedColor);
            }
        }

        public void HandleRightClick() {
            Paint(InputManager.IsMouseButtonPressed(MouseButtons.RightButton), Color.Transparent);
        }

        public void Paint(bool initializingPress, Color color) {
            if (ScreenManager.FocusedScreen != this) return;

            if (SelectedLayer is IPaintableLayer2D paintableLayer) {
                int penWidth = (int)(InputManager.GetPenPressure() * 5);
                float penOpacity = InputManager.GetPenPressure() != 0 ? InputManager.GetPenPressure() : 1;

                if (initializingPress) {
                    CurrentCommandChain = CommandManager.AddCommandChain(new CommandChain());
                    // (int)(InputManager.GetPenTilt().Length() * 5)
                    paintableLayer.PaintCircle((Vector2I)Screen.Camera.GlobalMousePos.Value, penWidth, color, penOpacity, true, CurrentCommandChain);
                } else {
                    paintableLayer.PaintLine((Vector2I)Screen.Camera.PreviousGlobalMousePos.Value, (Vector2I)Screen.Camera.GlobalMousePos.Value, color, penOpacity, penWidth, CurrentCommandChain);
                }
            }
        }

        public void SelectLayer() {
            // Get mouse position
            // Get top layer with texture under mouse pos
            // set SelectedLayer with that layer
        }

        public override void Draw() {
            Screen.Camera.DrawPoint((Vector2I)Screen.Camera.GlobalMousePos, SelectedColor * 0.5f);
            SelectedLayer?.Draw(Screen.Camera, true);
            ColorPicker.Draw();
        }
    }
}
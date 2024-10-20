namespace Somniloquy {
    using System;
    using System.Collections.Generic;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Input;

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
    
        public override void Update() {
            base.Update();
            Camera.Update();
            Editor?.Update();
            Section?.Update();
        }

        public override void Draw() {
            Editor.Draw();
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

        public Section2DEditor(Rectangle boundaries, Section2DScreen screen) : base(boundaries) {
            Screen = screen;

            GlobalKeybinds.Add(InputManager.RegisterKeybind(Keys.W, (parameters) => MoveScreen(new Vector2(0, -1)), false));
            GlobalKeybinds.Add(InputManager.RegisterKeybind(Keys.A, (parameters) => MoveScreen(new Vector2(-1, 0)), false));
			GlobalKeybinds.Add(InputManager.RegisterKeybind(Keys.S, (parameters) => MoveScreen(new Vector2(0, 1)), false));
            GlobalKeybinds.Add(InputManager.RegisterKeybind(Keys.D, (parameters) => MoveScreen(new Vector2(1, 0)), false));

            // GlobalKeybinds.Add(InputManager.RegisterKeybind(Keys.Q, (parameters) => ZoomScreen(-0.05f), false));
            // GlobalKeybinds.Add(InputManager.RegisterKeybind(Keys.E, (parameters) => ZoomScreen(0.05f), false));
            GlobalKeybinds.Add(InputManager.RegisterKeybind(Keys.Q, (parameters) => RotateScreen(-0.05f), false));
            GlobalKeybinds.Add(InputManager.RegisterKeybind(Keys.E, (parameters) => RotateScreen(0.05f), false));
            // GlobalKeybinds.Add(InputManager.RegisterKeybind(Keys.U, (parameters) => ShiftHue(-0.005f), false));
            // GlobalKeybinds.Add(InputManager.RegisterKeybind(Keys.O, (parameters) => ShiftHue(0.005f), false));

            GlobalKeybinds.Add(InputManager.RegisterKeybind(Keys.LeftAlt, (parameters) => SelectLayer(), false));

            GlobalKeybinds.Add(InputManager.RegisterKeybind(new object[] { MouseButtons.LeftButton }, (parameters) => HandleLeftClick(), false));
            GlobalKeybinds.Add(InputManager.RegisterKeybind(new object[] { MouseButtons.RightButton }, (parameters) => HandleRightClick(), false));

            SelectedLayer = new TextureLayer2D();
            Screen.Section.LayerGroups["group1"].AddLayer(SelectedLayer);
            ColorPicker = new ColorPicker(boundaries, this);
        }

        public override void Update() {
            base.Update();

            ZoomScreen(InputManager.ScrollWheelDelta * 0.001f);
        }

        public void UnregisterEditorGlobalKeybinds() {
            foreach (var keybind in GlobalKeybinds) {
                InputManager.UnregisterKeybind(keybind);
            }
        }

        public void MoveScreen(params object[] parameters) {
            if (parameters.Length == 1 && parameters[0] is Vector2 direction) {
                Screen.Camera.MoveCamera(direction);
            }
        }

        public void ZoomScreen(params object[] parameters) {
            if (parameters.Length == 1 && parameters[0] is float ratio) {
                Screen.Camera.ZoomCamera(ratio);
            }
        }

        public void RotateScreen(params object[] parameters) {
            if (parameters.Length == 1 && parameters[0] is float ratio) {
                Screen.Camera.RotateCamera(ratio);
            }
        }

        public void HandleLeftClick() { // This is a total placeholder, implement proper seperate functions for each action and implement keybind exclusive priorities
            if (!Focused) return;

            if (SelectedLayer is IPaintableLayer2D paintableLayer) {
                if (InputManager.IsMouseButtonPressed(MouseButtons.LeftButton)) {
                    paintableLayer.PaintCircle((Vector2I)Screen.Camera.GlobalMousePos.Value, (int)(InputManager.GetPenTilt().Length() * 5), SelectedColor, InputManager.GetPenPressure());
                } else {
                    paintableLayer.PaintLine((Vector2I)Screen.Camera.PreviousGlobalMousePos.Value, (Vector2I)Screen.Camera.GlobalMousePos.Value, SelectedColor, InputManager.GetPenPressure(), (int)(InputManager.GetPenTilt().Length() * 5));
                }
            }
        }

        public void HandleRightClick() {
            // Erasing = true;
            // HandleLeftClick();
            // Erasing = false;
        }

        public void SelectLayer() {
            // Get mouse position
            // Get top layer with texture under mouse pos
            // set SelectedLayer with that layer
        }

        public override void Draw() {
            Screen.Camera.DrawPoint((Vector2I)Screen.Camera.GlobalMousePos, Color.White * 0.5f);
            SelectedLayer?.Draw(Screen.Camera);
        }
    }
}
namespace Somniloquy {
    using System;
    using System.Collections.Generic;
    
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Input;

    public abstract class EditorMode {
        public abstract void LoadContent();
        public abstract void Update();
        public abstract void Draw();
    }

    public class PaintMode : EditorMode {
        public Section2DScreen Screen;
        public Section2DEditor Editor;
        public List<Keybind> PaintModeKeybinds = new();

        public PaintModeState PaintModeState = PaintModeState.Idle;
        public Vector2? PreviousGlobalMousePos;

        public Color SelectedColor = Color.White;
        public ColorPicker ColorPicker;

        private int currentBrushIndex;
        public Brush Brush => Brush.BrushTypes[currentBrushIndex];

        public PaintMode(Section2DScreen screen, Section2DEditor editor) {
            Screen = screen;
            Editor = editor;
            PaintModeKeybinds.Add(InputManager.RegisterKeybind(new object[] { Keys.LeftShift }, new object[] { Keys.LeftControl }, _ => DrawRectangle(), _ => PostRectangle(), TriggerOnce.False));
            PaintModeKeybinds.Add(InputManager.RegisterKeybind(new object[] { Keys.LeftControl}, new object[] { }, _ => DrawLine(), _ => PostLine(), TriggerOnce.False));

            PaintModeKeybinds.Add(InputManager.RegisterKeybind(new object[] { MouseButtons.LeftButton }, new object[] { Keys.LeftShift, Keys.LeftControl }, _ => HandleLeftClick(), TriggerOnce.False));
            PaintModeKeybinds.Add(InputManager.RegisterKeybind(new object[] { MouseButtons.RightButton }, new object[] { Keys.LeftShift, Keys.LeftControl }, _ => HandleRightClick(), TriggerOnce.False));
        
            DebugInfo.Subscribe(() => $"Selected Brush: {Brush}");
            DebugInfo.Subscribe(() => $"Selected Color: {SelectedColor}");
            DebugInfo.Subscribe(() => $"Pen Pressure: {InputManager.AveragePenPressure} Tilt: {InputManager.PenTilt}");
        
            ColorPicker = new ColorPicker(new Rectangle(SQ.WindowSize.X - 264, SQ.WindowSize.Y - 264, 256, 256), this);
        }

        public override void LoadContent() {
            ColorPicker.LoadContent();
            for (int i = 0; i < Brush.BrushTypes.Count; i++) {
                int currentIndex = i;
                InputManager.RegisterKeybind((Keys)(49 + currentIndex), _ => {
                    if (Editor.Focused) currentBrushIndex = currentIndex;
                }, TriggerOnce.True);
            }
        }

        ~PaintMode() {
            PaintModeKeybinds.ForEach(keybind => InputManager.UnregisterKeybind(keybind));
        }

        public void DrawRectangle() {
            if (PreviousGlobalMousePos is null) PreviousGlobalMousePos = Screen.Camera.GlobalMousePos;
            PaintModeState = PaintModeState.Rectangle;

            if (InputManager.IsMouseButtonPressed(MouseButtons.LeftButton)) {
                if (PreviousGlobalMousePos is null) return;
                ((PaintableLayer2D)Editor.SelectedLayer).PaintRectangle((Vector2I)PreviousGlobalMousePos, (Vector2I)Screen.Camera.GlobalMousePos, SelectedColor, 1f, true, CommandManager.AddCommandChain(new CommandChain()));
                PreviousGlobalMousePos = Screen.Camera.GlobalMousePos;
            }
        }

        public void DrawLine() {
            if (PreviousGlobalMousePos is null) PreviousGlobalMousePos = Screen.Camera.GlobalMousePos;
            PaintModeState = PaintModeState.Line;

            if (InputManager.IsMouseButtonPressed(MouseButtons.LeftButton)) {
                if (PreviousGlobalMousePos is null) return;
                if (InputManager.IsKeyDown(Keys.LeftShift)) {
                    ((PaintableLayer2D)Editor.SelectedLayer).PaintSnappedLine((Vector2I)PreviousGlobalMousePos, (Vector2I)Screen.Camera.GlobalMousePos, SelectedColor, 1f, 0, CommandManager.AddCommandChain(new CommandChain()));
                    PreviousGlobalMousePos = PixelActions.ApplySnappedLineAction((Vector2I)PreviousGlobalMousePos, (Vector2I)Screen.Camera.GlobalMousePos, 0, _ => { });
                } else {
                    ((PaintableLayer2D)Editor.SelectedLayer).PaintLine((Vector2I)PreviousGlobalMousePos, (Vector2I)Screen.Camera.GlobalMousePos, SelectedColor, 1f, 0, CommandManager.AddCommandChain(new CommandChain()));
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
            if (!Editor.Focused) return;
            
            if (InputManager.IsKeyDown(Keys.LeftAlt)) {
                if (Editor.SelectedLayer is PaintableLayer2D paintableLayer) {
                    var color = paintableLayer.GetColor((Vector2I)Screen.Camera.GlobalMousePos.Value);
                    if (color != null) {
                        SelectedColor = color.Value;
                        ColorPicker.SetColor(color.Value);
                    }
                }
            } else {
                if (Editor.SelectedLayer is PaintableLayer2D paintableLayer) {
                    Brush.Paint(paintableLayer, InputManager.IsMouseButtonPressed(MouseButtons.LeftButton), SelectedColor, Screen.Camera);
                }
            }
        }

        public void HandleRightClick() {
            if (!Editor.Focused) return;

            if (InputManager.IsKeyDown(Keys.LeftAlt)) {

            } else {
                if (Editor.SelectedLayer is PaintableLayer2D paintableLayer) {
                    Brush.Paint(paintableLayer, InputManager.IsMouseButtonPressed(MouseButtons.RightButton), Color.Transparent, Screen.Camera);
                }
            }
        }

        public override void Update() { }

        public override void Draw() {
            if (Editor.Focused && PaintModeState == PaintModeState.Idle) Screen.Camera.DrawPoint((Vector2I)Screen.Camera.GlobalMousePos, SelectedColor * 0.5f);
            
            if (Editor.Focused && PreviousGlobalMousePos is not null) {
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
        }
    }

}
namespace Somniloquy {
    using System;
    using System.Collections.Generic;
    
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Microsoft.Xna.Framework.Input;

    public abstract class EditorMode {
        public Section2DScreen Screen;
        public Section2DEditor Editor;

        public List<Keybind> Keybinds = new();
        public List<Func<string>> DebugBinds = new();

        public EditorMode(Section2DScreen screen, Section2DEditor editor) {
            Screen = screen;
            Editor = editor;
        }
        
        public Vector2 ToLayerPos(Vector2 worldPos) {
            return Editor.SelectedLayer.ToLayerPos(worldPos);
        }

        public abstract void LoadContent();
        public abstract void Update();
        public abstract void Draw();
        public virtual void UnloadContent() {
            Keybinds.ForEach(keybind => InputManager.UnregisterKeybind(keybind));
            DebugBinds.ForEach(debugBind => DebugInfo.Unsubscribe(debugBind));
        }
    }
    
    public enum PaintModeState { Idle, Rectangle, Line, Select, Block }

    public class PaintMode : EditorMode {
        public PaintModeState PaintModeState = PaintModeState.Idle;
        public Vector2? PreviousGlobalMousePos;

        public Texture2D SelectedTexture;
        public Color SelectedColor = Color.White;
        public ColorPicker ColorPicker;

        private int currentBrushIndex;
        public Brush Brush => Brush.BrushTypes[currentBrushIndex];

        public PaintMode(Section2DScreen screen, Section2DEditor editor) : base(screen, editor) {
            Keybinds.Add(InputManager.RegisterKeybind(new object[] { Keys.LeftShift }, new object[] { Keys.LeftControl, Keys.LeftAlt }, _ => PaintRectangle(), _ => PostRectangle(), TriggerOnce.False));
            Keybinds.Add(InputManager.RegisterKeybind(new object[] { Keys.LeftControl}, new object[] { Keys.LeftAlt }, _ => PaintLine(), _ => PostLine(), TriggerOnce.False));
            Keybinds.Add(InputManager.RegisterKeybind(new object[] { Keys.F, MouseButtons.LeftButton}, new object[] { }, _ => Fill(), TriggerOnce.True));

            Keybinds.Add(InputManager.RegisterKeybind(new object[] { MouseButtons.LeftButton }, new object[] { Keys.LeftShift, Keys.LeftControl, Keys.LeftAlt, Keys.F }, _ => Paint(), TriggerOnce.False));
            Keybinds.Add(InputManager.RegisterKeybind(new object[] { MouseButtons.RightButton }, new object[] { Keys.LeftShift, Keys.LeftControl, Keys.LeftAlt, Keys.F }, _ => Erase(), TriggerOnce.False));

            Keybinds.Add(InputManager.RegisterKeybind(new object[] { Keys.LeftAlt, MouseButtons.LeftButton }, new object[] { Keys.LeftControl }, _ => SelectColor(), TriggerOnce.False));
            Keybinds.Add(InputManager.RegisterKeybind(new object[] { Keys.LeftShift, Keys.C, MouseButtons.LeftButton }, new object[] { }, _ => SelectTexture(), _ => CopyTexture(), TriggerOnce.False));
            Keybinds.Add(InputManager.RegisterKeybind(new object[] { Keys.LeftShift, Keys.X, MouseButtons.LeftButton }, new object[] { }, _ => SelectTexture(), _ => CutTexture(), TriggerOnce.False));
            Keybinds.Add(InputManager.RegisterKeybind(new object[] { Keys.LeftShift, Keys.V, MouseButtons.LeftButton }, new object[] { }, _ => PasteTexture(), TriggerOnce.False));
            
            Keybinds.Add(InputManager.RegisterKeybind(new object[] { Keys.Space, Keys.LeftShift, MouseButtons.LeftButton }, new object[] { Keys.LeftControl }, _ => SelectTexture(), _ => SeperateLayer(new TextureLayer2D()), TriggerOnce.True));
            Keybinds.Add(InputManager.RegisterKeybind(new object[] { Keys.Space, Keys.LeftShift, Keys.T, MouseButtons.LeftButton }, new object[] { Keys.LeftControl }, _ => SelectTexture(), _ => SeperateLayer(new TileLayer2D()), TriggerOnce.True));
            Keybinds.Add(InputManager.RegisterKeybind(new object[] { Keys.Space, Keys.V }, new object[] { Keys.LeftControl }, _ => MergeLayer(), TriggerOnce.True));

            DebugBinds.Add(DebugInfo.Subscribe(() => $"Selected Brush: {Brush}"));
            DebugBinds.Add(DebugInfo.Subscribe(() => $"Selected Color: {SelectedColor}"));
            DebugBinds.Add(DebugInfo.Subscribe(() => $"Pen Pressure: {InputManager.AveragePenPressure} Tilt: {InputManager.PenTilt}"));
        
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

        public override void UnloadContent() {
            base.UnloadContent();
            ScreenManager.Screens.Remove(ColorPicker.HuePicker);
            ScreenManager.Screens.Remove(ColorPicker);
            ColorPicker = null;
        }

        public void PaintRectangle() {
            if (PreviousGlobalMousePos is null) PreviousGlobalMousePos = Editor.Camera.GlobalMousePos;
            PaintModeState = PaintModeState.Rectangle;

            if (InputManager.IsMouseButtonPressed(MouseButtons.LeftButton)) {
                if (PreviousGlobalMousePos is null) return;
                var chain = CommandManager.AddCommandChain(new CommandChain());
                ((PaintableLayer2D)Editor.SelectedLayer).PaintRectangle((Vector2I)ToLayerPos(PreviousGlobalMousePos.Value), (Vector2I)ToLayerPos(Editor.Camera.GlobalMousePos.Value), SelectedColor, 1f, true, chain);
                PreviousGlobalMousePos = Editor.Camera.GlobalMousePos;
            }
        }

        public void PaintLine() {
            if (PreviousGlobalMousePos is null) PreviousGlobalMousePos = Editor.Camera.GlobalMousePos;
            PaintModeState = PaintModeState.Line;

            if (InputManager.IsMouseButtonPressed(MouseButtons.LeftButton)) {
                if (PreviousGlobalMousePos is null) return;
                var chain = CommandManager.AddCommandChain(new CommandChain());
                if (InputManager.IsKeyDown(Keys.LeftShift)) {
                    ((PaintableLayer2D)Editor.SelectedLayer).PaintSnappedLine((Vector2I)ToLayerPos(PreviousGlobalMousePos.Value), (Vector2I)ToLayerPos(Editor.Camera.GlobalMousePos.Value), SelectedColor, 1f, 0, chain);
                    PreviousGlobalMousePos = PixelActions.ApplySnappedLineAction((Vector2I)ToLayerPos(PreviousGlobalMousePos.Value), (Vector2I)ToLayerPos(Editor.Camera.GlobalMousePos.Value), 0, _ => { });
                } else {
                    ((PaintableLayer2D)Editor.SelectedLayer).PaintLine((Vector2I)ToLayerPos(PreviousGlobalMousePos.Value), (Vector2I)ToLayerPos(Editor.Camera.GlobalMousePos.Value), SelectedColor, 1f, 0, chain);
                    PreviousGlobalMousePos = Editor.Camera.GlobalMousePos;
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

        public void SelectTexture() {
            if (!Editor.Focused || PaintModeState == PaintModeState.Block) return;
            if (Editor.SelectedLayer is PaintableLayer2D layer) {
                if (PreviousGlobalMousePos is null) {
                    PreviousGlobalMousePos = Editor.Camera.GlobalMousePos;
                    PaintModeState = PaintModeState.Select;
                }
                if (InputManager.IsMouseButtonPressed(MouseButtons.LeftButton)) {
                    if (PreviousGlobalMousePos is null) return;
                    var (start, end) = Vector2Extensions.Rationalize(PreviousGlobalMousePos.Value, Editor.Camera.GlobalMousePos.Value);
                    SelectedTexture = layer.GetTexture(new Rectangle((Vector2I)start, (Vector2I)(end - start)));
                }
            }
        }

        public void CopyTexture() {
            if (SelectedTexture is null) return;
            PreviousGlobalMousePos = null;
            PaintModeState = PaintModeState.Block;
        }

        public void CutTexture() {
            PreviousGlobalMousePos = null;
            PaintModeState = PaintModeState.Block;
        }

        public void SeperateLayer(PaintableLayer2D layer) {
            Editor.SelectedLayer.AddLayer(layer);
            var (start, end) = Vector2Extensions.Rationalize(PreviousGlobalMousePos.Value, Editor.Camera.GlobalMousePos.Value);
            layer.PaintTexture(new Rectangle((Vector2I)start, (Vector2I)(end - start)), SelectedTexture, 1f);
            CutTexture();
        }

        public void PasteTexture() {
            if (!Editor.Focused || PaintModeState == PaintModeState.Block) return;
            if (SelectedTexture is null) return;
            if (Editor.SelectedLayer is PaintableLayer2D layer) {
                layer.PaintTexture((Vector2I)ToLayerPos(Editor.Camera.GlobalMousePos.Value), SelectedTexture, 1f);
            }
        }

        public void MergeLayer() {
            if (!Editor.Focused || PaintModeState == PaintModeState.Block) return;
            if (Editor.SelectedLayer.Parent is PaintableLayer2D parent && Editor.SelectedLayer is PaintableLayer2D layer) {
                parent.PaintTexture(layer.GetTextureBounds(), layer.GetTexture(), 1f);
                layer.Dispose();
            }
        }

        public void SelectColor() {
            if (!Editor.Focused) return;
            if (Editor.SelectedLayer is PaintableLayer2D layer) {
                if (InputManager.IsKeyDown(Keys.LeftShift)) {
                    if (!InputManager.IsMouseButtonPressed(MouseButtons.LeftButton)) return;
                    var color = layer.GetColor((Vector2I)ToLayerPos(Editor.Camera.GlobalMousePos.Value));
                    if (color != null) {
                        color = SelectedColor.BlendWith(color.Value, 0.5f);
                        SelectedColor = color.Value;
                        ColorPicker.SetColor(color.Value);
                    }
                } else {
                    var color = layer.GetColor((Vector2I)ToLayerPos(Editor.Camera.GlobalMousePos.Value));
                    if (color != null) {
                        SelectedColor = color.Value;
                        ColorPicker.SetColor(color.Value);
                    }
                }
            }
        }

        public void Paint() {
            if (!Editor.Focused) return;
            if (Editor.SelectedLayer is PaintableLayer2D paintableLayer) {
                Brush.Paint(paintableLayer, InputManager.IsMouseButtonPressed(MouseButtons.LeftButton), SelectedColor, Editor.Camera);
            }
        }

        public void Erase() {
            if (!Editor.Focused) return;

            if (Editor.SelectedLayer is PaintableLayer2D paintableLayer) {
                Brush.Paint(paintableLayer, InputManager.IsMouseButtonPressed(MouseButtons.RightButton), Color.Transparent, Editor.Camera);
            }
        }

        public void Fill() {
            if (!Editor.Focused) return;

            if (Editor.SelectedLayer is PaintableLayer2D paintableLayer) {
                paintableLayer.Fill((Vector2I)ToLayerPos(Editor.Camera.GlobalMousePos.Value), SelectedColor);
            }
        }

        public override void Update() { }

        public override void Draw() {
            Editor.Camera.SB.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, transformMatrix: Editor.Camera.Transform);
            if (Editor.Focused && PaintModeState == PaintModeState.Idle) Editor.Camera.DrawPoint((Vector2I)ToLayerPos(Editor.Camera.GlobalMousePos.Value), SelectedColor * 0.5f);
            
            if (Editor.Focused && PreviousGlobalMousePos is not null) {
                if (PaintModeState == PaintModeState.Line) {
                    if (InputManager.IsKeyDown(Keys.LeftShift)) {
                        PixelActions.ApplySnappedLineAction((Vector2I)ToLayerPos(PreviousGlobalMousePos.Value), (Vector2I)ToLayerPos(Editor.Camera.GlobalMousePos.Value), 0, (pos) => Editor.Camera.SB.Draw(SQ.SB.Pixel, pos, SelectedColor * 0.5f));
                    } else {
                        PixelActions.ApplyLineAction((Vector2I)ToLayerPos(PreviousGlobalMousePos.Value), (Vector2I)ToLayerPos(Editor.Camera.GlobalMousePos.Value), 0, (pos) => Editor.Camera.SB.Draw(SQ.SB.Pixel, pos, SelectedColor * 0.5f));
                    }
                } else if (PaintModeState == PaintModeState.Rectangle) {
                    PixelActions.ApplyRectangleAction((Vector2I)ToLayerPos(PreviousGlobalMousePos.Value), (Vector2I)ToLayerPos(Editor.Camera.GlobalMousePos.Value), true, (pos) => Editor.Camera.SB.Draw(SQ.SB.Pixel, pos, SelectedColor * 0.5f));
                } else if (PaintModeState == PaintModeState.Select) {
                    PixelActions.ApplyRectangleAction((Vector2I)ToLayerPos(PreviousGlobalMousePos.Value), (Vector2I)ToLayerPos(Editor.Camera.GlobalMousePos.Value), true, (pos) => Editor.Camera.SB.Draw(SQ.SB.Pixel, pos, Color.White * 0.1f));
                } 
            }
        }
    }
}
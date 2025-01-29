namespace Somniloquy {
    using System;
    using System.Collections.Generic;
    
    using Microsoft.Xna.Framework;
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

        public abstract void LoadContent();
        public abstract void Update();
        public abstract void Draw();
        public virtual void UnloadContent() {
            Keybinds.ForEach(keybind => InputManager.UnregisterKeybind(keybind));
            DebugBinds.ForEach(debugBind => DebugInfo.Unsubscribe(debugBind));
        }
    }
    
    public enum PaintModeState { Idle, Rectangle, Line, Select }

    public class PaintMode : EditorMode {
        public PaintModeState PaintModeState = PaintModeState.Idle;
        public Vector2? PreviousGlobalMousePos;

        public Color SelectedColor = Color.White;
        public ColorPicker ColorPicker;

        private int currentBrushIndex;
        public Brush Brush => Brush.BrushTypes[currentBrushIndex];

        public PaintMode(Section2DScreen screen, Section2DEditor editor) : base(screen, editor) {
            Keybinds.Add(InputManager.RegisterKeybind(new object[] { Keys.LeftShift }, new object[] { Keys.LeftControl }, _ => PaintRectangle(), _ => PostRectangle(), TriggerOnce.False));
            Keybinds.Add(InputManager.RegisterKeybind(new object[] { Keys.LeftControl}, new object[] { }, _ => PaintLine(), _ => PostLine(), TriggerOnce.False));

            Keybinds.Add(InputManager.RegisterKeybind(new object[] { MouseButtons.LeftButton }, new object[] { Keys.LeftShift, Keys.LeftControl, Keys.LeftAlt }, _ => Paint(), TriggerOnce.False));
            Keybinds.Add(InputManager.RegisterKeybind(new object[] { MouseButtons.RightButton }, new object[] { Keys.LeftShift, Keys.LeftControl, Keys.LeftAlt }, _ => Erase(), TriggerOnce.False));

            Keybinds.Add(InputManager.RegisterKeybind(new object[] { Keys.LeftAlt, MouseButtons.LeftButton }, new object[] { Keys.LeftShift, Keys.LeftControl }, _ => SelectColor(), TriggerOnce.False));

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
            if (PreviousGlobalMousePos is null) PreviousGlobalMousePos = Screen.Camera.GlobalMousePos;
            PaintModeState = PaintModeState.Rectangle;

            if (InputManager.IsMouseButtonPressed(MouseButtons.LeftButton)) {
                if (PreviousGlobalMousePos is null) return;
                var chain = CommandManager.AddCommandChain(new CommandChain());
                ((PaintableLayer2D)Editor.SelectedLayer).PaintRectangle((Vector2I)PreviousGlobalMousePos, (Vector2I)Screen.Camera.GlobalMousePos, SelectedColor, 1f, true, chain);
                PreviousGlobalMousePos = Screen.Camera.GlobalMousePos;
            }
        }

        public void PaintLine() {
            if (PreviousGlobalMousePos is null) PreviousGlobalMousePos = Screen.Camera.GlobalMousePos;
            PaintModeState = PaintModeState.Line;

            if (InputManager.IsMouseButtonPressed(MouseButtons.LeftButton)) {
                if (PreviousGlobalMousePos is null) return;
                var chain = CommandManager.AddCommandChain(new CommandChain());
                if (InputManager.IsKeyDown(Keys.LeftShift)) {
                    ((PaintableLayer2D)Editor.SelectedLayer).PaintSnappedLine((Vector2I)PreviousGlobalMousePos, (Vector2I)Screen.Camera.GlobalMousePos, SelectedColor, 1f, 0, chain);
                    PreviousGlobalMousePos = PixelActions.ApplySnappedLineAction((Vector2I)PreviousGlobalMousePos, (Vector2I)Screen.Camera.GlobalMousePos, 0, _ => { });
                } else {
                    ((PaintableLayer2D)Editor.SelectedLayer).PaintLine((Vector2I)PreviousGlobalMousePos, (Vector2I)Screen.Camera.GlobalMousePos, SelectedColor, 1f, 0, chain);
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

        public void SelectColor() {
            if (!Editor.Focused) return;
            if (Editor.SelectedLayer is PaintableLayer2D layer) {
                var color = layer.GetColor((Vector2I)Screen.Camera.GlobalMousePos.Value);
                if (color != null) {
                    SelectedColor = color.Value;
                    ColorPicker.SetColor(color.Value);
                }
            }
        }

        public void Paint() {
            if (!Editor.Focused) return;
            if (Editor.SelectedLayer is PaintableLayer2D paintableLayer) {
                Brush.Paint(paintableLayer, InputManager.IsMouseButtonPressed(MouseButtons.LeftButton), SelectedColor, Screen.Camera);
            }
        }

        public void Erase() {
            if (!Editor.Focused) return;

            if (Editor.SelectedLayer is PaintableLayer2D paintableLayer) {
                Brush.Paint(paintableLayer, InputManager.IsMouseButtonPressed(MouseButtons.RightButton), Color.Transparent, Screen.Camera);
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

    public enum TileModeState { Idle, Rectangle, Line, Select }

    public class TileMode : EditorMode {
        public TileModeState TileModeState = TileModeState.Idle;
        public Vector2I? PreviousTilePos;

        public Tile2D[,] SelectedTiles;

        public TileMode(Section2DScreen screen, Section2DEditor editor) : base(screen, editor) {
            Keybinds.Add(InputManager.RegisterKeybind(new object[] { Keys.LeftShift }, new object[] { Keys.LeftControl, Keys.LeftAlt }, _ => PaintRectangle(), _ => PostRectangle(), TriggerOnce.False));
            Keybinds.Add(InputManager.RegisterKeybind(new object[] { Keys.LeftControl}, new object[] { Keys.LeftAlt }, _ => PaintLine(), _ => PostLine(), TriggerOnce.False));
        
            Keybinds.Add(InputManager.RegisterKeybind(new object[] { MouseButtons.LeftButton }, new object[] { Keys.LeftShift, Keys.LeftControl, Keys.LeftAlt }, _ => PaintTile(), TriggerOnce.False));
            Keybinds.Add(InputManager.RegisterKeybind(new object[] { MouseButtons.RightButton }, new object[] { Keys.LeftShift, Keys.LeftControl, Keys.LeftAlt }, _ => EraseTile(), TriggerOnce.False));

            Keybinds.Add(InputManager.RegisterKeybind(new object[] { Keys.LeftAlt, MouseButtons.LeftButton }, new object[] { Keys.LeftShift, Keys.LeftControl }, _ => SelectTile(), TriggerOnce.False));
            Keybinds.Add(InputManager.RegisterKeybind(new object[] { Keys.LeftShift, Keys.LeftAlt, MouseButtons.LeftButton }, new object[] { Keys.LeftControl }, _ => SelectTile(), TriggerOnce.False));

            DebugBinds.Add(DebugInfo.Subscribe(() => { 
                if (SelectedTiles is null) return "Selected Tiles: None";
                return $"Selected Tiles: {SelectedTiles.GetLength(0)}x{SelectedTiles.GetLength(1)}";
            }));
        }

        public override void LoadContent() {
            
        }

        public override void UnloadContent() {
            base.UnloadContent();
        }

        public void SelectTile() {
            if (!Editor.Focused) return;
            if (Editor.SelectedLayer is TileLayer2D layer) {
                SelectedTiles = new Tile2D[1, 1];
                SelectedTiles[0, 0] = layer.GetTile(layer.GetTilePosition((Vector2I)Screen.Camera.GlobalMousePos));
            }
        }

        public void PaintTile() {
            if (!Editor.Focused) return;
            if (Editor.SelectedLayer is TileLayer2D layer) {
                if (SelectedTiles is not null && SelectedTiles[0, 0] is not null) layer.SetTile(layer.GetTilePosition((Vector2I)Screen.Camera.GlobalMousePos), SelectedTiles[0, 0], null);
            }
        }

        public void EraseTile() {
            if (!Editor.Focused) return;
            if (Editor.SelectedLayer is TileLayer2D layer) {
                layer.SetTile(layer.GetTilePosition((Vector2I)Screen.Camera.GlobalMousePos), null, null);
            }
        }

        public void PaintRectangle() {
            if (!Editor.Focused) return;
            if (SelectedTiles is null || SelectedTiles[0, 0] is null) return;
            if (Editor.SelectedLayer is TileLayer2D layer) {
                if (PreviousTilePos is null) PreviousTilePos = layer.GetTilePosition((Vector2I)Screen.Camera.GlobalMousePos);
                TileModeState = TileModeState.Rectangle;

                if (InputManager.IsMouseButtonPressed(MouseButtons.LeftButton)) {
                    if (PreviousTilePos is null) return;
                    var chain = CommandManager.AddCommandChain(new CommandChain());
                    PixelActions.ApplyRectangleAction(PreviousTilePos.Value, layer.GetTilePosition((Vector2I)Screen.Camera.GlobalMousePos), true, (pos) => layer.SetTile(pos, SelectedTiles[0, 0], chain));
                    PreviousTilePos = layer.GetTilePosition((Vector2I)Screen.Camera.GlobalMousePos);
                }
            }
        }

        public void PaintLine() {
            if (!Editor.Focused) return;
            if (SelectedTiles is null || SelectedTiles[0, 0] is null) return;
            if (Editor.SelectedLayer is TileLayer2D layer) {
                if (PreviousTilePos is null) PreviousTilePos = layer.GetTilePosition((Vector2I)Screen.Camera.GlobalMousePos);
                TileModeState = TileModeState.Line;

                if (InputManager.IsMouseButtonPressed(MouseButtons.LeftButton)) {
                    if (PreviousTilePos is null) return;
                    var chain = CommandManager.AddCommandChain(new CommandChain());
                    if (InputManager.IsKeyDown(Keys.LeftShift)) {
                        PixelActions.ApplySnappedLineAction(PreviousTilePos.Value, layer.GetTilePosition((Vector2I)Screen.Camera.GlobalMousePos), 0, (pos) => layer.SetTile(pos, SelectedTiles[0, 0], chain));
                        PreviousTilePos = PixelActions.ApplySnappedLineAction((Vector2I)PreviousTilePos, layer.GetTilePosition((Vector2I)Screen.Camera.GlobalMousePos), 0, _ => { });
                    } else {
                        PixelActions.ApplyLineAction(PreviousTilePos.Value, layer.GetTilePosition((Vector2I)Screen.Camera.GlobalMousePos), 0, (pos) => layer.SetTile(pos, SelectedTiles[0, 0], chain));
                        PreviousTilePos = layer.GetTilePosition((Vector2I)Screen.Camera.GlobalMousePos);
                    }
                }
            }
        }

        public void PostRectangle() {
            PreviousTilePos = null;
            TileModeState = TileModeState.Idle;
        }

        public void PostLine() {
            PreviousTilePos = null;
            TileModeState = TileModeState.Idle;
        }
        
        public override void Update() {
            
        }

        public override void Draw() {
            if (Editor.SelectedLayer is TileLayer2D layer) {

                if (Editor.Focused && TileModeState == TileModeState.Idle) {
                    var tilePos = layer.GetTilePosition((Vector2I)Screen.Camera.GlobalMousePos);
                    if (SelectedTiles is not null && SelectedTiles[0, 0] is not null) SelectedTiles[0, 0].Draw(Screen.Camera, new Rectangle(tilePos * layer.TileLength, new(layer.TileLength)), 0.25f);
                    Screen.Camera.DrawFilledRectangle(new RectangleF(tilePos * layer.TileLength, new(layer.TileLength)), Color.White * 0.1f);
                } else if (Editor.Focused && PreviousTilePos is not null) {
                    if (TileModeState == TileModeState.Line) {
                    if (InputManager.IsKeyDown(Keys.LeftShift)) {
                        PixelActions.ApplySnappedLineAction(PreviousTilePos.Value, layer.GetTilePosition((Vector2I)Screen.Camera.GlobalMousePos), 0, (pos) => SelectedTiles[0, 0].Draw(Screen.Camera, new Rectangle(pos * layer.TileLength, new(layer.TileLength)), 0.25f));
                    } else {
                        PixelActions.ApplyLineAction(PreviousTilePos.Value, layer.GetTilePosition((Vector2I)Screen.Camera.GlobalMousePos), 0, (pos) => SelectedTiles[0, 0].Draw(Screen.Camera, new Rectangle(pos * layer.TileLength, new(layer.TileLength)), 0.25f));
                    }
                } else if (TileModeState == TileModeState.Rectangle) {
                    PixelActions.ApplyRectangleAction(PreviousTilePos.Value, layer.GetTilePosition((Vector2I)Screen.Camera.GlobalMousePos), true, (pos) => SelectedTiles[0, 0].Draw(Screen.Camera, new Rectangle(pos * layer.TileLength, new(layer.TileLength)), 0.25f));
                }
                }
            }
        }
    }
}
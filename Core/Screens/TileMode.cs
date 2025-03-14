namespace Somniloquy {
    using System;
    using System.Collections.Generic;
    
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Microsoft.Xna.Framework.Input;

    public enum TileModeState { Idle, Rectangle, Line, Select, Block }

    public class TileMode : EditorMode {
        public TileModeState TileModeState = TileModeState.Idle;
        public Vector2I? PreviousTilePos;

        public Tile2D[,] SelectedTiles;
        public CommandChain CurrentChain;

        public TileMode(Section2DScreen screen, Section2DEditor editor) : base(screen, editor) {
            Keybinds.Add(InputManager.RegisterKeybind(new object[] { Keys.LeftShift }, new object[] { Keys.LeftControl, Keys.LeftAlt, Keys.F }, _ => PaintRectangle(), _ => PostRectangle(), TriggerOnce.False));
            Keybinds.Add(InputManager.RegisterKeybind(new object[] { Keys.LeftControl}, new object[] { Keys.LeftAlt, Keys.F }, _ => PaintLine(), _ => PostLine(), TriggerOnce.False));
            Keybinds.Add(InputManager.RegisterKeybind(new object[] { Keys.F, MouseButtons.LeftButton}, new object[] { }, _ => FillTile(), TriggerOnce.True));
        
            Keybinds.Add(InputManager.RegisterKeybind(new object[] { MouseButtons.LeftButton }, new object[] { Keys.LeftShift, Keys.LeftControl, Keys.LeftAlt, Keys.F }, _ => PaintTile(), TriggerOnce.False));
            Keybinds.Add(InputManager.RegisterKeybind(new object[] { MouseButtons.RightButton }, new object[] { Keys.LeftShift, Keys.LeftControl, Keys.LeftAlt, Keys.F }, _ => EraseTile(), TriggerOnce.False));

            Keybinds.Add(InputManager.RegisterKeybind(new object[] { Keys.LeftAlt, MouseButtons.LeftButton }, new object[] { Keys.LeftShift, Keys.LeftControl, Keys.F }, _ => SelectTile(), TriggerOnce.False));
            Keybinds.Add(InputManager.RegisterKeybind(new object[] { Keys.LeftShift, Keys.LeftAlt }, new object[] { Keys.LeftControl, Keys.F }, _ => SelectTiles(), _ => PostSelectTiles(), TriggerOnce.False));

            DebugBinds.Add(DebugInfo.Subscribe(() => {
                if (SelectedTiles is null) return "Selected Tiles: None";
                return $"Selected Tiles: {SelectedTiles.GetLength(0)}x{SelectedTiles.GetLength(1)}";
            }));
            LayerTable.BuildUI();
        }

        public override void LoadContent() {
            
        }

        public override void UnloadContent() {
            base.UnloadContent();
        }

        private Vector2I GetTilePos(TileLayer2D layer) {
            return layer.GetTilePosition((Vector2I)ToLayerPos(Editor.Camera.GlobalMousePos.Value));
        }

        public void SelectTile() {
            if (!Editor.Focused || TileModeState == TileModeState.Block) return;
            if (Editor.SelectedLayer is TileLayer2D layer) {
                SelectedTiles = new Tile2D[1, 1];
                SelectedTiles[0, 0] = layer.GetTile(GetTilePos(layer));
            }
        }

        public void SelectTiles() {
            if (!Editor.Focused || TileModeState == TileModeState.Block) return;
            if (Editor.SelectedLayer is TileLayer2D layer) {
                if (PreviousTilePos is null) {
                    PreviousTilePos = GetTilePos(layer);
                    TileModeState = TileModeState.Select;
                }
                if (InputManager.IsMouseButtonPressed(MouseButtons.LeftButton)) {
                    if (PreviousTilePos is null) return;
                    var (start, end) = Vector2Extensions.Rationalize(PreviousTilePos.Value, GetTilePos(layer));
                    SelectedTiles = layer.GetTiles(start, end);
                }
            }
        }

        public void PostSelectTiles() {
            PreviousTilePos = null;
            TileModeState = TileModeState.Block;
        }

        public void PaintTile() {
            if (!Editor.Focused || TileModeState == TileModeState.Block) return;
            if (Editor.SelectedLayer is TileLayer2D layer && SelectedTiles is not null && SelectedTiles[0, 0] is not null) {
                if (PreviousTilePos is null) {
                    PreviousTilePos = GetTilePos(layer);
                    CurrentChain = CommandManager.AddCommandChain(new());
                    layer.SetTile(PreviousTilePos.Value, SelectedTiles[0, 0], CurrentChain);
                } else {
                    var currentTilePos = GetTilePos(layer);
                    var displacement = currentTilePos - PreviousTilePos;
                    layer.SetTile(currentTilePos, SelectedTiles[Util.PosMod(displacement.Value.X, SelectedTiles.GetLength(0)), Util.PosMod(displacement.Value.Y, SelectedTiles.GetLength(1))], CurrentChain);
                }
            }
        }

        public void EraseTile() {
            if (!Editor.Focused || TileModeState == TileModeState.Block) return;
            if (Editor.SelectedLayer is TileLayer2D layer) {
                layer.SetTile(GetTilePos(layer), null, null);
            }
        }

        public void PaintRectangle() {
            if (!Editor.Focused || TileModeState == TileModeState.Block) return;
            if (SelectedTiles is null) return;
            if (Editor.SelectedLayer is TileLayer2D layer) {
                if (PreviousTilePos is null) PreviousTilePos = GetTilePos(layer);
                TileModeState = TileModeState.Rectangle;

                if (InputManager.IsMouseButtonPressed(MouseButtons.LeftButton)) {
                    if (PreviousTilePos is null) return;
                    var chain = CommandManager.AddCommandChain(new CommandChain());
                    PixelActions.ApplyRectangleAction(PreviousTilePos.Value, GetTilePos(layer), true, (pos) => {
                        var displacement = pos - PreviousTilePos;
                        layer.SetTile(pos, SelectedTiles[Util.PosMod(displacement.Value.X, SelectedTiles.GetLength(0)), Util.PosMod(displacement.Value.Y, SelectedTiles.GetLength(1))], chain);
                    });
                    
                    PreviousTilePos = GetTilePos(layer);
                }
            }
        }

        public void PaintLine() {
            if (!Editor.Focused || TileModeState == TileModeState.Block) return;
            if (SelectedTiles is null) return;
            if (Editor.SelectedLayer is TileLayer2D layer) {
                if (PreviousTilePos is null) PreviousTilePos = GetTilePos(layer);
                TileModeState = TileModeState.Line;

                if (InputManager.IsMouseButtonPressed(MouseButtons.LeftButton)) {
                    if (PreviousTilePos is null) return;
                    var chain = CommandManager.AddCommandChain(new CommandChain());
                    if (InputManager.IsKeyDown(Keys.LeftShift)) {
                        PixelActions.ApplySnappedLineAction(PreviousTilePos.Value, GetTilePos(layer), 0, (pos) => {
                            var displacement = pos - PreviousTilePos;
                            layer.SetTile(pos, SelectedTiles[Util.PosMod(displacement.Value.X, SelectedTiles.GetLength(0)), Util.PosMod(displacement.Value.Y, SelectedTiles.GetLength(1))], chain);
                        });
                        PreviousTilePos = PixelActions.ApplySnappedLineAction((Vector2I)PreviousTilePos, GetTilePos(layer), 0, _ => { });
                    } else {
                        PixelActions.ApplyLineAction(PreviousTilePos.Value, GetTilePos(layer), 0, (pos) => {
                            var displacement = pos - PreviousTilePos;
                            layer.SetTile(pos, SelectedTiles[Util.PosMod(displacement.Value.X, SelectedTiles.GetLength(0)), Util.PosMod(displacement.Value.Y, SelectedTiles.GetLength(1))], chain);
                        });
                        PreviousTilePos = GetTilePos(layer);
                    }
                }
            }
        }

        public void PostRectangle() {
            PreviousTilePos = null;
            TileModeState = TileModeState.Block;
        }

        public void PostLine() {
            PreviousTilePos = null;
            TileModeState = TileModeState.Block;
        }

        public void FillTile() {
            if (!Editor.Focused) return;

            if (Editor.SelectedLayer is TileLayer2D layer) {
                layer.FillTile(GetTilePos(layer), SelectedTiles);
            }
        }

        
        public override void Update() {
            if (TileModeState == TileModeState.Block && !InputManager.IsMouseButtonDown(MouseButtons.LeftButton)) {
                TileModeState = TileModeState.Idle;
            }
        }

        public override void Draw() {
            if (Editor.SelectedLayer is TileLayer2D layer) {
                Editor.Camera.SB.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, transformMatrix: Editor.Camera.Transform);

                if (Editor.Focused && TileModeState == TileModeState.Idle || TileModeState == TileModeState.Block) {
                    var tilePos = GetTilePos(layer);
                    if (SelectedTiles is not null && SelectedTiles[0, 0] is not null) SelectedTiles[0, 0].Draw(Editor.Camera, new Rectangle(tilePos * layer.TileLength, new(layer.TileLength)), 0.25f);
                    Editor.Camera.DrawFilledRectangle(new RectangleF(tilePos * layer.TileLength, new(layer.TileLength)), Color.White * 0.1f);
                } else if (Editor.Focused && PreviousTilePos is not null) {
                    if (TileModeState == TileModeState.Line) {
                        if (InputManager.IsKeyDown(Keys.LeftShift)) {
                            PixelActions.ApplySnappedLineAction(PreviousTilePos.Value, GetTilePos(layer), 0, (pos) => {
                                var displacement = pos - PreviousTilePos;
                                SelectedTiles[Util.PosMod(displacement.Value.X, SelectedTiles.GetLength(0)), Util.PosMod(displacement.Value.Y, SelectedTiles.GetLength(1))]?.Draw(Editor.Camera, new Rectangle(pos * layer.TileLength, new(layer.TileLength)), 0.25f);
                            });
                        } else {
                            PixelActions.ApplyLineAction(PreviousTilePos.Value, GetTilePos(layer), 0, (pos) => {
                                var displacement = pos - PreviousTilePos;
                                SelectedTiles[Util.PosMod(displacement.Value.X, SelectedTiles.GetLength(0)), Util.PosMod(displacement.Value.Y, SelectedTiles.GetLength(1))]?.Draw(Editor.Camera, new Rectangle(pos * layer.TileLength, new(layer.TileLength)), 0.25f);
                            });
                        }
                    } else if (TileModeState == TileModeState.Rectangle) {
                        PixelActions.ApplyRectangleAction(PreviousTilePos.Value, GetTilePos(layer), true, (pos) => {
                            var displacement = pos - PreviousTilePos;
                            SelectedTiles[Util.PosMod(displacement.Value.X, SelectedTiles.GetLength(0)), Util.PosMod(displacement.Value.Y, SelectedTiles.GetLength(1))]?.Draw(Editor.Camera, new Rectangle(pos * layer.TileLength, new(layer.TileLength)), 0.25f);
                        });
                    } else if (TileModeState == TileModeState.Select) {
                        var (start, end) = Vector2Extensions.Rationalize(PreviousTilePos.Value, GetTilePos(layer));
                        Editor.Camera.DrawFilledRectangle(new RectangleF(start * layer.TileLength, layer.TileLength * (end - start + new Vector2I(1))), Color.White * 0.1f);
                    }
                }
            }
        }
    }
}
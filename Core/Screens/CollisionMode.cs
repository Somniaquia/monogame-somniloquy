namespace Somniloquy {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Microsoft.Xna.Framework.Input;
    
    public class CollisionMode : EditorMode {
        public Vector2[] CollisionVertices;
        
        public CollisionMode(Section2DScreen screen, Section2DEditor editor) : base(screen, editor) {
            Keybinds.Add(InputManager.RegisterKeybind(new object[] { MouseButtons.LeftButton }, new object[] { Keys.LeftShift, Keys.LeftControl, Keys.LeftAlt, Keys.F, Keys.Space }, _ => PaintTile(), TriggerOnce.False));
            Keybinds.Add(InputManager.RegisterKeybind(new object[] { MouseButtons.RightButton }, new object[] { Keys.LeftShift, Keys.LeftControl, Keys.LeftAlt, Keys.F, Keys.Space }, _ => EraseTile(), TriggerOnce.False));
            Keybinds.Add(InputManager.RegisterKeybind(new object[] { Keys.LeftAlt, MouseButtons.LeftButton }, new object[] { Keys.LeftShift, Keys.LeftControl, Keys.F, Keys.Space }, _ => SelectTile(), TriggerOnce.False));
            Keybinds.Add(InputManager.RegisterKeybind(new object[] { Keys.Space, MouseButtons.LeftButton }, new object[] { Keys.LeftShift, Keys.LeftControl, Keys.F, Keys.LeftAlt }, _ => EditTile(), TriggerOnce.False));
            
        }

        public override void LoadContent() {
        }

        public void PaintTile() {

        }

        public void EraseTile() {

        }

        public void SelectTile() {

        }

        public void EditTile() {
            if (!Editor.Focused) return;
            if (Editor.SelectedLayer is not PaintableLayer2D) return;
            Editor.SwitchEditorMode(new CollisionEditMode(Editor.SelectedLayer, (Vector2I)Editor.Camera.GlobalMousePos.Value, Screen, Editor));
        }

        public override void Update() {
            
        }

        public override void Draw() {
            if (Editor.SelectedLayer is TileLayer2D tileLayer) {
                Editor.Camera.SB.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, transformMatrix: Editor.Camera.Transform);
                var tilePos = tileLayer.GetTilePosition((Vector2I)Editor.Camera.GlobalMousePos.Value);
                Editor.Camera.DrawFilledRectangle(new RectangleF(tilePos * tileLayer.TileLength, new(tileLayer.TileLength)), Color.Tomato * 0.1f);
            } else if (Editor.SelectedLayer is TextureLayer2D textureLayer) {
                Editor.Camera.SB.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, transformMatrix: Editor.Camera.Transform);
                var chunkPos = textureLayer.GetChunkPosition((Vector2I)Editor.Camera.GlobalMousePos.Value);
                Editor.Camera.DrawFilledRectangle(new RectangleF(chunkPos * textureLayer.ChunkLength, new(textureLayer.ChunkLength)), Color.Tomato * 0.1f);
            }
        }
    }

    public class CollisionEditMode : EditorMode {
        public Layer2D SelectedLayer;
        public Vector2I SelectedPos;

        public CollisionEditMode(Layer2D layer, Vector2I selectedPos, Section2DScreen screen, Section2DEditor editor) : base(screen, editor) {
            SelectedLayer = layer;
            SelectedPos = selectedPos;

            Screen.Section.Root.Hide();
            SelectedLayer.Show();
            SelectedLayer.Opacity = 0.5f;
            Keybinds.Add(InputManager.RegisterKeybind(new object[] { MouseButtons.LeftButton }, new object[] { Keys.LeftShift, Keys.LeftControl, Keys.LeftAlt, Keys.F }, _ => AddVertex(), TriggerOnce.False));
            Keybinds.Add(InputManager.RegisterKeybind(new object[] { MouseButtons.RightButton }, new object[] { Keys.LeftShift, Keys.LeftControl, Keys.LeftAlt, Keys.F }, _ => RemoveVertex(), TriggerOnce.False));
            LayerTable.DestroyUI();
        }

        private Vector2I GetMouseLocation() {
            return Util.Round(Editor.Camera.GlobalMousePos.Value);
        }

        public void Deselect() {
            Editor.SwitchEditorMode(new CollisionMode(Screen, Editor));
        }

        public void AddVertex() {
            if (!Editor.Focused) return;
            if (Editor.SelectedLayer is TileLayer2D tileLayer) {
                Vector2I tilePos = tileLayer.GetTilePosition(SelectedPos);
                Tile2D selectedTile = tileLayer.GetTile(tilePos);
                Vector2I pos = GetMouseLocation() - tilePos * tileLayer.TileLength;

                if (pos.X < 0 || pos.Y < 0 || pos.X > tileLayer.TileLength || pos.Y > tileLayer.TileLength) {
                    Deselect();
                    return;
                }

                // if (selectedTile is null) tileLayer.SetTile(tilePos, new Tile2D(), null);
                selectedTile.CollisionVertices ??= new();
                if (selectedTile.CollisionVertices.Contains(pos)) return;
                selectedTile.CollisionVertices.Add(pos);
            } else if (Editor.SelectedLayer is TextureLayer2D textureLayer) {
                Vector2I chunkPos = textureLayer.GetChunkPosition(SelectedPos);
                Vector2I pos = GetMouseLocation() - chunkPos * textureLayer.ChunkLength;

                if (pos.X < 0 || pos.Y < 0 || pos.X > textureLayer.ChunkLength || pos.Y > textureLayer.ChunkLength) {
                    Deselect();
                    return;
                }

                // if (!textureLayer.Chunks.ContainsKey(chunkPos)) textureLayer
                TextureChunk2D selectedChunk = textureLayer.Chunks[chunkPos];
                selectedChunk.CollisionVertices ??= new();
                if (selectedChunk.CollisionVertices.Contains(pos)) return;
                selectedChunk.CollisionVertices.Add(pos);
            }
        }

        public void RemoveVertex() {
            if (!Editor.Focused) return;
            if (Editor.SelectedLayer is TileLayer2D tileLayer) {
                Vector2I tilePos = tileLayer.GetTilePosition(SelectedPos);
                Tile2D selectedTile = tileLayer.GetTile(tilePos);
                Vector2I pos = GetMouseLocation() - tilePos * tileLayer.TileLength;

                if (pos.X < 0 || pos.Y < 0 || pos.X > tileLayer.TileLength || pos.Y > tileLayer.TileLength) {
                    Deselect();
                    return;
                }

                if (selectedTile.CollisionVertices is null || !selectedTile.CollisionVertices.Contains(pos)) return;
                selectedTile.CollisionVertices.Remove(pos);
            } else if (Editor.SelectedLayer is TextureLayer2D textureLayer) {
                Vector2I chunkPos = textureLayer.GetChunkPosition(SelectedPos);
                TextureChunk2D selectedChunk = textureLayer.Chunks[chunkPos];
                Vector2I pos = GetMouseLocation() - chunkPos * textureLayer.ChunkLength;

                if (pos.X < 0 || pos.Y < 0 || pos.X > textureLayer.ChunkLength || pos.Y > textureLayer.ChunkLength) {
                    Deselect();
                    return;
                }

                if (selectedChunk.CollisionVertices is null || !selectedChunk.CollisionVertices.Contains(pos)) return;
                selectedChunk.CollisionVertices.Remove(pos);
            }
        }

        public override void LoadContent() {
        }

        public override void Update() {
        }
        
        public override void Draw() {
            if (Editor.SelectedLayer is TileLayer2D tileLayer) {
                Vector2I tilePos = tileLayer.GetTilePosition(SelectedPos);
                Vector2I pos = GetMouseLocation() - tilePos * tileLayer.TileLength;

                if (tileLayer.GetTile(tilePos) is not null) {
                    Editor.Camera.SB.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, transformMatrix: Editor.Camera.Transform);
                    tileLayer.GetTile(tilePos).Draw(Editor.Camera, new Rectangle(tilePos * tileLayer.TileLength, new Vector2I(tileLayer.TileLength)), 1f);
                }

                if (pos.X < 0 || pos.Y < 0 || pos.X > tileLayer.TileLength || pos.Y > tileLayer.TileLength) {
                    return;
                }

                Editor.Camera.SB.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
                Editor.Camera.SB.DrawCircle((Vector2I)Editor.Camera.ToScreenPos(GetMouseLocation()), 4, Color.Tomato, true);
            } else if (Editor.SelectedLayer is TextureLayer2D textureLayer) {
                Vector2I chunkPos = textureLayer.GetChunkPosition(SelectedPos);
                Vector2I pos = GetMouseLocation() - chunkPos * textureLayer.ChunkLength;
                
                if (textureLayer.Chunks.ContainsKey(chunkPos) && textureLayer.Chunks[chunkPos] is not null) {
                    Editor.Camera.SB.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, transformMatrix: Editor.Camera.Transform);
                    Editor.Camera.SB.Draw(textureLayer.Chunks[chunkPos].Texture, new Rectangle(chunkPos * textureLayer.ChunkLength, new Vector2I(textureLayer.ChunkLength)), Color.White);
                }

                if (pos.X < 0 || pos.Y < 0 || pos.X > textureLayer.ChunkLength || pos.Y > textureLayer.ChunkLength) {
                    return;
                }

                Editor.Camera.SB.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
                Editor.Camera.SB.DrawCircle((Vector2I)Editor.Camera.ToScreenPos(GetMouseLocation()), 4, Color.Tomato, true);
            }
        }
    }
}
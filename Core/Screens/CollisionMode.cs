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
            LayerTable.BuildUI();
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
            Keybinds.Add(InputManager.RegisterKeybind(new object[] { MouseButtons.LeftButton }, new object[] { Keys.LeftShift, Keys.LeftControl, Keys.LeftAlt, Keys.F, Keys.Space }, _ => AddVertex(), TriggerOnce.False));
            Keybinds.Add(InputManager.RegisterKeybind(new object[] { MouseButtons.RightButton }, new object[] { Keys.LeftShift, Keys.LeftControl, Keys.LeftAlt, Keys.F, Keys.Space }, _ => RemoveVertex(), TriggerOnce.False));
            
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
                var tilePosPixels = tilePos * tileLayer.TileLength;
                var mousePosInTile = GetMouseLocation() - tilePosPixels;
                var tile = tileLayer.GetTile(tilePos);
                if (tile is not null) {
                    Editor.Camera.SB.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, transformMatrix: Editor.Camera.Transform);
                    tile.Draw(Editor.Camera, new Rectangle(tilePos * tileLayer.TileLength, new Vector2I(tileLayer.TileLength)), 1f);
                }

                Editor.Camera.SB.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
                if (!(mousePosInTile.X < 0 || mousePosInTile.Y < 0 || mousePosInTile.X > tileLayer.TileLength || mousePosInTile.Y > tileLayer.TileLength)) {
                    Editor.Camera.SB.DrawCircle((Vector2I)Editor.Camera.ToScreenPos(GetMouseLocation()), 5, Color.Tomato * 0.5f, true);
                }

                if (tile is null || tile.CollisionVertices is null) return;
                var vertices = new List<VertexPositionColor>();
                for (int i = 0; i < tile.CollisionVertices.Count - 1; i++) {
                    Editor.Camera.SB.DrawCircle((Vector2I)Editor.Camera.ToScreenPos(tilePosPixels + (Vector2I)tile.CollisionVertices[i]), 5, Color.Tomato, true);
                    vertices.Add(new VertexPositionColor(new Vector3(Editor.Camera.ToScreenPos(tileLayer.ToWorldPos(new Vector2(tilePosPixels.X + tile.CollisionVertices[i].X, tilePosPixels.Y + tile.CollisionVertices[i].Y))), 0), Color.Tomato));
                    vertices.Add(new VertexPositionColor(new Vector3(Editor.Camera.ToScreenPos(tileLayer.ToWorldPos(new Vector2(tilePosPixels.X + tile.CollisionVertices[i + 1].X, tilePosPixels.Y + tile.CollisionVertices[i + 1].Y))), 0), Color.Tomato));
                }
                Editor.Camera.SB.DrawCircle((Vector2I)Editor.Camera.ToScreenPos(tilePosPixels + (Vector2I)tile.CollisionVertices.Last()), 5, Color.Tomato, true);
                vertices.Add(new VertexPositionColor(new Vector3(Editor.Camera.ToScreenPos(tileLayer.ToWorldPos(new Vector2(tilePosPixels.X + tile.CollisionVertices.Last().X, tilePosPixels.Y + tile.CollisionVertices.Last().Y))), 0), Color.Tomato));
                vertices.Add(new VertexPositionColor(new Vector3(Editor.Camera.ToScreenPos(tileLayer.ToWorldPos(new Vector2(tilePosPixels.X + tile.CollisionVertices[0].X, tilePosPixels.Y + tile.CollisionVertices[0].Y))), 0), Color.Tomato));
                
                if (vertices.Count == 0) return;
                Editor.Camera.SB.End();
                var verticesArray = vertices.ToArray();

                VertexBuffer vertexBuffer = new(SQ.GD, typeof(VertexPositionColor), verticesArray.Length, BufferUsage.WriteOnly);
                vertexBuffer.SetData(verticesArray);
                SQ.GD.SetVertexBuffer(vertexBuffer);

                foreach (var pass in SQ.BasicEffect.CurrentTechnique.Passes) {
                    pass.Apply();
                    SQ.GD.DrawPrimitives(PrimitiveType.LineList, 0, verticesArray.Length / 2);
                }
            } else if (Editor.SelectedLayer is TextureLayer2D textureLayer) {
                Vector2I chunkPos = textureLayer.GetChunkPosition(SelectedPos);
                var chunkPosPixels = chunkPos * textureLayer.ChunkLength;
                var mousePosInChunk = GetMouseLocation() - chunkPosPixels;
                
                TextureChunk2D chunk = null;
                if (textureLayer.Chunks.ContainsKey(chunkPos)) chunk = textureLayer.Chunks[chunkPos];
                if (chunk is not null) {
                    Editor.Camera.SB.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, transformMatrix: Editor.Camera.Transform);
                    Editor.Camera.SB.Draw(textureLayer.Chunks[chunkPos].Texture, new Rectangle(chunkPos * textureLayer.ChunkLength, new Vector2I(textureLayer.ChunkLength)), Color.White);
                }

                Editor.Camera.SB.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
                if (!(mousePosInChunk.X < 0 || mousePosInChunk.Y < 0 || mousePosInChunk.X > textureLayer.ChunkLength || mousePosInChunk.Y > textureLayer.ChunkLength)) {
                    Editor.Camera.SB.DrawCircle((Vector2I)Editor.Camera.ToScreenPos(GetMouseLocation()), 4, Color.Tomato * 0.5f, true);
                }

                if (chunk is null || chunk.CollisionVertices is null) return;
                var vertices = new List<VertexPositionColor>();
                for (int i = 0; i < chunk.CollisionVertices.Count - 1; i++) {
                    Editor.Camera.SB.DrawCircle((Vector2I)Editor.Camera.ToScreenPos(chunkPosPixels + (Vector2I)chunk.CollisionVertices[i]), 5, Color.Tomato, true);
                    vertices.Add(new VertexPositionColor(new Vector3(Editor.Camera.ToScreenPos(textureLayer.ToWorldPos(new Vector2(chunkPosPixels.X + chunk.CollisionVertices[i].X, chunkPosPixels.Y + chunk.CollisionVertices[i].Y))), 0), Color.Tomato));
                    vertices.Add(new VertexPositionColor(new Vector3(Editor.Camera.ToScreenPos(textureLayer.ToWorldPos(new Vector2(chunkPosPixels.X + chunk.CollisionVertices[i + 1].X, chunkPosPixels.Y + chunk.CollisionVertices[i + 1].Y))), 0), Color.Tomato));
                }
                Editor.Camera.SB.DrawCircle((Vector2I)Editor.Camera.ToScreenPos(chunkPosPixels + (Vector2I)chunk.CollisionVertices.Last()), 5, Color.Tomato, true);
                vertices.Add(new VertexPositionColor(new Vector3(Editor.Camera.ToScreenPos(textureLayer.ToWorldPos(new Vector2(chunkPosPixels.X + chunk.CollisionVertices.Last().X, chunkPosPixels.Y + chunk.CollisionVertices.Last().Y))), 0), Color.Tomato));
                vertices.Add(new VertexPositionColor(new Vector3(Editor.Camera.ToScreenPos(textureLayer.ToWorldPos(new Vector2(chunkPosPixels.X + chunk.CollisionVertices[0].X, chunkPosPixels.Y + chunk.CollisionVertices[0].Y))), 0), Color.Tomato));
                
                if (vertices.Count == 0) return;
                Editor.Camera.SB.End();
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
}
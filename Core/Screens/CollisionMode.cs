namespace Somniloquy {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Microsoft.Xna.Framework.Input;
    
    public class CollisionMode : EditorMode {
        public List<(Vector2, Vector2)> CollisionEdges;
        
        public CollisionMode(Section2DScreen screen, Section2DEditor editor) : base(screen, editor) {
            Keybinds.Add(InputManager.RegisterKeybind(new object[] { MouseButtons.LeftButton }, new object[] { Keys.LeftShift, Keys.LeftControl, Keys.LeftAlt, Keys.F, Keys.Space }, _ => PaintTile(), TriggerOnce.False));
            Keybinds.Add(InputManager.RegisterKeybind(new object[] { MouseButtons.RightButton }, new object[] { Keys.LeftShift, Keys.LeftControl, Keys.LeftAlt, Keys.F, Keys.Space }, _ => EraseTile(), TriggerOnce.False));
            Keybinds.Add(InputManager.RegisterKeybind(new object[] { Keys.LeftAlt, MouseButtons.LeftButton }, new object[] { Keys.LeftShift, Keys.LeftControl, Keys.F, Keys.Space }, _ => SelectTile(), TriggerOnce.False));
            Keybinds.Add(InputManager.RegisterKeybind(new object[] { Keys.LeftControl, Keys.LeftAlt, MouseButtons.LeftButton }, new object[] { Keys.LeftShift, Keys.F, Keys.Space }, _ => EditTile(), TriggerOnce.False));
            LayerTable.BuildUI();
        }

        public override void LoadContent() {
            
        }

        public void PaintTile() {
            if (Editor.Focused && Editor.SelectedLayer is TileLayer2D tileLayer && CollisionEdges is not null && CollisionEdges.Count > 0) {
                var tilePos = tileLayer.GetTilePosition((Vector2I)Editor.Camera.GlobalMousePos.Value);
                var tile = tileLayer.GetTile(tilePos);
                if (tile is null) return;
                tile.CollisionEdges = CollisionEdges is null ? null : CollisionEdges.ToList();
            }
        }

        public void EraseTile() {
            if (Editor.Focused && Editor.SelectedLayer is TileLayer2D tileLayer) {
                var tilePos = tileLayer.GetTilePosition((Vector2I)Editor.Camera.GlobalMousePos.Value);
                var tile = tileLayer.GetTile(tilePos);
                if (tile is null) return;
                tile.CollisionEdges = null;
            }
        }

        public void SelectTile() {
            if (Editor.Focused && Editor.SelectedLayer is TileLayer2D tileLayer) {
                var tilePos = tileLayer.GetTilePosition((Vector2I)Editor.Camera.GlobalMousePos.Value);
                if (tileLayer.GetTile(tilePos) is null || tileLayer.GetTile(tilePos).CollisionEdges is null) { CollisionEdges = null; return; }
                CollisionEdges = tileLayer.GetTile(tilePos).CollisionEdges.ToList();
            }
        }

        public void EditTile() {
            if (!Editor.Focused) return;
            if (Editor.SelectedLayer is not PaintableLayer2D) return;
            Vector2I? tilePos = Editor.SelectedLayer is TileLayer2D tileLayer ? tileLayer.GetTilePosition((Vector2I)tileLayer.ToLayerPos(Editor.Camera.GlobalMousePos.Value)) : null;
            Editor.SwitchEditorMode(new CollisionEditMode(Editor.SelectedLayer, tilePos, Screen, Editor));
        }

        public override void Update() {
            
        }

        public override void Draw() {
            if (Editor.SelectedLayer is TileLayer2D tileLayer) {
                Editor.Camera.SB.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, transformMatrix: Editor.Camera.Transform);
                var tilePos = tileLayer.GetTilePosition((Vector2I)Editor.Camera.GlobalMousePos.Value);
                var tilePosPixels = tilePos * tileLayer.TileLength;
                Editor.Camera.DrawFilledRectangle(new RectangleF(tilePosPixels, new(tileLayer.TileLength)), Color.Tomato * 0.1f);
            } else if (Editor.SelectedLayer is TextureLayer2D textureLayer) {
                Editor.Camera.SB.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
                Editor.Camera.DrawFilledRectangle(new RectangleF(new Vector2(0), SQ.WindowSize), Color.Tomato * 0.1f);
            }
        }
    }
    
    public enum CollisionEditModeState { Idle, Line, Block }

    public class CollisionEditMode : EditorMode {
        public CollisionEditModeState CollisionEditModeState = CollisionEditModeState.Idle;
        public Layer2D SelectedLayer;
        public Vector2I? SelectedTilePos;
        public Vector2I? PreviousMouseLocation;


        public CollisionEditMode(Layer2D layer, Vector2I? selectedTilePos, Section2DScreen screen, Section2DEditor editor) : base(screen, editor) {
            SelectedLayer = layer;
            SelectedTilePos = selectedTilePos;

            if (layer is TileLayer2D) SelectedLayer.Opacity = 0.75f;
            Keybinds.Add(InputManager.RegisterKeybind(new object[] { MouseButtons.LeftButton }, new object[] { Keys.LeftShift, Keys.LeftControl, Keys.LeftAlt, Keys.F, Keys.Space }, _ => AddVertex(), TriggerOnce.True));
            Keybinds.Add(InputManager.RegisterKeybind(new object[] { Keys.LeftControl}, new object[] { Keys.LeftShift, Keys.LeftAlt, Keys.F, Keys.Space }, _ => AddLine(), _ => PostLine(), TriggerOnce.False));
            Keybinds.Add(InputManager.RegisterKeybind(new object[] { MouseButtons.RightButton }, new object[] { Keys.LeftShift, Keys.LeftControl, Keys.LeftAlt, Keys.F, Keys.Space }, _ => RemoveLineContainingVertex(), TriggerOnce.False));
            
            LayerTable.DestroyUI();
        }

        public override void LoadContent() { }

        private Vector2I GetMouseLocation() {
            return Util.Round(Editor.Camera.GlobalMousePos.Value);
        }

        public void Deselect() {
            Editor.SwitchEditorMode(new CollisionMode(Screen, Editor));
        }
        
        public void AddVertex() {
            if (!Editor.Focused || CollisionEditModeState != CollisionEditModeState.Idle) return;
            if (Editor.SelectedLayer is TileLayer2D tileLayer) {
                // var chain = CommandManager.AddCommandChain(new CommandChain());

                Tile2D selectedTile = tileLayer.GetTile(SelectedTilePos.Value);
                Vector2I pos = GetMouseLocation() - SelectedTilePos.Value * tileLayer.TileLength;

                if (pos.X < 0 || pos.Y < 0 || pos.X > tileLayer.TileLength || pos.Y > tileLayer.TileLength) { Deselect(); return; }

                selectedTile.CollisionEdges ??= new();
                selectedTile.CollisionEdges.Add((pos, pos));
            } else if (Editor.SelectedLayer is TextureLayer2D textureLayer) {
                var chunkPos = textureLayer.GetChunkPosition(GetMouseLocation());
                if (!textureLayer.Chunks.ContainsKey(chunkPos)) return;
                var chunk = textureLayer.Chunks[chunkPos];
                if (textureLayer.Chunks[chunkPos].CollisionEdges is null) return;

                Vector2I pos = GetMouseLocation() - chunkPos * textureLayer.ChunkLength;
                
                chunk.CollisionEdges.Add((pos, pos));
            }
        }

        public void AddLine() {
            if (PreviousMouseLocation is null) PreviousMouseLocation = GetMouseLocation();
            if (Editor.SelectedLayer is TileLayer2D layer) {
                Vector2I pos = PreviousMouseLocation.Value - SelectedTilePos.Value * layer.TileLength;
                if (pos.X < 0 || pos.Y < 0 || pos.X > layer.TileLength || pos.Y > layer.TileLength) { Deselect(); return; }
            } 

            CollisionEditModeState = CollisionEditModeState.Line;

            if (InputManager.IsMouseButtonPressed(MouseButtons.LeftButton)) {
                // var chain = CommandManager.AddCommandChain(new CommandChain());

                if (Editor.SelectedLayer is TileLayer2D tileLayer) {
                    Tile2D selectedTile = tileLayer.GetTile(SelectedTilePos.Value);
                    Vector2I previousPos = PreviousMouseLocation.Value - SelectedTilePos.Value * tileLayer.TileLength;
                    Vector2I pos = GetMouseLocation() - SelectedTilePos.Value * tileLayer.TileLength;

                    if (pos.X < 0 || pos.Y < 0 || pos.X > tileLayer.TileLength || pos.Y > tileLayer.TileLength) { Deselect(); return; }

                    // if (selectedTile is null) tileLayer.SetTile(tilePos, new Tile2D(), null);
                    selectedTile.CollisionEdges ??= new();
                    selectedTile.CollisionEdges.Add((previousPos, pos));
                } else if (Editor.SelectedLayer is TextureLayer2D textureLayer) {
                    var chunkPos = textureLayer.GetChunkPosition(GetMouseLocation());
                    if (!textureLayer.Chunks.ContainsKey(chunkPos)) return;
                    var chunk = textureLayer.Chunks[chunkPos];
                    if (textureLayer.Chunks[chunkPos].CollisionEdges is null) return;

                    Vector2I previousPos = PreviousMouseLocation.Value - chunkPos * textureLayer.ChunkLength;
                    Vector2I pos = GetMouseLocation() - chunkPos * textureLayer.ChunkLength;
                    
                    chunk.CollisionEdges.Add((previousPos, pos));
                }

                PreviousMouseLocation = GetMouseLocation();
            }
        }

        public void PostLine() {
            CollisionEditModeState = CollisionEditModeState.Idle;
            PreviousMouseLocation = null;
        }

        public void RemoveLineContainingVertex() {
            if (!Editor.Focused) return;
            if (Editor.SelectedLayer is TileLayer2D tileLayer) {
                // var chain = CommandManager.AddCommandChain(new CommandChain());

                Tile2D selectedTile = tileLayer.GetTile(SelectedTilePos.Value);
                Vector2I pos = GetMouseLocation() - SelectedTilePos.Value * tileLayer.TileLength;

                if (tileLayer.GetTilePosition(GetMouseLocation()) != SelectedTilePos) { Deselect(); return; }

                foreach (var vertices in selectedTile.CollisionEdges.ToList()) {
                    if (vertices.Item1 == pos || vertices.Item2 == pos) {
                        selectedTile.CollisionEdges.Remove(vertices);
                    }
                }
            } else if (Editor.SelectedLayer is TextureLayer2D textureLayer) {
                var chunkPos = textureLayer.GetChunkPosition(GetMouseLocation());
                if (!textureLayer.Chunks.ContainsKey(chunkPos)) return;
                var chunk = textureLayer.Chunks[chunkPos];
                if (textureLayer.Chunks[chunkPos].CollisionEdges is null) return;
                Vector2I pos = GetMouseLocation() - chunkPos * textureLayer.ChunkLength;
                
                foreach (var vertices in chunk.CollisionEdges.ToList()) {
                    if (vertices.Item1 == pos || vertices.Item2 == pos) {
                        chunk.CollisionEdges.Remove(vertices);
                    }
                }
            }
        }

        public override void Update() { }
        
        public override void Draw() {
            if (Editor.SelectedLayer is TileLayer2D tileLayer) {
                var tilePosPixels = SelectedTilePos.Value * tileLayer.TileLength;
                var mousePosInTile = GetMouseLocation() - tilePosPixels;
                var tile = tileLayer.GetTile(SelectedTilePos.Value);
                if (tile is not null) {
                    Editor.Camera.SB.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, transformMatrix: Editor.Camera.Transform);
                    tile.Draw(Editor.Camera, new Rectangle(SelectedTilePos.Value * tileLayer.TileLength, new Vector2I(tileLayer.TileLength)), 1f);
                }

                var vertices = new List<VertexPositionColor>();
                Editor.Camera.SB.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
                if (!(mousePosInTile.X < 0 || mousePosInTile.Y < 0 || mousePosInTile.X > tileLayer.TileLength || mousePosInTile.Y > tileLayer.TileLength)) {
                    if (CollisionEditModeState == CollisionEditModeState.Idle) {
                        Editor.Camera.SB.DrawCircle((Vector2I)Editor.Camera.ToScreenPos(GetMouseLocation()), 5, Color.Tomato * 0.5f, true);
                    } else if (CollisionEditModeState == CollisionEditModeState.Line) {
                        if (PreviousMouseLocation is not null) {
                            Editor.Camera.SB.DrawCircle((Vector2I)Editor.Camera.ToScreenPos(PreviousMouseLocation.Value), 5, Color.Tomato * 0.5f, true);
                            Editor.Camera.SB.DrawCircle((Vector2I)Editor.Camera.ToScreenPos(GetMouseLocation()), 5, Color.Tomato * 0.5f, true);
                            vertices.Add(new VertexPositionColor(new Vector3(Editor.Camera.ToScreenPos(PreviousMouseLocation.Value), 0), Color.Tomato * 0.5f));
                            vertices.Add(new VertexPositionColor(new Vector3(Editor.Camera.ToScreenPos(GetMouseLocation()), 0), Color.Tomato * 0.5f));
                        }
                    }
                }

                if (tile is not null && tile.CollisionEdges is not null) {
                    for (int i = 0; i < tile.CollisionEdges.Count; i++) {
                        Editor.Camera.SB.DrawCircle((Vector2I)Editor.Camera.ToScreenPos(tilePosPixels + (Vector2I)tile.CollisionEdges[i].Item1), 5, Color.Tomato, true);
                        Editor.Camera.SB.DrawCircle((Vector2I)Editor.Camera.ToScreenPos(tilePosPixels + (Vector2I)tile.CollisionEdges[i].Item2), 5, Color.Tomato, true);
                        vertices.Add(new VertexPositionColor(new Vector3(Editor.Camera.ToScreenPos(tileLayer.ToWorldPos(new Vector2(tilePosPixels.X + tile.CollisionEdges[i].Item1.X, tilePosPixels.Y + tile.CollisionEdges[i].Item1.Y))), 0), Color.Tomato));
                        vertices.Add(new VertexPositionColor(new Vector3(Editor.Camera.ToScreenPos(tileLayer.ToWorldPos(new Vector2(tilePosPixels.X + tile.CollisionEdges[i].Item2.X, tilePosPixels.Y + tile.CollisionEdges[i].Item2.Y))), 0), Color.Tomato));
                    }
                }
                
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
                Editor.Camera.SB.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
                if (CollisionEditModeState == CollisionEditModeState.Idle) {
                    Editor.Camera.SB.DrawCircle((Vector2I)Editor.Camera.ToScreenPos(GetMouseLocation()), 5, Color.Tomato * 0.5f, true);
                } else if (CollisionEditModeState == CollisionEditModeState.Line) {
                    if (PreviousMouseLocation is not null) {
                        Editor.Camera.SB.DrawCircle((Vector2I)Editor.Camera.ToScreenPos(PreviousMouseLocation.Value), 5, Color.Tomato * 0.5f, true);
                        Editor.Camera.SB.DrawCircle((Vector2I)Editor.Camera.ToScreenPos(GetMouseLocation()), 5, Color.Tomato * 0.5f, true);
                        Editor.Camera.SB.End();
                        
                        var verticesArray = new VertexPositionColor[] { 
                            new VertexPositionColor(new Vector3(Editor.Camera.ToScreenPos(PreviousMouseLocation.Value), 0), Color.Tomato * 0.5f),
                            new VertexPositionColor(new Vector3(Editor.Camera.ToScreenPos(GetMouseLocation()), 0), Color.Tomato * 0.5f)
                        };

                        VertexBuffer vertexBuffer = new(SQ.GD, typeof(VertexPositionColor), verticesArray.Length, BufferUsage.WriteOnly);
                        vertexBuffer.SetData(verticesArray);
                        SQ.GD.SetVertexBuffer(vertexBuffer);

                        foreach (var pass in SQ.BasicEffect.CurrentTechnique.Passes) {
                            pass.Apply();
                            SQ.GD.DrawPrimitives(PrimitiveType.LineList, 0, verticesArray.Length / 2);
                        }
                    }
                }

                textureLayer.DrawCollisionBounds(Editor.Camera);
            }
        }
    }
}
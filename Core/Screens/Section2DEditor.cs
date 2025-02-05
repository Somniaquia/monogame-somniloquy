namespace Somniloquy {
    using System;
    using System.Collections.Generic;

    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Microsoft.Xna.Framework.Input;

    public class Section2DEditor : BoxUI {
        public Section2DScreen Screen;
        public EditorMode EditorMode;

        public List<Keybind> GlobalKeybinds = new();
        public List<Func<string>> GlobalDebugBinds = new();
        public Layer2D SelectedLayer;

        public Section2DEditor(Section2DScreen screen) : base() {
            Screen = screen;
            Boundaries = screen.Boundaries;

            GlobalKeybinds.Add(InputManager.RegisterKeybind(Keys.F1, _ => { SwitchEditorMode(new PaintMode(Screen, this)); }, TriggerOnce.True));
            GlobalKeybinds.Add(InputManager.RegisterKeybind(Keys.F2, _ => { SwitchEditorMode(new TileMode(Screen, this)); }, TriggerOnce.True));

            GlobalKeybinds.Add(InputManager.RegisterKeybind(Keys.W, Keys.Space, _ => MoveScreen(new Vector2(0, -1)), TriggerOnce.False));
            GlobalKeybinds.Add(InputManager.RegisterKeybind(Keys.A, Keys.Space, _ => MoveScreen(new Vector2(-1, 0)), TriggerOnce.False));
			GlobalKeybinds.Add(InputManager.RegisterKeybind(Keys.S, Keys.Space, _ => MoveScreen(new Vector2(0, 1)), TriggerOnce.False));
            GlobalKeybinds.Add(InputManager.RegisterKeybind(Keys.D, Keys.Space, _ => MoveScreen(new Vector2(1, 0)), TriggerOnce.False));

            GlobalKeybinds.Add(InputManager.RegisterKeybind(Keys.Q, Keys.Space, _ => ZoomScreen(-0.05f), TriggerOnce.False));
            GlobalKeybinds.Add(InputManager.RegisterKeybind(Keys.E, Keys.Space, _ => ZoomScreen(0.05f), TriggerOnce.False));
            GlobalKeybinds.Add(InputManager.RegisterKeybind(Keys.OemPipe, _ => Screen.Camera.TargetRotation = 0, TriggerOnce.True));
            GlobalKeybinds.Add(InputManager.RegisterKeybind(Keys.OemOpenBrackets, Keys.Space, _ => RotateScreen(-0.05f), TriggerOnce.False));
            GlobalKeybinds.Add(InputManager.RegisterKeybind(Keys.OemCloseBrackets, Keys.Space, _ => RotateScreen(0.05f), TriggerOnce.False));
            
            GlobalKeybinds.Add(InputManager.RegisterKeybind(Keys.LeftAlt, _ => SelectLayerUnderMouse(), TriggerOnce.False));
            GlobalKeybinds.Add(InputManager.RegisterKeybind(new object[] { Keys.Space, Keys.Q }, _ => ScaleLayer(0.5f), TriggerOnce.Block));
            GlobalKeybinds.Add(InputManager.RegisterKeybind(new object[] { Keys.Space, Keys.E }, _ => ScaleLayer(2f), TriggerOnce.Block));
            GlobalKeybinds.Add(InputManager.RegisterKeybind(new object[] { Keys.Space, Keys.W }, _ => MoveLayer(new Vector2I(0, -1)), TriggerOnce.Block));
            GlobalKeybinds.Add(InputManager.RegisterKeybind(new object[] { Keys.Space, Keys.A }, _ => MoveLayer(new Vector2I(-1 , 0)), TriggerOnce.Block));
            GlobalKeybinds.Add(InputManager.RegisterKeybind(new object[] { Keys.Space, Keys.S }, _ => MoveLayer(new Vector2I(0, 1)), TriggerOnce.Block));
            GlobalKeybinds.Add(InputManager.RegisterKeybind(new object[] { Keys.Space, Keys.D }, _ => MoveLayer(new Vector2I(1, 0)), TriggerOnce.Block));
            GlobalKeybinds.Add(InputManager.RegisterKeybind(new object[] { Keys.Space, Keys.OemOpenBrackets }, _ => RotateLayer(-3.141592653589793238f / 8), TriggerOnce.Block));
            GlobalKeybinds.Add(InputManager.RegisterKeybind(new object[] { Keys.Space, Keys.OemCloseBrackets }, _ => RotateLayer(3.141592653589793238f / 8), TriggerOnce.Block));

            GlobalKeybinds.Add(InputManager.RegisterKeybind(new object[] { Keys.Space, Keys.H }, _ => SelectedLayer?.ToggleHide(), TriggerOnce.True));
            GlobalKeybinds.Add(InputManager.RegisterKeybind(new object[] { Keys.Space, Keys.S }, _ => { Screen.Section.Root.ToggleHide(); SelectedLayer?.Show(); }, TriggerOnce.True));

            GlobalKeybinds.Add(InputManager.RegisterKeybind(new object[] {Keys.LeftControl, Keys.Z}, new object[] {Keys.LeftShift, MouseButtons.LeftButton}, _ => CommandManager.Undo(), TriggerOnce.Block, true));
            GlobalKeybinds.Add(InputManager.RegisterKeybind(new object[] {Keys.LeftControl, Keys.LeftShift, Keys.Z}, new object[] {MouseButtons.LeftButton}, _ => CommandManager.Redo(), TriggerOnce.Block, true));

            LayerTable.Initialize();

            GlobalDebugBinds.Add(DebugInfo.Subscribe(() => $"Selected Layer: {SelectedLayer} - {SelectedLayer.Identifier}"));
            GlobalDebugBinds.Add(DebugInfo.Subscribe(() => $"Undo: {CommandManager.UndoHistory.Count} Redo: {CommandManager.RedoHistory.Count}"));
            SelectedLayer = Screen.Section.Root.Layers[0];
        }

        public override void LoadContent() {
            LayerTable.BuildUI();
            SwitchEditorMode(new PaintMode(Screen, this));
            Screen.Camera.TargetZoom = 4f;
        }

        public override void UnloadContent() {
            base.UnloadContent();
            EditorMode?.UnloadContent();
            GlobalKeybinds.ForEach(bind => InputManager.UnregisterKeybind(bind));
            GlobalDebugBinds.ForEach(bind => DebugInfo.Unsubscribe(bind));
            LayerTable.DestroyUI();
        }

        public void SwitchEditorMode(EditorMode editorMode) {
            EditorMode?.UnloadContent();
            EditorMode = editorMode;
            EditorMode.LoadContent();
        }

        public override void Update() {
            base.Update();
            EditorMode?.Update();
            // ZoomScreen(InputManager.ScrollWheelDelta * 0.001f);
        }

        public void UnregisterEditorGlobalKeybinds() {
            foreach (var keybind in GlobalKeybinds) {
                InputManager.UnregisterKeybind(keybind);
            }
        }

        public void MoveScreen(params object[] parameters) {
            if (!Focused) return;
            if (parameters.Length == 1 && parameters[0] is Vector2 direction) {
                Screen.Camera.MoveCamera(direction * 1.2f);
            }
        }

        public void ZoomScreen(params object[] parameters) {
            if (!Focused) return;
            if (parameters.Length == 1 && parameters[0] is float ratio) {
                Screen.Camera.ZoomCamera(ratio * 0.75f);
            }
        }

        public void RotateScreen(params object[] parameters) {
            if (!Focused) return;
            if (parameters.Length == 1 && parameters[0] is float ratio) {
                Screen.Camera.RotateCamera(ratio);
            }
        }

        public void MoveLayer(params object[] parameters) {
            if (!Focused) return;
            if (parameters.Length == 1 && parameters[0] is Vector2I direction && SelectedLayer is not null) {
                SelectedLayer.Displacement += direction;
                SelectedLayer.UpdateTransform();
            }
        }

        public void ScaleLayer(params object[] parameters) {
            if (!Focused) return;
            if (parameters.Length == 1 && parameters[0] is float ratio && SelectedLayer is not null) {
                SelectedLayer.Scale *= ratio;
                SelectedLayer.UpdateTransform();
            }
        }

        public void RotateLayer(params object[] parameters) {
            if (!Focused) return;
            if (parameters.Length == 1 && parameters[0] is float ratio && SelectedLayer is not null) {
                SelectedLayer.Rotation += ratio;
                SelectedLayer.UpdateTransform();
            }
        }

        public void SelectLayerUnderMouse(List<Layer2D> iter = null) {
            if (!Focused) return;

            iter ??= Screen.Section.Root.Layers;
            foreach (var layer in iter) {
                if (layer.Enabled && layer is PaintableLayer2D paintableLayer) {
                    Color? color = paintableLayer.GetColor((Vector2I)Screen.Camera.GlobalMousePos.Value);
                    if (color != null && color.Value.A != 0) {
                        SelectedLayer = layer;
                    }
                }
                if (layer.HasChildren()) {
                    SelectLayerUnderMouse(layer.Layers);
                }
            }
        }

        public void Save() { // TODO resolve ScreenManager algorithm that determines focused screen
            if (FileExplorer.Active) return;
            if (!string.IsNullOrEmpty(Screen.Section.Identifier)) {
                FileExplorer.SaveSection(Screen.Section.Identifier.Split(".")[0]);
            }  else {
                FileExplorer.BuildUI();
                FileExplorer.OpenDirectory("c:\\Somnia\\Projects\\monogame-somniloquy\\Assets");
            }
        }

        public override void Draw() {
            Screen.Section.Draw(Screen.Camera);
            EditorMode?.Draw();

            Screen.Camera.SB.End();
            
            if (SelectedLayer is TileLayer2D tileLayer) {
                DrawGrids(tileLayer.TileLength, Color.White * MathF.Min(Screen.Camera.Zoom / 16.0f, 0.25f));
                DrawGrids(tileLayer.ChunkLength * tileLayer.TileLength, Color.White * MathF.Min(Screen.Camera.Zoom / 4.0f, 0.5f));
            } else if (SelectedLayer is TextureLayer2D textureLayer) {
                DrawGrids(textureLayer.ChunkLength, Color.White * MathF.Min(Screen.Camera.Zoom / 4.0f, 0.5f));
            }
        }

        private void DrawGrids(int spacing, Color color) {
            if (color.A < 5) return;

            List<VertexPositionColor> vertices = new();
            var bounds = Screen.Camera.VisibleBounds;

            for (float y = MathF.Floor(bounds.Top / spacing) * spacing; y <= bounds.Bottom; y += spacing) {
                Vector2 start = Screen.Camera.ToScreenPos(SelectedLayer.ToWorldPos(new Vector2(bounds.Left, y)));
                Vector2 end = Screen.Camera.ToScreenPos(SelectedLayer.ToWorldPos(new Vector2(bounds.Right, y)));
                vertices.Add(new VertexPositionColor(new Vector3(start, 0), color));
                vertices.Add(new VertexPositionColor(new Vector3(end, 0), color));
            }

            for (float x = MathF.Floor(bounds.Left / spacing) * spacing; x <= bounds.Right; x += spacing) {
                Vector2 start = Screen.Camera.ToScreenPos(SelectedLayer.ToWorldPos(new Vector2(x, bounds.Top)));
                Vector2 end = Screen.Camera.ToScreenPos(SelectedLayer.ToWorldPos(new Vector2(x, bounds.Bottom)));
                vertices.Add(new VertexPositionColor(new Vector3(start, 0), color));
                vertices.Add(new VertexPositionColor(new Vector3(end, 0), color));
            }

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
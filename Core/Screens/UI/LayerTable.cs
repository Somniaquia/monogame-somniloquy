namespace Somniloquy {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Input;

    public enum Directions { Left, Right, Up, Down, Center }

    public static class LayerTable {
        public static bool Active;

        public static Section2DScreen Screen;
        public static Section2D Section;
        public static Section2DEditor Editor;

        public static BoxUI RootUI;

        private static bool createdLayer;

        public static void Initialize() {
            InputManager.RegisterKeybind(new object[] { Keys.Space, Keys.Up }, new object[] { Keys.N, Keys.Down, Keys.Left, Keys.Right }, _ => { MoveLayer(Directions.Up); }, TriggerOnce.Block );
            InputManager.RegisterKeybind(new object[] { Keys.Space, Keys.Down }, new object[] { Keys.N, Keys.Up, Keys.Left, Keys.Right }, _ => { MoveLayer(Directions.Down); }, TriggerOnce.Block );
            InputManager.RegisterKeybind(new object[] { Keys.Space, Keys.Delete }, new object[] {  }, _ => { DeleteLayer(); }, TriggerOnce.Block );

            InputManager.RegisterKeybind(new object[] { Keys.Space, Keys.N, Keys.Up }, new object[] { Keys.Down }, _ => { CreateLayer(Directions.Up, typeof(TextureLayer2D)); }, TriggerOnce.True );
            InputManager.RegisterKeybind(new object[] { Keys.Space, Keys.N, Keys.Down }, new object[] { Keys.Up }, _ => { CreateLayer(Directions.Down, typeof(TextureLayer2D)); }, TriggerOnce.True );
            InputManager.RegisterKeybind(new object[] { Keys.Space, Keys.N }, new object[] { }, null, _ => { PostCreateLayer(typeof(TextureLayer2D)); }, TriggerOnce.False );
            InputManager.RegisterKeybind(new object[] { Keys.Space, Keys.T, Keys.Up }, new object[] { Keys.Down }, _ => { CreateLayer(Directions.Up, typeof(TileLayer2D)); }, TriggerOnce.True );
            InputManager.RegisterKeybind(new object[] { Keys.Space, Keys.T, Keys.Down }, new object[] { Keys.Up }, _ => { CreateLayer(Directions.Down, typeof(TileLayer2D)); }, TriggerOnce.True );
            InputManager.RegisterKeybind(new object[] { Keys.Space, Keys.T }, new object[] { }, null, _ => { PostCreateLayer(typeof(TileLayer2D)); }, TriggerOnce.False );
        }
        
        public static void MoveLayer(Directions dir) {
            var index = Editor.SelectedLayer.Parent.Layers.FindIndex(layer => layer == Editor.SelectedLayer);
            if (dir == Directions.Down) {
                if (Editor.SelectedLayer.HasChildren()) {
                    Editor.SelectedLayer = Editor.SelectedLayer.Layers[0];
                } else if (index + 1 < Editor.SelectedLayer.Parent.Layers.Count) { 
                    Editor.SelectedLayer = Editor.SelectedLayer.Parent.Layers[index + 1]; 
                }
            } else {
                if (index > 0) {
                    Editor.SelectedLayer = Editor.SelectedLayer.Parent.Layers[index - 1];
                } else {
                    if (Editor.SelectedLayer.Parent.Parent is null) return;
                    Editor.SelectedLayer = Editor.SelectedLayer.Parent;
                }
            }
        }

        public static void CreateLayer(Directions dir, Type type) {
            Layer2D layer = null;
            if (type == typeof(TextureLayer2D)) {
                layer = new TextureLayer2D();
            } else if (type == typeof(TileLayer2D)) {
                layer = new TileLayer2D();
            }

            if (dir == Directions.Center) {
                Editor.SelectedLayer.AddLayer(layer);
            } else if (dir == Directions.Down) {
                var index = Editor.SelectedLayer.Parent.Layers.IndexOf(Editor.SelectedLayer);
                Editor.SelectedLayer.Parent.InsertLayer(index + 1, layer);
            } else if (dir == Directions.Up) {
                var index = Editor.SelectedLayer.Parent.Layers.IndexOf(Editor.SelectedLayer);
                Editor.SelectedLayer.Parent.InsertLayer(index, layer);
            }

            Editor.SelectedLayer = layer;
            BuildUI();
            createdLayer = true;
        }

        public static void PostCreateLayer(Type type) {
            if (!createdLayer) {
                CreateLayer(Directions.Center, type);
            }
            createdLayer = false;
        }

        public static void DeleteLayer() {
            var previous = Editor.SelectedLayer;
            MoveLayer(Directions.Down);
            if (previous != Editor.SelectedLayer) {
                previous.Parent.Layers.Remove(previous);
                BuildUI();
            } else {
                MoveLayer(Directions.Up);
                MoveLayer(Directions.Up);
                if (previous != Editor.SelectedLayer) {
                    previous.Parent.Layers.Remove(previous);
                    BuildUI();
                }
            }
        }

        public static void AddLayer() {
            
        }

        public static void BuildUI() {
            DestroyUI();
            RootUI = new BoxUI(Util.ShrinkRectangle(new Rectangle(0, 0, SQ.WindowSize.X, SQ.WindowSize.Y - 300), new(20))) { Identifier = "root", Renderer = null, Focusable = false, MainAxis = Axis.Horizontal, MainAxisAlign = Align.End, PerpendicularAxisAlign = Align.Begin };
            Active = true;

            Screen = ScreenManager.GetFirstOfType<Section2DScreen>();
            Section = Screen.Section;
            Editor = Screen.Editor;

            AddLayerLables();
        }

        public static void AddLayerLables(BoxUI parentLabel = null) {
            List<Layer2D> iter;
            if (parentLabel is null) {
                parentLabel = RootUI;
                iter = Section.Root.Layers;
            } else {
                iter = ((LayerLabel)parentLabel).Layer.Layers;
            }

            foreach (var innerParent in iter.Where(layer => layer.HasChildren())) {
                var innerParentLabel = new LayerLabel(parentLabel, Screen, innerParent, 5, 0) {  };
                AddLayerLables(innerParentLabel);
            }

            if (iter.Any(layer => !layer.HasChildren())) {
                var leftoverLabel = new BoxUI(parentLabel, 0, 5) { Renderer = null, Focusable = true, MainAxis = Axis.Vertical, MainAxisAlign = Align.Begin, MainAxisShrink = true };
                foreach (var layer in iter.Where(layer => !layer.HasChildren())) {
                    var childLabel = new LayerLabel(leftoverLabel, Screen, layer, 5, 10);
                }
            }
        }

        public static void DestroyUI() {
            Active = false;
            RootUI?.Destroy();
            RootUI = null;
        }
    }

    public class LayerLabel : TextLabel {
        public Section2DScreen Screen;
        public Layer2D Layer;

        public LayerLabel(BoxUI parent, Section2DScreen sectionScreen, Layer2D layer, float margin = 0, float padding = 0) : base(parent, margin, padding) {
            Screen = sectionScreen;
            Layer = layer;
            // Text = layer.Identifier;
        }

        public override void Update() {
            base.Update();

            if (Layer == Screen.Editor.SelectedLayer) {
                ((BoxUIDefaultRenderer)Renderer).Color = Color.Cyan;
                Layer.Opacity = 1f;
            } else if (Focused) {
                ((BoxUIDefaultRenderer)Renderer).Color = Color.Yellow;
                Layer.Opacity = 0.2f;

                if (InputManager.IsKeyDown(Keys.LeftAlt)) {
                    Screen.Editor.SelectedLayer = Layer;
                } else {
                    if (InputManager.IsMouseButtonPressed(MouseButtons.LeftButton)) {
                        Layer.ToggleHide();
                    }
                }
            } else if (Layer.Enabled) {
                ((BoxUIDefaultRenderer)Renderer).Color = Color.White;
                Layer.Opacity = 0.2f;
            } else {
                ((BoxUIDefaultRenderer)Renderer).Color = Color.Gray;
                Layer.Opacity = 0f;
            }
        }
    }
}
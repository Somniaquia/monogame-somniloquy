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

        public static void Initialize() {
            InputManager.RegisterKeybind(new object[] { Keys.Space, Keys.Up }, new object[] { Keys.Down, Keys.Left, Keys.Right }, (parameters) => { MoveLayer(Directions.Up); }, TriggerOnce.Block );
            InputManager.RegisterKeybind(new object[] { Keys.Space, Keys.Down }, new object[] { Keys.Up, Keys.Left, Keys.Right }, (parameters) => { MoveLayer(Directions.Down); }, TriggerOnce.Block );
            InputManager.RegisterKeybind(new object[] { Keys.Space, Keys.Delete }, new object[] {  }, (parameters) => { DeleteLayer(); }, TriggerOnce.Block );

            InputManager.RegisterKeybind(new object[] { Keys.Space, Keys.N }, new object[] { Keys.Up, Keys.Down }, (parameters) => { CreateLayer(Directions.Center, typeof(TextureLayer2D)); }, TriggerOnce.Block );
            InputManager.RegisterKeybind(new object[] { Keys.Space, Keys.Up, Keys.N }, new object[] { Keys.Up }, (parameters) => { CreateLayer(Directions.Up, typeof(TextureLayer2D)); }, TriggerOnce.Block );
            InputManager.RegisterKeybind(new object[] { Keys.Space, Keys.Down, Keys.N }, new object[] { Keys.Down }, (parameters) => { CreateLayer(Directions.Down, typeof(TextureLayer2D)); }, TriggerOnce.Block );
            InputManager.RegisterKeybind(new object[] { Keys.Space, Keys.T }, new object[] { Keys.Up, Keys.Down }, (parameters) => { CreateLayer(Directions.Center, typeof(TileLayer2D)); }, TriggerOnce.Block );
            InputManager.RegisterKeybind(new object[] { Keys.Space, Keys.Up, Keys.T }, new object[] { Keys.Up }, (parameters) => { CreateLayer(Directions.Up, typeof(TileLayer2D)); }, TriggerOnce.Block );
            InputManager.RegisterKeybind(new object[] { Keys.Space, Keys.Down, Keys.T }, new object[] { Keys.Down }, (parameters) => { CreateLayer(Directions.Down, typeof(TileLayer2D)); }, TriggerOnce.Block );
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

            BuildUI();
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

            foreach (var innerParent in iter.FindAll(layer => layer.HasChildren())) {
                var innerParentLabel = new LayerLabel(parentLabel, Screen, innerParent, 5, 5) {  };
                AddLayerLables(innerParentLabel);
            }

            if (iter.FindAll(layer => !layer.HasChildren()).Count > 0) {
                var leftoverLabel = new BoxUI(parentLabel, 5, 5) { Renderer = null, Focusable = false, MainAxis = Axis.Vertical, MainAxisAlign = Align.Begin, MainAxisShrink = true };
                foreach (var layer in iter.OfType<PaintableLayer2D>()) {
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
                // Layer.Opacity = 0.5f;
                // Layer.Draw(Layer.Section.Screen.Camera);

                if (InputManager.IsKeyDown(Keys.LeftAlt)) {
                    Screen.Editor.SelectedLayer = Layer;
                } else {
                    if (InputManager.IsMouseButtonPressed(MouseButtons.LeftButton)) {
                        Layer.Enabled = !Layer.Enabled;
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
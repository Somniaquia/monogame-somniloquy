namespace Somniloquy {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Input;

    public static class LayerTable {
        public static bool Active;

        public static Section2DScreen Screen;
        public static Section2D Section;
        public static Section2DEditor Editor;

        public static BoxUI RootUI;

        public static void Initialize() {
            InputManager.RegisterKeybind(new object[] { Keys.Space, Keys.Up }, new object[] { Keys.Down, Keys.Left, Keys.Right }, (parameters) => { MoveLayer(false); }, TriggerOnce.Block );
            InputManager.RegisterKeybind(new object[] { Keys.Space, Keys.Down }, new object[] { Keys.Up, Keys.Left, Keys.Right }, (parameters) => { MoveLayer(true); }, TriggerOnce.Block );
        }
        
        public static void MoveLayer(bool next) {
            if (Editor.SelectedLayer.Parent.Layers.Count == 1) return;
            var index = Editor.SelectedLayer.Parent.Layers.FindIndex(layer => layer == Editor.SelectedLayer);
            if (next) {
                if (index + 1 < Editor.SelectedLayer.Parent.Layers.Count) Editor.SelectedLayer = Editor.SelectedLayer.Parent.Layers[index + 1];
            } else {
                if (index > 0) Editor.SelectedLayer = Editor.SelectedLayer.Parent.Layers[index - 1];
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

        public static void AddLayerLables(BoxUI groupLabel = null) {
            List<Layer2D> iter;
            if (groupLabel is null) {
                groupLabel = RootUI;
                iter = Section.Root.Layers;
            } else {
                iter = ((LayerGroupLabel)groupLabel).Group.Layers;
            }

            foreach (var innerGroup in iter.OfType<LayerGroup2D>()) {
                var innerGroupLabel = new LayerGroupLabel(groupLabel, Screen, innerGroup, 5, 5) {  };
                // innerGroupLabel.AddChild(new TextLabel(5, 5, innerGroup.Identifier) { Renderer = null });
                AddLayerLables(innerGroupLabel);
            }

            if (iter.OfType<IPaintableLayer2D>().Count() > 0) {
                var leftoverLabel = new LayerGroupLabel(groupLabel, Screen, null, 5, 5) { Renderer = null, MainAxis = Axis.Vertical, MainAxisAlign = Align.Begin, MainAxisShrink = true };
                foreach (var layer in iter.OfType<IPaintableLayer2D>()) {
                    var childLabel = new LayerLabel(leftoverLabel, Screen, (Layer2D)layer, 5, 10);
                }
            }
        }

        public static void DestroyUI() {
            Active = false;
            RootUI?.Destroy();
            RootUI = null;
        }
    }

    public class LayerGroupLabel : BoxUI {
        public Section2DScreen Screen;
        public LayerGroup2D Group;

        public LayerGroupLabel(BoxUI parent, Section2DScreen sectionScreen, LayerGroup2D group, float margin = 0, float padding = 0) : base(parent, margin, padding) {
            Screen = sectionScreen;
            Group = group;
        }

        public override void Update() {
            base.Update();

            if (Group is null) return;
            // if (LayerGroup == Screen.Editor.Editor.SelectedLayer) {
            //     ((BoxUIDefaultRenderer)Renderer).Color = Color.Cyan;
            // } else 
            if (Focused) {
                ((BoxUIDefaultRenderer)Renderer).Color = Color.Yellow;

                if (InputManager.IsMouseButtonPressed(MouseButtons.LeftButton)) {
                    Group.Enabled = !Group.Enabled;
                    foreach (var layer in Group.Layers) {
                        layer.Enabled = Group.Enabled;
                    }
                }
            } else if (Group.Enabled) {
                ((BoxUIDefaultRenderer)Renderer).Color = Color.White;
            } else {
                ((BoxUIDefaultRenderer)Renderer).Color = Color.Gray;
            }
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
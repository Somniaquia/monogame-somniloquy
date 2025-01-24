namespace Somniloquy {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Xna.Framework;

    public static class LayerTable {
        public static bool Active;

        public static Section2DScreen Screen;
        public static Section2D Section;
        public static Section2DEditor Editor;
        public static Layer2D SelectedLayer;

        public static BoxUI RootUI;

        public static void BuildUI() {
            RootUI = new BoxUI(Util.ShrinkRectangle(new Rectangle(0, 0, SQ.WindowSize.X, SQ.WindowSize.Y), new(20))) { Identifier = "root", Focusable = false, Renderer = null, MainAxis = Axis.Horizontal, MainAxisAlign = Align.Begin };
            Active = true;

            Screen = ScreenManager.GetFirstOfType<Section2DScreen>();
            Section = Screen.Section;
            Editor = Screen.Editor;
            SelectedLayer = Editor.SelectedLayer;

            AddLayerLables();
        }

        public static void AddLayerLables(LayerGroup2D group = null, BoxUI groupLabel = null) {
            List<Layer2D> iter;
            if (group is null) {
                groupLabel = RootUI;
                iter = Section.Layers;
            } else {
                iter = group.Layers;
            }
            
            foreach (var innerGroup in iter.OfType<LayerGroup2D>()) {
                var innerGroupLabel = groupLabel.AddChild(new BoxUI(groupLabel, 5, 5) { MainAxis = Axis.Vertical });
                AddLayerLables(innerGroup, innerGroupLabel);
            }

            var leftoverLabel = groupLabel.AddChild(new BoxUI(groupLabel, 5, 5) { Renderer = null, MainAxis = Axis.Vertical });
            foreach (var layer in iter.OfType<IPaintableLayer2D>()) {
                var childLabel = groupLabel.AddChild(new BoxUI(leftoverLabel, 5, 5));
            }
        }

        public static void DestroyUI() {
            Active = false;
            RootUI?.Destroy();
            RootUI = null;
        }
    }
}
namespace Somniloquy {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Microsoft.Xna.Framework.Input;
    
    public enum Directions { Left, Right, Up, Down }

    public class LayerTable {
        public Section2DScreen SectionScreen;
        public Dictionary<int, LayerGroup2D> LayerGroups;
        public int LayerGroupIndex;
        public int LayerIndex;

        public LayerTable(Section2DScreen sectionScreen) {
            SectionScreen = sectionScreen;
            LayerGroups = sectionScreen.Section.LayerGroups;

            InputManager.RegisterKeybind(new object[] { Keys.Left }, new object[] { Keys.Space }, (parameters) => SelectLayer(Directions.Left), TriggerOnce.True, true);
            InputManager.RegisterKeybind(new object[] { Keys.Right }, new object[] { Keys.Space }, (parameters) => SelectLayer(Directions.Right), TriggerOnce.True, true);
            InputManager.RegisterKeybind(new object[] { Keys.Up }, new object[] { Keys.Space }, (parameters) => SelectLayer(Directions.Up), TriggerOnce.True, true);
            InputManager.RegisterKeybind(new object[] { Keys.Down }, new object[] { Keys.Space }, (parameters) => SelectLayer(Directions.Down), TriggerOnce.True, true);   

            InputManager.RegisterKeybind(new object[] { Keys.Space, Keys.Left }, new object[] {  }, (parameters) => AddLayer(Directions.Left), TriggerOnce.True, true);
            InputManager.RegisterKeybind(new object[] { Keys.Space, Keys.Right }, new object[] {  }, (parameters) => AddLayer(Directions.Right), TriggerOnce.True, true);
            InputManager.RegisterKeybind(new object[] { Keys.Space, Keys.Up }, new object[] {  }, (parameters) => AddLayer(Directions.Up), TriggerOnce.True, true);
            InputManager.RegisterKeybind(new object[] { Keys.Space, Keys.Down }, new object[] {  }, (parameters) => AddLayer(Directions.Down), TriggerOnce.True, true);   
        }

        public void ElevateLayers(int layerIndex) {
            foreach (var layerGroup in LayerGroups) {
                foreach (var layerPair in layerGroup.Value.Layers.OrderBy(pair => pair.Key).Reverse()) {
                    if (layerPair.Key >= layerIndex) {
                        layerGroup.Value.Layers[layerPair.Key + 1] = layerPair.Value;
                        layerGroup.Value.Layers.Remove(layerPair.Key);
                    }
                }
            }
        }

        public void ElevateLayerGroups(int layerGroupIndex) {
            foreach (var layerGroupPair in LayerGroups.OrderBy(pair => pair.Key).Reverse()) {
                if (layerGroupPair.Key >= layerGroupIndex) {
                    LayerGroups[layerGroupPair.Key + 1] = layerGroupPair.Value; 
                    LayerGroups.Remove(layerGroupPair.Key); 
                }
            }
        }
        // TODO: Rewrite garbage code
        public void AddLayer(Directions direction) {
            if (direction is Directions.Up or Directions.Down) {
                int newIndex = direction == Directions.Down ? LayerIndex + 1 : LayerIndex - 1; 
                if (LayerGroups[LayerGroupIndex].Layers.ContainsKey(newIndex) || newIndex == -1) {
                    ElevateLayers(LayerIndex);
                    newIndex = LayerIndex;
                }
                var newLayer = new TextureLayer2D();
                LayerGroups[LayerGroupIndex].Layers[newIndex] = newLayer;
                SectionScreen.Editor.SelectedLayer = newLayer;
                LayerIndex = newIndex;
            } 

            if (direction is Directions.Left or Directions.Right) {
                int newIndex = direction == Directions.Left ? LayerGroupIndex + 1 : LayerGroupIndex - 1; 
                if ((LayerGroups.ContainsKey(newIndex) && LayerGroups[newIndex].Layers.ContainsKey(LayerIndex)) || newIndex == -1) {
                    ElevateLayerGroups(LayerGroupIndex);
                    newIndex = LayerGroupIndex;
                }

                var newLayer = new TextureLayer2D();
                if (!LayerGroups.ContainsKey(newIndex)) {
                    var newLayerGroup = new LayerGroup2D();
                    LayerGroups[newIndex] = newLayerGroup;
                }
                LayerGroups[newIndex].Layers[LayerIndex] = newLayer;
                SectionScreen.Editor.SelectedLayer = newLayer;
                LayerGroupIndex = newIndex;
            }
        }

        public void SelectLayer(Directions direction) {
            if (direction is Directions.Right) {
                for (int i = LayerGroupIndex - 1; i >= 0; i--) {
                    if (LayerGroups[i].Layers.ContainsKey(LayerIndex)) {
                        LayerGroupIndex = i;
                        break;
                    }
                }
            } else if (direction is Directions.Left) {
                for (int i = LayerGroupIndex + 1; i <= LayerGroups.OrderBy(pair => pair.Key).Last().Key; i++) {
                    if (LayerGroups[i].Layers.ContainsKey(LayerIndex)) {
                        LayerGroupIndex = i;
                        break;
                    }
                }
            } else if (direction is Directions.Up) {
                for (int i = LayerIndex - 1; i >= 0; i--) {
                    if (LayerGroups[LayerGroupIndex].Layers.ContainsKey(i)) {
                        LayerIndex = i;
                        break;
                    }
                }
            } else if (direction is Directions.Down) {
                for (int i = LayerIndex + 1; i <= LayerGroups[LayerGroupIndex].Layers.OrderBy(pair => pair.Key).Last().Key; i++) {
                    if (LayerGroups[LayerGroupIndex].Layers.ContainsKey(i)) {
                        LayerIndex = i;
                        break;
                    }
                }
            }

            SectionScreen.Editor.SelectedLayer = LayerGroups[LayerGroupIndex].Layers[LayerIndex];
        }

        public void Draw() {
            for (int i = 0; i < LayerGroups.Count; i++) {
                var groupPair = LayerGroups.ElementAt(i);
                var group = groupPair.Value;

                for (int j = 0; j < group.Layers.Count; j++) {
                    var layerPair = group.Layers.ElementAt(j);
                    var layer = layerPair.Value;
                    var color = SectionScreen.Editor.SelectedLayer == layer ? Color.Aqua : Color.White;
                    SQ.SB.Draw(SQ.SB.Pixel, new Rectangle(SQ.WindowSize.X - 44 * (groupPair.Key + 1), 4 + 20 * layerPair.Key, 40, 16), color);
                }
            }
        }
    }

    public class LayerLabel : Screen {
        public LayerLabel(Rectangle boundaries) : base(boundaries) {
        }
    }
}
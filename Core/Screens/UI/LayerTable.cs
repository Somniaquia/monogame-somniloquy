namespace Somniloquy {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Input;
    
    public enum Directions { Left, Right, Up, Down }

    public class LayerTable {
        public Section2DScreen SectionScreen;
        public Dictionary<string, LayerGroup2D> LayerGroups;

        public LayerTable(Section2DScreen sectionScreen) {
            SectionScreen = sectionScreen;
            LayerGroups = sectionScreen.Section.LayerGroups;

            InputManager.RegisterKeybind(new object[] { Keys.LeftControl, Keys.N, Keys.Left }, new object[] {  }, (parameters) => AddLayer(Directions.Left), true, true);
            InputManager.RegisterKeybind(new object[] { Keys.LeftControl, Keys.N, Keys.Right }, new object[] {  }, (parameters) => AddLayer(Directions.Right), true, true);
            InputManager.RegisterKeybind(new object[] { Keys.LeftControl, Keys.N, Keys.Up }, new object[] {  }, (parameters) => AddLayer(Directions.Up), true, true);
            InputManager.RegisterKeybind(new object[] { Keys.LeftControl, Keys.N, Keys.Down }, new object[] {  }, (parameters) => AddLayer(Directions.Down), true, true);
            
            InputManager.RegisterKeybind(new object[] { Keys.LeftControl, Keys.Space, Keys.N, Keys.Left }, new object[] {  }, (parameters) => AddLayerGroup(Directions.Left), true, true);
            InputManager.RegisterKeybind(new object[] { Keys.LeftControl, Keys.Space, Keys.N, Keys.Right }, new object[] {  }, (parameters) => AddLayerGroup(Directions.Right), true, true);
            
            InputManager.RegisterKeybind(new object[] { Keys.LeftControl, Keys.N }, new object[] {  }, (parameters) => AddLayer(Directions.Left), true, true);
        }

        public void AddLayerGroup(Directions direction) {

        }

        public void AddLayer(Directions direction) {

        }

        public void Draw() {
            for (int i = 0; i < LayerGroups.Count; i++) {
                var groupPair = LayerGroups.ElementAt(i);
                var group = groupPair.Value;

                for (int j = 0; j < group.Layers.Count; j++) {
                    var layer = group.Layers[j];
                }
            }
        }
    }

    public class LayerLabel : Screen {
        public LayerLabel(Rectangle boundaries) : base(boundaries) {
        }
    }
}
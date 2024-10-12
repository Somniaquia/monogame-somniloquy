namespace Somniloquy {
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Xna.Framework;

    public partial class Section2D {
        public World World;
        public Section2DScreen Screen;
        public string Identifier;
        public Vector2 CoordsInWorldMap;
        public Dictionary<string, LayerGroup2D> LayerGroups = new();

        public static Section2D Read(string json) {
            // TODO: Serialization
            return new();
        }

        public void LoadLayerGroup() {

        }

        public void UnloadLayerGroup() {

        }

        public void AddLayerGroup(string groupName) {
            LayerGroup2D layerGroup = new(groupName);
            LayerGroups.Add(groupName, layerGroup);
        }

        public void Update() {
            foreach (var layerGroup in LayerGroups.Select(pair => pair.Value)) {
                if (!layerGroup.Loaded) continue;
                layerGroup.Update();
            }
        }
    }

    public partial class LayerGroup2D {
        public string Identifier;
        public List<Layer2D> Layers = new();
        public bool Loaded;

        public void AddLayer(Layer2D layer) {
            Layers.Add(layer);
        }

        public LayerGroup2D(string identifier) {
            Identifier = identifier;
        }

        public void Update() {
            foreach (var layer in Layers) {
                layer.Update();
            }
        }
    }
}
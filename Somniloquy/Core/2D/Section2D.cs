namespace Somniloquy {
    using System.Collections.Generic;
    using Microsoft.Xna.Framework;

    public partial class Section2D {
        public Section2DScreen DisplayedScreen;
        public string Identifier;
        public World World;
        public Vector2 CoordsInWorldMap;
        public List<LayerGroup2D> LayerGroups = new();

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
            LayerGroups.Add(layerGroup);
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
    }
}
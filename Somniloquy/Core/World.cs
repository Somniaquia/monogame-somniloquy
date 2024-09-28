namespace Somniloquy {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using MonoGame.Extended;
    using Newtonsoft.Json;

    /// <summary>
    /// A World stores multiple map layers, which are the building blocks of a world. 
    /// World includes variables and functionalities of:
    /// - world rules, which determine how the particular world behave in the game
    /// -- this includes camera panning, entity physics, TODO: Think about what would fit world rules
    /// World does not include:
    /// - tile and entity data of the world. Those are saved in layers instead.
    /// To summarize, a world is merely a container for layers that should be grouped for sharing similar themes or behaviors
    /// </summary>
    public class World {
        public string Name { get; set; }
        public SpriteSheet SpriteSheet { get; set; }
        public List<Tile> Tiles { get; set; } = new();
        public List<TileLayer2D> Layers { get; set; } = new();

        public Tile DefaultTile { get; set; }

        public World() {
            SpriteSheet = new(new Point(TileLayer2D.TileLength, TileLayer2D.TileLength));
            DefaultTile = NewTile(false);
        }

        public TileLayer2D NewLayer() {
            var layer = new TileLayer2D(this);
            Layers.Add(layer);
            return layer;
        }

        public Tile NewTile(bool createNewFrame = true) {
            Tile tile;
            if (createNewFrame) {
                tile = new Tile(SpriteSheet, SpriteSheet.NewFrame());
            } else {
                tile = new Tile(SpriteSheet, SpriteSheet.GetLatestFrame());
            }
            Tiles.Add(tile);
            return tile;
        }

        public void Update() {
            foreach (var layer in Layers) {
                layer.Update();
            }
        }

        public void RemoveUnnecessaryTiles() {
            Tiles = Tiles.Distinct().ToList();
        }
    }
}
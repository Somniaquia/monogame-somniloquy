namespace Somniloquy {
    using System;
    using System.Collections.Generic;
    using System.Text.Json.Serialization;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using MoonSharp.Interpreter;

    public enum TileEventTriggers { OnEnter, WhileStand, OnStep, OnLeave }

    public class Tile2D {
        [JsonInclude] public TileSpriteSheet SpriteSheet;
        [JsonInclude] public Sprite2D Sprite;
        [JsonInclude] public string[] ConditionValues;

        [JsonInclude] public List<(Vector2, Vector2)> CollisionEdges;
        [JsonInclude] public Dictionary<TileEventTriggers, Script> Scripts;

        public Tile2D() { }
        public Tile2D(Section2D section) {
            SpriteSheet = section.TileSpriteSheet;
            Sprite = section.TileSprite;

            var frame = new SheetSpriteFrame2D(SpriteSheet, SpriteSheet.AllocateSpace());
            ConditionValues = new[] { section.NextTileID.ToString(), "0", "0" };
            Sprite.AddFrame(ConditionValues, frame);
            section.NextTileID++;
        }

        public Color? GetColor(Vector2I position) {
            return Sprite.GetFrame(ConditionValues)?.GetColor(position);
        }

        public void PaintPixel(Vector2I position, Color color, float opacity, CommandChain chain) {
            Sprite.GetFrame(ConditionValues)?.PaintPixel(position, color, opacity, chain);
        }

        public void SetPixel(Vector2I position, Color color, CommandChain chain) {
            Sprite.GetFrame(ConditionValues)?.SetPixel(position, color, chain);
        }

        public void Update() {
            
        }

        public void Draw(Camera2D camera, Rectangle destination, float opacity = 1f) {
            Sprite.GetFrame(ConditionValues)?.Draw(camera, destination, Color.White * opacity);
        }
    }
}
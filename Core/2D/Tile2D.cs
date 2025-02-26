namespace Somniloquy {
    using System;
    using System.Collections.Generic;
    using System.Text.Json.Serialization;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using MoonSharp.Interpreter;

    public enum TileEventTriggers { OnEnter, WhileStand, OnStep, OnLeave }

    public class Tile2D {
        [JsonInclude] public SpriteFrameCollection2D SpriteFrames;
        [JsonInclude] public string[] Conditions;

        [JsonInclude] public List<(Vector2, Vector2)> CollisionEdges;
        [JsonInclude] public Dictionary<TileEventTriggers, Script> Scripts;

        public Tile2D() { }

        public Tile2D SetSprite(SpriteFrameCollection2D spriteFrames) {
            SpriteFrames = spriteFrames;
            return this;
        }

        public Color GetColor(Vector2I position) {
            return GetColor(position);
        }

        public void PaintPixel(Vector2I position, Color color, float opacity, CommandChain chain) {
            SpriteFrames.PaintPixel(position, color, opacity, chain);
        }

        public void SetPixel(Vector2I position, Color color, CommandChain chain) {
            SpriteFrames.SetPixel(position, color, chain);
        }

        public void Update() {
            SpriteFrames.Update();
        }

        public void Draw(Camera2D camera, Rectangle destination, float opacity = 1f) {
            if (SpriteFrames is null || SpriteFrames.Animations.Count == 0)
                camera.DrawFilledRectangle(destination, Color.Magenta * 0.5f);
            else {
                SpriteFrames.Draw(camera, destination, Color.White * opacity);
            }
        }
    }
}
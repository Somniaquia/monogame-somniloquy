namespace Somniloquy {
    using System;
    using System.Collections.Generic;

    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;

    public class Tile2D {
        public Sprite2D Sprite;

        public Tile2D() {
            
        }

        public Color GetColor(Vector2I position) => Sprite.GetColor(position);
        public void PaintPixel(Vector2I position, Color color, float opacity, CommandChain chain) => Sprite.PaintPixel(position, color, opacity, chain);
        public void SetPixel(Vector2I position, Color color, CommandChain chain) => Sprite.SetPixel(position, color, chain);

        public void Update() {
            
        }

        public void Draw(Rectangle destination, float opacity = 1f) {
            if (Sprite is null || Sprite.Animations.Count == 0)
                SQ.SB.DrawFilledRectangle(destination, Color.Magenta * 0.5f);
            else {
                Sprite.Draw(destination, Color.White * opacity);
            }
        }
    }
}
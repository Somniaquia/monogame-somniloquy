namespace Somniloquy {
    using System;
    using System.Collections.Generic;

    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;

    using MonoGame.Extended;

    public class Tile {
        public Sprite Sprite { get; set; }
        public Point[] CollisionVertices { get; set; } = new Point[4] { new Point(0, 0), new Point(16, 0), new Point(16, 16), new Point(0, 16) };

        public Tile(SpriteSheet spriteSheet, int frameIndexInSpriteSheet) {
            Sprite = new(spriteSheet);
            
            Sprite.AddAnimation("Default");
            Sprite.AddFrame("Default", frameIndexInSpriteSheet, Point.Zero);
        }

        public Color GetColorAt(Point positionInTile) {
            return Sprite.SpriteSheet.GetPixelColor(positionInTile + new Point(0, Sprite.CurrentAnimation.FrameIndices[Sprite.CurrentAnimationFrame] * Layer.TileLength));
        }

        public void Update() {
            
        }

        public void Draw(Rectangle destination, float opacity = 1f) {
            if (Sprite is null || Sprite.Animations.Count == 0)
                GameManager.DrawFilledRectangle(destination, Color.AliceBlue);
            else {
                Sprite.Draw(destination, opacity);
            }
        }

        public void DrawCollisionBoundaries(Rectangle destination, float opacity = 1f) {
            foreach (var vertex in CollisionVertices) {
                GameManager.SpriteBatch.DrawPoint(new Vector2(destination.X + vertex.X, destination.Y + vertex.Y), Color.DarkBlue * opacity);
            }

            for (int i = 0; i < CollisionVertices.Length; i++) {
                var vertex1 = CollisionVertices[i];
                var vertex2 = CollisionVertices[(i + 1) % CollisionVertices.Length];
                GameManager.SpriteBatch.DrawLine(
                    new Vector2(destination.X + (float)vertex1.X / Layer.TileLength * destination.Width, destination.Y + (float)vertex1.Y / Layer.TileLength * destination.Height),
                    new Vector2(destination.X + (float)vertex2.X / Layer.TileLength * destination.Width, destination.Y + (float)vertex2.Y / Layer.TileLength * destination.Height),
                    Color.DarkBlue * opacity * 0.5f
                );
            }
        }
    }
}
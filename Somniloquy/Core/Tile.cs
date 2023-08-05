namespace Somniloquy {
    using System;
    using System.Collections.Generic;

    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;

    using MonoGame.Extended;

    public class Tile {
        public Sprite Sprite { get; set; }
        public Point[] CollisionVertices { get; set; } = new Point[4] { new Point(0, 0), new Point(15, 0), new Point(15, 15), new Point(0, 15) };

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

        public void Draw(Rectangle destination, float opacity = 1f, bool drawCollisionBounds = false) {
            if (Sprite is null || Sprite.Animations.Count == 0)
                GameManager.DrawFilledRectangle(destination, Color.AliceBlue);
            else {
                Sprite.Draw(destination, opacity);
            }

            if (drawCollisionBounds) {
                foreach (var vertex in CollisionVertices) {
                    GameManager.SpriteBatch.DrawPoint(new Vector2(destination.X + vertex.X, destination.Y + vertex.Y), Color.Blue);
                }

                for (int i = 0; i < CollisionVertices.Length; i++) {
                    var vertex1 = CollisionVertices[i];
                    var vertex2 = CollisionVertices[(i + 1) % CollisionVertices.Length];
                    GameManager.SpriteBatch.DrawLine(
                        new Vector2(destination.X + vertex1.X, destination.Y + vertex1.Y),
                        new Vector2(destination.X + vertex2.X, destination.Y + vertex2.Y),
                        Color.Blue * 0.5f
                    );
                }
            }
        }
    }
}
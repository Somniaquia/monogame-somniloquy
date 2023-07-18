namespace Somniloquy {
    using System;
    using System.Collections.Generic;

    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;

    using MonoGame.Extended;

    public class Tile {
        public FunctionalSprite FSprite { get; set; } = new();
        public Point[] CollisionVertices { get; set; } = new Point[4] { new Point(0, 0), new Point(7, 0), new Point(7, 7), new Point(0, 7) };

        // For loading worlds from Ceddi-Edition
        public Tile() {
            Texture2D transparentTexture = new(GameManager.GraphicsDeviceManager.GraphicsDevice, 8, 8);
            Color[] data = new Color[8 * 8];
            Array.Fill(data, Color.Transparent);
            transparentTexture.SetData(data);

            var defaultAnimation = FSprite.AddAnimation("Default");
            defaultAnimation.AddFrame(transparentTexture);
        }

        public Tile(int tileLength = 8) {
            Texture2D transparentTexture = new(GameManager.GraphicsDeviceManager.GraphicsDevice, tileLength, tileLength);
            Color[] data = new Color[tileLength * tileLength];
            Array.Fill(data, Color.Transparent);
            transparentTexture.SetData(data);

            var defaultAnimation = FSprite.AddAnimation("Default");
            defaultAnimation.AddFrame(transparentTexture);
        }

        ~Tile() {
            FSprite.Dispose();
        }

        public Color GetColorAt(Point positionInTile) {
            return FSprite.GetCurrentAnimation().GetFrameColors(0)[positionInTile.X, positionInTile.Y].Value;
        }

        public void Update() {
            
        }

        public void Draw(Rectangle destination, float opacity = 1f, bool drawCollisionBounds = false) {
            if (FSprite is null)
                GameManager.DrawFilledRectangle(destination, Color.DarkGray);
            else {
                GameManager.SpriteBatch.Draw(
                    FSprite.GetCurrentAnimation().SpriteSheet, 
                    destination, 
                    FSprite.GetCurrentAnimation().FrameBoundaries[FSprite.FrameInCurrentAnimation], 
                    Color.White * opacity
                );
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
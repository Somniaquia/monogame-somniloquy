namespace Somniloquy {
    using System;
    using System.Collections.Generic;

    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using MonoGame.Extended;
    using Newtonsoft.Json;

    public class Tile {
        public FunctionalSprite FSprite { get; set; } = new();
        [JsonIgnore]
        public Point[] CollisionVertices { get; set; }

        // For loading worlds from Ceddi-Edition
        public Tile() {
            Texture2D transparentTexture = new(GameManager.GraphicsDeviceManager.GraphicsDevice, 8, 8);
            Color[] data = new Color[8 * 8];
            Array.Fill(data, Color.Transparent);
            transparentTexture.SetData(data);

            var defaultAnimation = FSprite.AddAnimation("Default");
            defaultAnimation.AddFrame(transparentTexture);

            CollisionVertices = new Point[4] { new Point(0, 0), new Point(8, 0), new Point(8, 8), new Point(0, 8) };
        }

        public Tile(int tileLength = 8) {
            Texture2D transparentTexture = new(GameManager.GraphicsDeviceManager.GraphicsDevice, tileLength, tileLength);
            Color[] data = new Color[tileLength * tileLength];
            Array.Fill(data, Color.Transparent);
            transparentTexture.SetData(data);

            var defaultAnimation = FSprite.AddAnimation("Default");
            defaultAnimation.AddFrame(transparentTexture);

            CollisionVertices = new Point[4] { new Point(0, 0), new Point(8, 0), new Point(8, 8), new Point(0, 8) };
        }

        ~Tile() {
            FSprite.Dispose();
        }

        public Color GetColorAt(Point positionInTile) {
            return FSprite.GetCurrentAnimation().GetFrameColors(0)[positionInTile.X, positionInTile.Y].Value;
        }

        public void Update() {
            
        }

        public void Draw(Rectangle destination, float opacity = 1f) {
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
        }
    }
}
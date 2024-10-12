namespace Somniloquy {
    using System;
    using System.Collections.Generic;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;

    public class SQTexture2D : Texture2D {
        public static List<SQTexture2D> ChangedTextures = new();

        public Color[] TextureData;

        public SQTexture2D(GraphicsDevice graphicsDevice, int width, int height) : base(graphicsDevice, width, height) {
            TextureData = new Color[width * height];
            Array.Fill(TextureData, Color.Transparent);
            SetData(TextureData);
        }

        public void SetPixel(Vector2I position, Color color, CommandChain chain = null) {
            TextureData[position.Unwrap(Width)] = color;
            ChangedTextures.Add(this);
            chain?.AddCommand(new TextureEditCommand(this, position, TextureData[position.Unwrap(Width)], color));
        }

        public void PaintPixel(Vector2I position, Color color, float opacity = 1f, CommandChain chain = null) {
            if (opacity == 1f) {
                SetPixel(position, color, chain);
            } else {
                var blendedColor = BlendColor(position, color, opacity);
                SetPixel(position, blendedColor, chain);
            }
        }

        public Color GetColor(Vector2I position) {
            return TextureData[position.Unwrap(Width)];
        }

        private Color BlendColor(Vector2I position, Color paintingColor, float opacity) {
            if (opacity == 1f) return paintingColor;

            Color canvasColor = TextureData[position.Unwrap(Width)];
            paintingColor.A = (byte)(255 * opacity);
            
            Color blendedColor = new(
                paintingColor.R * paintingColor.A + canvasColor.R * (1 - paintingColor.A),
                paintingColor.G * paintingColor.A + canvasColor.G * (1 - paintingColor.A),
                paintingColor.B * paintingColor.A + canvasColor.B * (1 - paintingColor.A),
                paintingColor.A + canvasColor.A * (1 - paintingColor.A)
            );

            return blendedColor;
        }

        public void Draw(Rectangle destination, Rectangle source, Color color, SpriteEffects effects = SpriteEffects.None) {
            SQ.SB.Draw(this, destination, source, color, effects);
        }

        public static void ApplyTextureChanges() {
            ChangedTextures.ForEach(texture => texture.SetData(texture.TextureData));
            ChangedTextures.Clear();
        }
    }
}
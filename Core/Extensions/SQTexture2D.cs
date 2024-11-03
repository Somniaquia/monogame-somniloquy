namespace Somniloquy {
    using System;
    using System.Collections.Generic;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;

    public class SQTexture2D : Texture2D, ISpriteSheet {
        public static HashSet<SQTexture2D> ChangedTextures = new();

        public Color[] TextureData;

        public SQTexture2D(GraphicsDevice graphicsDevice, int width, int height) : base(graphicsDevice, width, height) {
            TextureData = new Color[width * height];
            Array.Fill(TextureData, Color.Transparent);
            SetData(TextureData);
        }

        public void SetPixel(Vector2I position, Color color, CommandChain chain = null) {
            chain?.AddCommand(new TextureEditCommand(this, position, TextureData[position.Unwrap(Width)], color));
            TextureData[position.Unwrap(Width)] = color;
            ChangedTextures.Add(this);
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

        // Caused funny patterns - leaving it here
        // private Color BlendColor(Vector2I position, Color paintingColor, float opacity) { 
        //     if (opacity == 1f) return paintingColor;

        //     Color canvasColor = TextureData[position.Unwrap(Width)];
        //     paintingColor.A = (byte)(255 * opacity);
            
        //     Color blendedColor = new(
        //         paintingColor.R * paintingColor.A + canvasColor.R * (1 - paintingColor.A),
        //         paintingColor.G * paintingColor.A + canvasColor.G * (1 - paintingColor.A),
        //         paintingColor.B * paintingColor.A + canvasColor.B * (1 - paintingColor.A),
        //         paintingColor.A + canvasColor.A * (1 - paintingColor.A)
        //     );

        //     return blendedColor;
        // }

        private Color BlendColor(Vector2I position, Color paintingColor, float opacity) {
            if (opacity == 1f) return paintingColor;

            Color canvasColor = TextureData[position.Unwrap(Width)];
            
            Color blendedColor = new(
                (int)(paintingColor.R * opacity + canvasColor.R * (1 - opacity)),
                (int)(paintingColor.G * opacity + canvasColor.G * (1 - opacity)),
                (int)(paintingColor.B * opacity + canvasColor.B * (1 - opacity)),
                (int)(paintingColor.A * opacity + canvasColor.A * (1 - opacity))
            );

            // DebugInfo.AddTempLine(() => $"PaintingColor: {paintingColor} * Opacity: {opacity} + CanvasColor {canvasColor} => BlendedColor {blendedColor}");
            
            return blendedColor;
        }

        public void Draw(Rectangle destination, Rectangle source, Color color, SpriteEffects effects = SpriteEffects.None) {
            SQ.SB.Draw(this, destination, source, color, effects);
        }

        public static void ApplyTextureChanges() {
            foreach (var texture in ChangedTextures) {
                texture.SetData(texture.TextureData);
            }
            ChangedTextures.Clear();
        }
    }
}
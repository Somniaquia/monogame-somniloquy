namespace Somniloquy {
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;

    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using System.Linq;

    public class SpriteSheet {
        [JsonConverter(typeof(Texture2DConverter))]
        public Texture2D RawSpriteSheet { get; set; }
        public Point FrameSize { get; set; }
        public List<int> EmptyFramePositions { get; set; } = new();

        public SpriteSheet(Point frameSize) {
            FrameSize = frameSize;
            
            RawSpriteSheet = new Texture2D(GameManager.GraphicsDevice, frameSize.X, frameSize.Y);
            Color[] transparent = new Color[frameSize.X * frameSize.Y];
            Array.Fill(transparent, Color.Transparent);
            RawSpriteSheet.SetData(transparent);
        }
        
        private void ExpandTexture(int additionalHeight) {
            Texture2D expandedTexture = new(GameManager.GraphicsDevice, RawSpriteSheet.Width, RawSpriteSheet.Height + additionalHeight);

            Color[] originalColors = new Color[RawSpriteSheet.Width * RawSpriteSheet.Height];
            RawSpriteSheet.GetData(originalColors);
            expandedTexture.SetData(0, new Rectangle(0, 0, RawSpriteSheet.Width, RawSpriteSheet.Height), originalColors, 0, originalColors.Length);

            Color[] newColors = new Color[RawSpriteSheet.Width * additionalHeight];
            Array.Fill(newColors, Color.Transparent);
            expandedTexture.SetData(0, new Rectangle(0, RawSpriteSheet.Height, RawSpriteSheet.Width, additionalHeight), newColors, 0, newColors.Length);

            RawSpriteSheet.Dispose();
            RawSpriteSheet = expandedTexture;
        }
        
        public int GetLatestFrame() {
            return RawSpriteSheet.Height / Layer.TileLength - 1;
        }

        public int NewFrame() {
            ExpandTexture(Layer.TileLength);
            return RawSpriteSheet.Height / Layer.TileLength - 1;
        }

        private void ModifyTexture(Color?[,] colors, Rectangle destination) {
            Color[] colorsWithinMargin = new Color[destination.Width * destination.Height];
            RawSpriteSheet.GetData(0, destination, colorsWithinMargin, 0, colorsWithinMargin.Length);

            for (int y = 0; y < destination.Height; y++) {
                for (int x = 0; x < destination.Width; x++) {
                    if (colors[x, y] is not null) {
                        colorsWithinMargin[y * destination.Width + x] = colors[x, y].Value;
                    }
                }
            }

            RawSpriteSheet.SetData(0, destination, colorsWithinMargin, 0, colorsWithinMargin.Length);
        }


        public void PaintOnFrame(Color?[,] colors, int frameIndex) {
            ModifyTexture(colors, new Rectangle(0, frameIndex * FrameSize.Y, FrameSize.X, FrameSize.Y));
        }

        public Color?[,] GetFrameColors(int frameIndex) {
            var margin = new Rectangle(0, frameIndex * FrameSize.Y, FrameSize.X, FrameSize.Y);
            Color[] retrievedColors = new Color[margin.Width * margin.Height];
            RawSpriteSheet.GetData(0, margin, retrievedColors, 0, retrievedColors.Length);

            return Utils.ToNullableColors(Utils.ConvertTo2D(retrievedColors, FrameSize.X));
        }

        public Color GetPixelColor(Point position) {
            Color[] retrievedColor = new Color[1];
            RawSpriteSheet.GetData(0, new Rectangle(position, new Point(1, 1)), retrievedColor, 0, 1);
            return retrievedColor[0];
        }

    }
}
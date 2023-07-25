namespace Somniloquy {
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;

    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using System.Linq;

    public class SpriteSheet {
        [JsonConverter(typeof(Texture2DConverter))]
        public Texture2D Texture { get; set; }
        public Point FrameSize { get; set; }
        public int SheetWidth { get; set; }
        public List<Point> EmptyFramePositions { get; set; } = new();
        public Point PointerPosition { get; set; }

        public SpriteSheet(Point frameSize, int sheetWidth = 16) {
            FrameSize = frameSize;
            SheetWidth = sheetWidth;
            Texture = new Texture2D(GameManager.GraphicsDevice, sheetWidth, 1);
        }

        public Point AddFrame() {
            if (PointerPosition.X == SheetWidth) {
                ExpandTexture(FrameSize.Y);
                PointerPosition = new Point(0, PointerPosition.Y + 1);
            }

            return PointerPosition;
        }
        
        private void ExpandTexture(int additionalHeight) {
            Texture2D expandedTexture = new(GameManager.GraphicsDevice, Texture.Width, Texture.Height + additionalHeight);

            Color[] originalColors = new Color[Texture.Width * Texture.Height];
            Texture.GetData(originalColors);
            expandedTexture.SetData(0, new Rectangle(0, 0, Texture.Width, Texture.Height), originalColors, 0, originalColors.Length);

            Color[] newColors = new Color[Texture.Width * additionalHeight];
            for (int i = 0; i < newColors.Length; i++) {
                newColors[i] = Color.Transparent;
            }
            expandedTexture.SetData(0, new Rectangle(0, Texture.Height, Texture.Width, additionalHeight), newColors, 0, newColors.Length);

            Texture.Dispose();
            Texture = expandedTexture;
        }

        public void ModifyTexture(Color?[,] colors, Rectangle destination) {
            Color[] colorsWithinMargin = new Color[destination.Width * destination.Height];
            Texture.GetData(0, destination, colorsWithinMargin, 0, colorsWithinMargin.Length);

            for (int y = 0; y < destination.Height; y++) {
                for (int x = 0; x < destination.Width; x++) {
                    if (colors[x, y] is not null) {
                        colorsWithinMargin[y * destination.Width + x] = colors[x, y].Value;
                    }
                }
            }

            Texture.SetData(0, destination, colorsWithinMargin, 0, colorsWithinMargin.Length);
        }


        public void PaintOnFrame(Color?[,] colors, Point framePosition) {
            ModifyTexture(colors, new Rectangle(framePosition.X * FrameSize.X, framePosition.Y * FrameSize.Y, FrameSize.X, FrameSize.Y));
        }

        public Color?[,] GetFrameColors(Point framePosition) {
            var margin = new Rectangle(framePosition.X * FrameSize.X, framePosition.Y * FrameSize.Y, FrameSize.X, FrameSize.Y);
            Color[] retrievedColors = new Color[margin.Width * margin.Height];
            Texture.GetData(0, margin, retrievedColors, 0, retrievedColors.Length);

            return Utils.ToNullableColors(Utils.ConvertTo2D(retrievedColors, FrameSize.X));
        }

        public Color GetPixelColor(Point position) {
            Color[] retrievedColor = new Color[1];
            Texture.GetData(0, new Rectangle(position, new Point(1, 1)), retrievedColor, 0, 1);
            return retrievedColor[0];
        }

    }
}
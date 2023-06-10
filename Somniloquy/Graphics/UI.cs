namespace Somniloquy
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using MonoGame.Extended;

    public class UIFrame {

    }

    public class UIImage {

    }

    public class ColorChart : UIImage {
        public Texture2D Chart { get; set; }

        public static Color ColorFromHSV(float hue, float saturation, float value) {
            int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
            float f = hue / 60 - (float)Math.Floor(hue / 60);

            value = value * 255;
            int v = Convert.ToInt32(value);
            int p = Convert.ToInt32(value * (1 - saturation));
            int q = Convert.ToInt32(value * (1 - f * saturation));
            int t = Convert.ToInt32(value * (1 - (1 - f) * saturation));

            if (hi == 0)
                return new Color(v, t, p);
            else if (hi == 1)
                return new Color(q, v, p);
            else if (hi == 2)
                return new Color(p, v, t);
            else if (hi == 3)
                return new Color(p, q, v);
            else if (hi == 4)
                return new Color(t, p, v);
            else
                return new Color(v, p, q);
        }

        public void GenerateColorChart(int width=16, int height=16, int hue=0) {
            hue = Commons.Modulo(hue, 255);
            if (Chart == null)
                Chart = new Texture2D(ResourceManager.GraphicsDeviceManager.GraphicsDevice, width, height);

            Color[] chartData = new Color[width * height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float saturation = (float)x / width;
                    float value = 1f - (float)y / height;
                    Color color = ColorFromHSV(0, saturation, value);
                    chartData[y * width + x] = color;
                }
            }

            Chart.SetData(chartData);
        }

        public Color FetchColor(Point positionOnChart) {
            return Color.Beige;
        }

        public Point FetchPositionOnChart(Color color) {
            return Point.Zero;
        }

        public void Draw(Rectangle destination) {
            ResourceManager.SpriteBatch.Draw(Chart, destination, Color.White);
        }
    }
}
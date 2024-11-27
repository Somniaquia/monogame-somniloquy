namespace Somniloquy {
    using System;
    using Microsoft.Xna.Framework;

    public struct ColorOkHSL { 
        public byte H, S, L, A;
        
        public ColorOkHSL(byte h, byte s, byte l, byte a = 255) {
            H = h; S = s; L = l; A = a;
        }
    }
    
    public static class ColorExtensions {
        public static Color ToRGB(this ColorOkHSL hsl) {
            float h = hsl.H / 255f * 360f;
            float s = hsl.S / 255f;
            float l = hsl.L / 255f;

            float c = (1f - Math.Abs(2f * l - 1f)) * s;
            float x = c * (1f - Math.Abs(h / 60f % 2f - 1f));
            float m = l - c / 2f;

            float r = 0, g = 0, b = 0;

            if (h < 60) {
                r = c; g = x; b = 0;
            } else if (h < 120) {
                r = x; g = c; b = 0;
            } else if (h < 180) {
                r = 0; g = c; b = x;
            } else if (h < 240) {
                r = 0; g = x; b = c;
            } else if (h < 300) {
                r = x; g = 0; b = c;
            } else {
                r = c; g = 0; b = x;
            }

            byte red = (byte)MathF.Round((r + m) * 255);
            byte green = (byte)MathF.Round((g + m) * 255);
            byte blue = (byte)MathF.Round((b + m) * 255);

            return new Color(red, green, blue, hsl.A);
        }


        public static ColorOkHSL ToOkHSL(this Color color) {
            float r = color.R / 255f;
            float g = color.G / 255f;
            float b = color.B / 255f;

            float max = MathF.Max(r, MathF.Max(g, b));
            float min = MathF.Min(r, MathF.Min(g, b));
            float delta = max - min;

            float h = 0f;
            if (delta != 0) {
                if (max == r) {
                    h = (g - b) / delta % 6;
                } else if (max == g) {
                    h = (b - r) / delta + 2;
                } else {
                    h = (r - g) / delta + 4;
                }
                h *= 60f;
                if (h < 0) h += 360f;
            }

            float l = (max + min) / 2f;
            float s = (delta == 0) ? 0 : delta / (1f - Math.Abs(2f * l - 1f));

            byte hue = (byte)MathF.Round(h / 360f * 255);
            byte saturation = (byte)MathF.Round(s * 255);
            byte lightness = (byte)MathF.Round(l * 255);

            return new ColorOkHSL(hue, saturation, lightness, color.A);
        }
    }
}
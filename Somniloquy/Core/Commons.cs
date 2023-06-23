namespace Somniloquy {
    using System;
    using System.Collections.Generic;

    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;

    public static class Commons {
        public static float Lerp(float origin, float target, float lerpModifier) {
            return origin * (1 - lerpModifier) + target * lerpModifier;
        }
        
        public static float FloorDivideF(float dividend, float divisor) {
            return dividend >= 0 ? dividend / divisor : (dividend - divisor + 1) / divisor;
        }

        public static float ModuloF(float dividend, float divisor) {
            return (dividend%divisor + divisor) % divisor;
        }
        
        public static int FloorDivide(int dividend, int divisor) {
            return dividend >= 0 ? dividend / divisor : (dividend - divisor + 1) / divisor;
        }

        public static int Modulo(int dividend, int divisor) {
            return (dividend%divisor + divisor) % divisor;
        }

        public static bool IsWithinBoundaries(Point point, Rectangle boundaries) {
            return (boundaries.Left <= point.X && point.X <= boundaries.Right & boundaries.Top < point.Y && point.Y < boundaries.Bottom);
        }

        public static Color InvertColor(Color color) {
            return new Color(255 - color.R, 255 - color.G, 255 - color.B);
        }
    }
}
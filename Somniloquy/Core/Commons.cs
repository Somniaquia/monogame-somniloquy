namespace Somniloquy {
    using System;
    using System.Collections.Generic;

    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;

    public static class Commons {
        public static float Lerp(float origin, float target, float lerpModifier) {
            return origin * (1 - lerpModifier) + target * lerpModifier;
        }

        public static float ModuloF(float dividend, float divisor) {
            return (dividend%divisor + divisor) % divisor;
        }

        public static int Modulo(int dividend, int divisor) {
            return (dividend%divisor + divisor) % divisor;
        }
    }
}
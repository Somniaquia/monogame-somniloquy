namespace Somniloquy {
    using Microsoft.Xna.Framework;

    public static class Vector2Extensions {
        public static Vector2 PerpendicularClockwise(this Vector2 vector2) {
            return new Vector2(vector2.Y, -vector2.X);
        }

        public static Vector2 PerpendicularCounterClockwise(this Vector2 vector2) {
            return new Vector2(-vector2.Y, vector2.X);
        }

        public static int Unwrap(this Vector2I Vector2I, int width) {
            return Vector2I.Y * width + Vector2I.X;
        }

        public static Vector2I Wrap(int index, int width) {
            return new Vector2I(index % width, index / width);
        }
    }
}
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

        public static (Vector2, Vector2) Rationalize(Vector2 v1, Vector2 v2) {
            return (new Vector2(Util.Min(v1.X, v2.X), Util.Min(v1.Y, v2.Y)), new Vector2(Util.Max(v1.X, v2.X), Util.Max(v1.Y, v2.Y)));
        }
        
        public static (Vector2I, Vector2I) Rationalize(Vector2I v1, Vector2I v2) {
            return (new Vector2I(Util.Min(v1.X, v2.X), Util.Min(v1.Y, v2.Y)), new Vector2I(Util.Max(v1.X, v2.X), Util.Max(v1.Y, v2.Y)));
        }
    }
}
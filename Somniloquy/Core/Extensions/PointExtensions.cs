namespace Somniloquy {
    using Microsoft.Xna.Framework;

    public static class PointExtensions {
        public static int Unwrap(this Point point, int width) {
            return point.Y * width + point.X;
        }

        public static Point Wrap(int index, int width) {
            return new Point(index % width, index / width);
        }
    }
}
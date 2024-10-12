namespace Somniloquy {
    using Microsoft.Xna.Framework;

    public static class RectangleExtensions {
        public static Vector2I TopLeft(this Rectangle rectangle) {
            return new(rectangle.Left, rectangle.Top);
        }
        public static Vector2I TopRight(this Rectangle rectangle) {
            return new(rectangle.Right, rectangle.Top);
        }
        public static Vector2I BottomLeft(this Rectangle rectangle) {
            return new(rectangle.Left, rectangle.Bottom);
        }
        public static Vector2I BottomRight(this Rectangle rectangle) {
            return new(rectangle.Right, rectangle.Bottom);
        }

        public static Vector2 TopLeft(this RectangleF rectangle) {
            return new(rectangle.Left, rectangle.Top);
        }
        public static Vector2 TopRight(this RectangleF rectangle) {
            return new(rectangle.Right, rectangle.Top);
        }
        public static Vector2 BottomLeft(this RectangleF rectangle) {
            return new(rectangle.Left, rectangle.Bottom);
        }
        public static Vector2 BottomRight(this RectangleF rectangle) {
            return new(rectangle.Right, rectangle.Bottom);
        }
    }
}
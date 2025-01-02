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

        public static Rectangle ExpandSouthEast(this Rectangle rectangle, int amount) {
            return new(rectangle.X, rectangle.Y, rectangle.Width + amount, rectangle.Height + amount);
        }

        public static RectangleF ExpandSouthEast(this RectangleF rectangle, float amount) {
            return new(rectangle.X, rectangle.Y, rectangle.Width + amount, rectangle.Height + amount);
        }

        public static float GetAxisLength(this RectangleF rectangle, Axis axis) {
            return (axis == Axis.Horizontal) ? rectangle.Width : rectangle.Height;
        }

        public static float GetSide(this RectangleF rectangle, Axis axis, bool start) {
            if (axis == Axis.Horizontal) {
                return start ? rectangle.X : rectangle.X + rectangle.Width;
            } else {
                return start ? rectangle.Y : rectangle.Y + rectangle.Height;
            }
        }
    }
}
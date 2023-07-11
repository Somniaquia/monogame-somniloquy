namespace Somniloquy {
    using System;
    using System.Collections.Generic;

    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using MonoGame.Extended;

    public static class MathsHelper {
        public static float Lerp(float origin, float target, float lerpModifier) {
            return origin * (1 - lerpModifier) + target * lerpModifier;
        }
        
        public static Point ToPoint(Vector2 vector) {
            return new Point((int)MathF.Floor(vector.X), (int)Math.Floor(vector.Y));
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
    
        public static Rectangle ValidizeRectangle(Rectangle rectangle) {
            if (rectangle.Width < 0) {
                rectangle.X = rectangle.X + rectangle.Width;
                rectangle.Width = -rectangle.Width;
            }

            if (rectangle.Height < 0) {
                rectangle.Y = rectangle.Y + rectangle.Height;
                rectangle.Height = -rectangle.Height;
            }
            return rectangle;
        }

        public static (Point, Point) ValidizePoints(Point point1, Point point2) {
            if (point1.X > point2.X) {
                (point2.X, point1.X) = (point1.X, point2.X);
            }

            if (point1.Y > point2.Y) {
                (point2.Y, point1.Y) = (point1.Y, point2.Y);
            }

            return (point1, point2);
        }

        public static Point AnchorPoint(Point point, Point anchor) {
            if (point.X == anchor.X || point.Y == anchor.Y) return point;

            float slope = (float)(point.Y - anchor.Y) / (point.X - anchor.X);
            float distance = Vector2.Distance(point.ToVector2(), anchor.ToVector2());

            if (MathF.Abs(slope) >= 1) {
                slope = MathF.Round(slope);
            } else {
                slope = 1 / MathF.Round(1 / slope);
            }

            return new Point(point.X, (int)(anchor.Y + (point.X - anchor.X) * slope));
        }

        public static Point ToPoint(Point2 point) {
            return new Point((int)point.X, (int)point.Y);
        }

        internal static bool IntersectsOrAdjacent(Rectangle rect1, Rectangle rect2) {
            if (rect1.X < rect2.X + rect2.Width &&
                rect1.X + rect1.Width > rect2.X &&
                rect1.Y < rect2.Y + rect2.Height &&
                rect1.Y + rect1.Height > rect2.Y) {
                return true;
            }

            // Check if the rectangles are adjacent horizontally
            if ((rect1.Right == rect2.Left || rect1.Left == rect2.Right) &&
                rect1.Top < rect2.Bottom && rect1.Bottom > rect2.Top) {
                return true;
            }

            // Check if the rectangles are adjacent vertically
            if ((rect1.Bottom == rect2.Top || rect1.Top == rect2.Bottom) &&
                rect1.Left < rect2.Right && rect1.Right > rect2.Left) {
                return true;
            }

            return false;
        }
    }
}
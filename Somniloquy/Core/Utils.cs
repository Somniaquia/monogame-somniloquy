namespace Somniloquy {
    using System;
    using System.Collections.Generic;

    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using MonoGame.Extended;

    public static class Utils {
        private static Random random = new();

        public static float Lerp(float origin, float target, float lerpModifier) {
            return origin * (1 - lerpModifier) + target * lerpModifier;
        }
        
        public static Point ToPoint(Vector2 vector) {
            return new Point((int)MathF.Floor(vector.X), (int)Math.Floor(vector.Y));
        }

        public static Point Floor(Vector2 v) {
            return new Point((int)Math.Floor(v.X), (int)Math.Floor(v.Y));
        }

        public static Point Ceiling(Vector2 v) {
            return new Point((int)Math.Ceiling(v.X), (int)Math.Ceiling(v.Y));
        }

        public static float PosMod(float dividend, float divisor) {
            return (dividend%divisor + divisor) % divisor;
        }

        public static Vector2 PosMod(Vector2 va, Vector2 vb) {
            return new(PosMod(va.X, vb.X), PosMod(va.Y, vb.Y));
        }
        
        public static Point PosMod(Point va, Point vb) {
            return new(PosMod(va.X, vb.X), PosMod(va.Y, vb.Y));
        }
        
        public static int PosMod(int dividend, int divisor) {
            return (dividend%divisor + divisor) % divisor;
        }

        public static float FloorDivideF(float dividend, float divisor) {
            return dividend >= 0 ? dividend / divisor : (dividend - divisor + 1) / divisor;
        }
        
        public static int FloorDivide(int dividend, int divisor) {
            return dividend >= 0 ? dividend / divisor : (dividend - divisor + 1) / divisor;
        }


        public static bool IsWithinBoundaries(Point point, Rectangle boundaries) {
            return (boundaries.Left <= point.X && point.X <= boundaries.Right & boundaries.Top < point.Y && point.Y < boundaries.Bottom);
        }

        public static Color InvertColor(Color color) {
            return new Color(255 - color.R, 255 - color.G, 255 - color.B);
        }
    
        public static Rectangle ValidizeRectangle(Rectangle rectangle) {
            if (rectangle.Width < 0) {
                rectangle.X += rectangle.Width;
                rectangle.Width = -rectangle.Width;
            }

            if (rectangle.Height < 0) {
                rectangle.Y += rectangle.Height;
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

        /// <summary>
        /// Checks if an edge starting with V1 and ends at V2 intersects with a circel
        /// </summary>
        /// <param name="">The edge to check collisions on</param>
        /// <param name="">The circle to check collisions on</param>
        /// <returns></returns>
        public static bool Intersects((Vector2, Vector2) edge, CircleF circle) {
            Vector2 closestPointOnEdge = GetClosestPointOnLine((edge.Item1, edge.Item2), circle.Center);
            float distanceSquared = Vector2.DistanceSquared(closestPointOnEdge, circle.Center);
            float radiusSquared = circle.Radius * circle.Radius;

            return distanceSquared <= radiusSquared;
        }

        public static Vector2 GetClosestPointOnLine((Vector2, Vector2) line, Vector2 point) {
            float lineLengthSquared = (line.Item2 - line.Item1).LengthSquared();

            if (lineLengthSquared == 0f) {
                return line.Item1;
            }

            float t = MathHelper.Clamp(Vector2.Dot(point - line.Item1, line.Item2 - line.Item1) / lineLengthSquared, 0f, 1f);
            return line.Item1 + t * (line.Item2 - line.Item1);
        }

        public static float CalculateGaussian(float x, float sigma) {
            const float pi = 3.14159265358979323846f;
            return (float)(1.0 / Math.Sqrt(2 * pi * sigma * sigma) * Math.Exp(-(x * x) / (2 * sigma * sigma)));
        }

        public static float[] GetSampleWeights(int sampleCount) {
            float[] sampleWeights = new float[sampleCount];

            float totalWeights = 0.0f;
            float sigma = sampleCount / 2.0f;

            for (int i = 0; i < sampleCount; i++) {
                float x = i - sampleCount / 2;
                sampleWeights[i] = CalculateGaussian(x, sigma);
                totalWeights += sampleWeights[i];
            }

            for (int i = 0; i < sampleCount; i++) {
                sampleWeights[i] /= totalWeights;
            }
            
            return sampleWeights;
        }

        public static Vector2[] GetSampleOffsets(Direction direction, int sampleCount) {
            Vector2[] sampleOffsets = new Vector2[sampleCount];

            var length = direction == Direction.Horizontal ? SQ.WindowSize.X : SQ.WindowSize.Y;
            float delta = 1.0f / length;

            for (int i = 0; i < sampleCount; i++) {
                sampleOffsets[i] = direction == Direction.Horizontal ? new Vector2((i - sampleCount / 2) * delta, 0) : new Vector2(0, (i - sampleCount / 2) * delta);
            }

            return sampleOffsets;
        }

        public static T[,] ConvertTo2D<T>(T[] oneDimensionalArray, int newWidth) {
            int length = oneDimensionalArray.Length;
            int newHeight = (length + newWidth - 1) / newWidth;

            T[,] result = new T[newWidth, newHeight];

            for (int i = 0; i < length; i++) {
                int x = i % newWidth;
                int y = i / newWidth;

                result[x, y] = oneDimensionalArray[i];
            }

            return result;
        }


        public static Color?[,] ToNullableColors(Color[,] colors) {
            int width = colors.GetLength(0);
            int height = colors.GetLength(1);

            Color?[,] result = new Color?[width, height];

            for (int i = 0; i < width; i++) {
                for (int j = 0; j < height; j++) {
                    result[i, j] = colors[i, j];
                }
            }

            return result;
        }

        public static Color[,] FromNullableColors(Color?[,] nullableColors) {
            int width = nullableColors.GetLength(0);
            int height = nullableColors.GetLength(1);

            Color[,] result = new Color[width, height];

            for (int i = 0; i < width; i++) {
                for (int j = 0; j < height; j++) {
                    if (nullableColors[i, j].HasValue) {
                        result[i, j] = nullableColors[i, j].Value;
                    }
                    else {
                        result[i, j] = Color.Transparent;
                    }
                }
            }

            return result;
        }

        public static Rectangle ResizeRectangle(Rectangle target, Direction direction, float ratio, float offset) {
            if (direction == Direction.Horizontal) {
                return new Rectangle(target.X + (int)(target.Width * offset), target.Y, (int) (target.Width * ratio), target.Height);
            } else {
                return new Rectangle(target.X, target.Y + (int)(target.Height * offset), target.Width, (int)(target.Height * ratio));
            }  
        }

        public static void TransparentizeTexture(Texture2D target, Color? targetColor) {
            Color[] colorData = new Color[target.Width * target.Height];
            target.GetData(colorData);

            for (int i = 0; i < colorData.Length; i++) {
                if (colorData[i] == targetColor) {
                    colorData[i] = Color.Transparent;
                }
            }

            target.SetData(colorData);
        }

        public static int RandomInteger(int min, int nonInclusiveMax) {
            return random.Next(min, nonInclusiveMax);
        }

        public static T GetNextEnumValue<T>(T currentValue) where T : Enum {
            T[] values = (T[])Enum.GetValues(typeof(T));
            int j = Array.IndexOf(values, currentValue) + 1;
            return (j == values.Length) ? values[0] : values[j];
        }
    }
}
namespace Somniloquy {
    using System;
    using Microsoft.Xna.Framework;

    public delegate void PixelAction(Vector2I Vector2I);

    public class PixelActions {
        public static void ApplyRectangleAction(Vector2I start, Vector2I end, bool filled, PixelAction action) {
            for (int y = start.Y; y < end.Y; y++) {
                for (int x = start.X; x < end.X; x++) {
                    action(new Vector2I(x, y));
                }
            }
        }
 
        public static void ApplyLineAction(Vector2I start, Vector2I end, int width, PixelAction action) {
            int x0 = start.X;
            int y0 = start.Y;
            int x1 = end.X;
            int y1 = end.Y;

            int dx = Math.Abs(x1 - x0);
            int dy = Math.Abs(y1 - y0);

            int sx = x0 < x1 ? 1 : -1;
            int sy = y0 < y1 ? 1 : -1;

            int err = dx - dy;

            while (true) {
                ApplyCircleAction(new Vector2I(x0, y0), width, true, action);

                if (x0 == x1 && y0 == y1) break;

                int e2 = 2 * err;
                if (e2 > -dy) {
                    err -= dy;
                    x0 += sx;
                }
                if (e2 < dx) {
                    err += dx;
                    y0 += sy;
                }
            }
        }

        public static void ApplyCircleAction(Vector2I center, int radius, bool filled, PixelAction action) {
            if (radius == 0) {
                action(center);
            } else if (filled) {
                int x = 0;
                int y = radius;
                int d = 3 - 2 * radius;

                while (y >= x) {
                    ApplyHorizontalLineAction(center.X - x, center.X + x, center.Y + y, action);
                    ApplyHorizontalLineAction(center.X - x, center.X + x, center.Y - y, action);
                    ApplyHorizontalLineAction(center.X - y, center.X + y, center.Y + x, action);
                    ApplyHorizontalLineAction(center.X - y, center.X + y, center.Y - x, action);

                    x++;

                    if (d > 0) {
                        y--;
                        d = d + 4 * (x - y) + 10;
                    } else {
                        d = d + 4 * x + 6;
                    }
                }
            } else {
                int x = 0;
                int y = radius;
                int d = 3 - 2 * radius;

                while (y >= x) {
                    action(new Vector2I(center.X + x, center.Y + y));
                    action(new Vector2I(center.X - x, center.Y + y));
                    action(new Vector2I(center.X + x, center.Y - y));
                    action(new Vector2I(center.X - x, center.Y - y));
                    action(new Vector2I(center.X + y, center.Y + x));
                    action(new Vector2I(center.X - y, center.Y + x));
                    action(new Vector2I(center.X + y, center.Y - x));
                    action(new Vector2I(center.X - y, center.Y - x));

                    x++;

                    if (d > 0) {
                        y--;
                        d = d + 4 * (x - y) + 10;
                    } else {
                        d = d + 4 * x + 6;
                    }
                }
            }
        }

        private static void ApplyHorizontalLineAction(int xStart, int xEnd, int y, PixelAction action) {
            for (int x = xStart; x <= xEnd; x++) {
                action(new Vector2I(x, y));
            }
        }
    }
}
namespace Somniloquy {
    using System;
    using Microsoft.Xna.Framework;

    public delegate void Action(params object[] parameters);

    public class Actions {
        public static void LineAction(Point start, Point end, Color color, float opacity, Action action, int width) {
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
                FilledCircleAction(new Point(x0, y0), width, color, opacity, action);

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

        public static void CircleAction(Point center, int radius, Color color, float opacity, Action action) {
            int x = 0;
            int y = radius;
            int d = 3 - 2 * radius;

            while (y >= x) {
                action(new Point(center.X + x, center.Y + y), color, opacity);
                action(new Point(center.X - x, center.Y + y), color, opacity);
                action(new Point(center.X + x, center.Y - y), color, opacity);
                action(new Point(center.X - x, center.Y - y), color, opacity);
                action(new Point(center.X + y, center.Y + x), color, opacity);
                action(new Point(center.X - y, center.Y + x), color, opacity);
                action(new Point(center.X + y, center.Y - x), color, opacity);
                action(new Point(center.X - y, center.Y - x), color, opacity);

                x++;

                if (d > 0) {
                    y--;
                    d = d + 4 * (x - y) + 10;
                }
                else {
                    d = d + 4 * x + 6;
                }
            }
        }

        public static void FilledCircleAction(Point center, int radius, Color color, float opacity, Action action) {
            int x = 0;
            int y = radius;
            int d = 3 - 2 * radius;

            while (y >= x) {
                HorizontalLineAction(center.X - x, center.X + x, center.Y + y, color, opacity, action);
                HorizontalLineAction(center.X - x, center.X + x, center.Y - y, color, opacity, action);
                HorizontalLineAction(center.X - y, center.X + y, center.Y + x, color, opacity, action);
                HorizontalLineAction(center.X - y, center.X + y, center.Y - x, color, opacity, action);

                x++;

                if (d > 0) {
                    y--;
                    d = d + 4 * (x - y) + 10;
                } else {
                    d = d + 4 * x + 6;
                }
            }
        }

        private static void HorizontalLineAction(int xStart, int xEnd, int y, Color color, float opacity, Action action) {
            for (int x = xStart; x <= xEnd; x++) {
                action(new Point(x, y), color, opacity);
            }
        }
    }
}
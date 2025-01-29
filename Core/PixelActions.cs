namespace Somniloquy {
    using System;
    using Microsoft.Xna.Framework;

    public delegate void PixelAction(Vector2I Vector2I);

    public class PixelActions {
        public static void ApplyRectangleAction(Vector2I start, Vector2I end, bool filled, PixelAction action) {
            (start, end) = Vector2Extensions.Rationalize(start, end);
            if (filled) {
                for (int y = start.Y; y <= end.Y; y++) {
                    for (int x = start.X; x <= end.X; x++) {
                        action(new Vector2I(x, y));
                    }
                }
            } else {
                ApplyLineAction(start, new(start.X, end.Y), 0, action);
                ApplyLineAction(start, new(end.X, start.Y), 0, action);
                ApplyLineAction(end, new(start.X, end.Y), 0, action);
                ApplyLineAction(end, new(end.X, start.Y), 0, action);
            }
        }

        public static Vector2I ApplySnappedLineAction(Vector2I start, Vector2I end, int radius, PixelAction action) {
            int dx = end.X - start.X;
            int dy = end.Y - start.Y;

            if (dx == 0 || dy == 0) {
                ApplyLineAction(start, end, radius, action);
                return end;
            }

            float m = dy / (float)dx;
            var (interval, reciprocal) = GetSnappedSlope(m);
            
            float d = (start - end).Length();
            
            int ux = dx / Math.Abs(dx);
            int uy = dy / Math.Abs(dy);
            int steps = 0; dx = 0; dy = 0;
            while (new Vector2(dx, dy).Length() <= d) {
                action(start + new Vector2I(dx, dy));
                steps++;
                if (steps == interval) {
                    steps = 0;
                    if (reciprocal) dy += uy;
                    else dx += ux;
                }
                if (reciprocal) dx += ux;
                else dy += uy;
            }

            if (steps == 0) {
                if (reciprocal) dy -= uy;
                else dx -= ux;
            }
            if (reciprocal) dx -= ux;
            else dy -= uy;
            return start + new Vector2I(dx, dy);
        }

        private static (int, bool) GetSnappedSlope(float m) {
            int candidateInt = (int)MathF.Round(m);
            
            float reciprocal = 1f/m;
            int reciprocalRounded = (int)MathF.Round(reciprocal);
            float candidateRecip = reciprocalRounded != 0 ? 1f/reciprocalRounded : 0;
            
            float diffInt = MathF.Abs(MathF.Atan(m) - MathF.Atan(candidateInt));
            float diffRecip = MathF.Abs(MathF.Atan(m) - MathF.Atan(candidateRecip));
            int choice = diffRecip < diffInt ? reciprocalRounded : candidateInt;
            return (Math.Abs(choice), diffRecip < diffInt);
        }
 
        public static void ApplyLineAction(Vector2I start, Vector2I end, int radius, PixelAction action) {
            var (x0, y0, x1, y1) = (start.X, start.Y, end.X, end.Y);
            var (dx, dy) = (Math.Abs(x1 - x0), Math.Abs(y1 - y0));
            int sx = x0 < x1 ? 1 : -1, sy = y0 < y1 ? 1 : -1;
            int err = dx - dy;

            if (radius > 0) {
                ApplyCircleAction(new Vector2I(x0, y0), radius, true, action);
                ApplyCircleAction(new Vector2I(x1, y1), radius, true, action);
    
                Vector2 d = Vector2.Normalize(end - start).PerpendicularClockwise() * radius;

                Vector2I previousOffsettedStart = (Vector2I)(start - d);
                Vector2I previousOffsettedEnd = (Vector2I)(end - d);

                for (int i = - radius; i <= radius; i++) {
                    Vector2I offsettedStart = (Vector2I)(start + (float)i / radius * d);
                    Vector2I offsettedEnd = (Vector2I)(end + (float)i / radius * d);
                    if ((previousOffsettedStart - offsettedStart).Length() > 1.4) {
                        ApplyLineAction(new Vector2I(previousOffsettedStart.X, offsettedStart.Y), new Vector2I(previousOffsettedEnd.X, offsettedEnd.Y), 0, action);
                    }
                    previousOffsettedStart = offsettedStart;
                    previousOffsettedEnd = offsettedEnd;

                    ApplyLineAction(offsettedStart, offsettedEnd, 0, action);
                }
            } else {
                while (true) {
                    action(new Vector2I(x0, y0));
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
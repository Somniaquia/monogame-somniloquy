namespace Somniloquy {
    using System;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;

    public class SQSpriteBatch : SpriteBatch {
        public SQSpriteBatch(GraphicsDevice graphicsDevice) : base(graphicsDevice) { }

        public Texture2D Pixel;

        public void DrawFilledRectangle(Rectangle destination, Color color) {
            Draw(Pixel, destination, null, color, 0f, Vector2.Zero, SpriteEffects.None, 0f);
        }

        public void DrawPixelizedLine(Point position1, Point position2, Color color) {
            int dx = Math.Abs(position2.X - position1.X);
            int dy = Math.Abs(position2.Y - position1.Y);
            int sx = (position1.X < position2.X) ? 1 : -1;
            int sy = (position1.Y < position2.Y) ? 1 : -1;
            int err = dx - dy;

            while (true) {
                Draw(Pixel, new Rectangle(position1.X, position1.Y, 1, 1), color);

                if (position1.X == position2.X && position1.Y == position2.Y)
                    break;

                int err2 = 2 * err;

                if (err2 > -dy) {
                    err -= dy;
                    position1.X += sx;
                }

                if (err2 < dx) {
                    err += dx;
                    position1.Y += sy;
                }
            }
        }
    }
}
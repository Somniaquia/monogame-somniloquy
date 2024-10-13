namespace Somniloquy {
    using System;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;

    public class SQSpriteBatch : SpriteBatch {
        public SQSpriteBatch(GraphicsDevice graphicsDevice) : base(graphicsDevice) { }

        public Texture2D Pixel;

        public void Draw(Texture2D texture, Rectangle destination, Rectangle source, Color color, SpriteEffects effects) {
            Draw(texture, destination, source, color, 0f, Vector2.Zero, effects, 0f);
        }

        public void DrawFilledRectangle(Rectangle destination, Color color) {
            Draw(Pixel, destination, null, color, 0f, Vector2.Zero, SpriteEffects.None, 0f);
        }

        public void DrawLine(Vector2I start, Vector2I end, Color color, int width = 0) {
            PixelActions.ApplyLineAction(start, end, width, (Vector2I position) => {
                DrawPoint(position, color);
            });
        }

        public void DrawCircle(Vector2I center, int radius, Color color, bool filled) {
            PixelActions.ApplyCircleAction(center, radius, filled, (Vector2I position) => {
                DrawPoint(position, color);
            });
        }

        public void DrawPoint(Vector2I position, Color color) {
            Draw(Pixel, position, color);
        }
    }
}
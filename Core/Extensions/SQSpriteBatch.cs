namespace Somniloquy {
    using System;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;

    public class SQSpriteBatch : SpriteBatch {
        public Texture2D Pixel;
        public bool Active;

        public SQSpriteBatch(GraphicsDevice graphicsDevice) : base(graphicsDevice) {
            Pixel = new(SQ.GD, 1, 1);
            Pixel.SetData(new[] { Color.White });
        }

        public void Draw(Texture2D texture, Rectangle destination, Rectangle source, Color color, SpriteEffects effects) {
            Draw(texture, destination, source, color, 0f, Vector2.Zero, effects, 0f);
        }

        public void DrawFilledRectangle(RectangleF destination, Color color) {
            Draw(Pixel, (Rectangle)destination, null, color, 0f, Vector2.Zero, SpriteEffects.None, 0f);
        }

        public void DrawRectangle(RectangleF destination, Color color, int width = 0) {
            DrawLine((Vector2I)destination.TopLeft(), (Vector2I)destination.TopRight(), color, width);
            DrawLine((Vector2I)destination.TopRight(), (Vector2I)destination.BottomRight(), color, width);
            DrawLine((Vector2I)destination.BottomRight(), (Vector2I)destination.BottomLeft(), color, width);
            DrawLine((Vector2I)destination.BottomLeft(), (Vector2I)destination.TopLeft(), color, width);
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

        // public void Begin(SpriteSortMode spriteSortMode, BlendState blendState, SamplerState samplerState) {
        //     if (!Active) {
        //         base.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
        //         Active = true;
        //     }
        // }

        // public new void End() {
        //     if (Active) {
        //         base.End();
        //         Active = false;
        //     }
        // }
    }
}
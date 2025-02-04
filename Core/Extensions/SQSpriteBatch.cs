namespace Somniloquy {
    using System;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;

    public class SQSpriteBatch : SpriteBatch {
        public Texture2D Pixel;

        public bool Active;
        public SpriteSortMode SortMode;
        public BlendState BlendState;
        public SamplerState SamplerState;
        public DepthStencilState DepthStencilState;
        public RasterizerState RasterizerState;
        public Effect Effect;
        public Matrix? TransformMatrix;

        public SQSpriteBatch(GraphicsDevice graphicsDevice) : base(graphicsDevice) {
            Pixel = new(SQ.GD, 1, 1);
            Pixel.SetData(new[] { Color.White });
        }

        public new void Begin(SpriteSortMode spriteSortMode, BlendState blendState = null, SamplerState samplerState = null, DepthStencilState depthStencilState = null, RasterizerState rasterizerState = null, Effect effect = null, Matrix? transformMatrix = null) {
            blendState ??= BlendState.AlphaBlend;
            samplerState ??= SamplerState.LinearClamp;
            depthStencilState ??= DepthStencilState.None;
            rasterizerState ??= RasterizerState.CullCounterClockwise;

            if (!Active || spriteSortMode != SortMode || blendState != BlendState || samplerState != SamplerState || depthStencilState != DepthStencilState || rasterizerState != RasterizerState || Effect != effect || transformMatrix != TransformMatrix)  {
                End();

                base.Begin(spriteSortMode, blendState, samplerState, depthStencilState, rasterizerState, effect, transformMatrix);

                SortMode = spriteSortMode;
                BlendState = blendState;
                SamplerState = samplerState;
                DepthStencilState = depthStencilState;
                RasterizerState = rasterizerState;
                TransformMatrix = transformMatrix;
                Active = true;
            }
        }

        public new void End() {
            if (Active) {
                base.End();
                Active = false;
            }
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
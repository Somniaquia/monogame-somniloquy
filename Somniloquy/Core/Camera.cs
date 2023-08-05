namespace Somniloquy {
    using System;
    using System.Collections.Generic;

    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using MonoGame.Extended;

    public class Camera {
        public const float LerpModifier =  0.1f;
        public Vector2 Position = Vector2.Zero;
        private Vector2 visiblePosition = Vector2.Zero;
        public float zoom;
        private float visibleZoom;
        public float Rotation = 0.0f;
        private float visibleRotation = 0.0f;
        public Matrix Transform { get; private set; }

        public Camera(float zoom) {
            this.zoom = zoom;
            visibleZoom = zoom;
        }

        public void Move(Vector2 displacement) {
            Position += displacement * 10 / MathF.Sqrt(zoom);
        }

        public void Zoom(float delta) {
            zoom *= MathF.Pow(MathF.E, delta);
            zoom = MathF.Min(MathF.Max(zoom, 1f), 64f);
        }

        public void UpdateTransformation() {
            visiblePosition.X = Utils.Lerp(visiblePosition.X, Position.X, LerpModifier);
            visiblePosition.Y = Utils.Lerp(visiblePosition.Y, Position.Y, LerpModifier);

            visibleZoom = Utils.Lerp(visibleZoom, zoom, LerpModifier);

            Rotation = Utils.ModuloF(Rotation, 2 * 3.141592653589793f);
            visibleRotation = Utils.Lerp(visibleRotation, Rotation, LerpModifier);

            Viewport viewport = GameManager.GraphicsDevice.Viewport;

            Transform =
                Matrix.CreateTranslation(new Vector3(-visiblePosition.X, -visiblePosition.Y, 0)) *
                Matrix.CreateRotationZ(visibleRotation) *
                Matrix.CreateScale(new Vector3(visibleZoom, visibleZoom, 1)) *
                Matrix.CreateTranslation(new Vector3(viewport.Width * 0.5f, viewport.Height * 0.5f, 0)
            );
        }

        public Vector2 ApplyTransform(Vector2 originalVector) {
            return Vector2.Transform(originalVector, Transform);
        }

        public Vector2 ApplyInvertTransform(Vector2 transformedVector) {
            return Vector2.Transform(transformedVector, Matrix.Invert(Transform));
        }

        public Rectangle ApplyTransform(Rectangle originalRectangle) {
            return new Rectangle(
                Vector2.Transform(originalRectangle.Location.ToVector2(), Transform).ToPoint(),
                Vector2.Transform((originalRectangle.Location + originalRectangle.Size).ToVector2(), Transform).ToPoint() - Vector2.Transform(originalRectangle.Location.ToVector2(), Transform).ToPoint()
            );
        }

        public (Vector2, Vector2) GetCameraBounds() {
            Viewport viewport = GameManager.GraphicsDevice.Viewport;
            Vector2 upperLeft = ApplyInvertTransform(Vector2.Zero);
            //upperLeft.X += viewport.Width * 0.5f;
            //upperLeft.Y += viewport.Height * 0.5f;

            Vector2 bottomRight = ApplyInvertTransform(new Vector2(viewport.Width, viewport.Height));
            bottomRight.X += viewport.Width * 0.5f;
            bottomRight.Y += viewport.Height * 0.5f;
            return (upperLeft, bottomRight);
        }
    }
}
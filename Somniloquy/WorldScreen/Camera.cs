namespace Somniloquy {
    using System;
    using System.Collections.Generic;

    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;

    public class Camera
    {
        public const float LerpModifier =  0.1f;
        public Vector2 Position = Vector2.Zero;
        private Vector2 visiblePosition = Vector2.Zero;
        public float Zoom;
        private float visibleZoom;
        public float Rotation = 0.0f;
        private float visibleRotation = 0.0f;
        public Matrix Transform { get; private set; }

        public Camera(float zoom=1.0f) {
            Zoom = zoom;
            visibleZoom = zoom;
        }

        public void Move(Vector2 displacement) {
            Position += displacement * 10 / MathF.Sqrt(Zoom);
        }

        public void UpdateTransformation() {
            visiblePosition.X = MathsHelper.Lerp(visiblePosition.X, Position.X, LerpModifier);
            visiblePosition.Y = MathsHelper.Lerp(visiblePosition.Y, Position.Y, LerpModifier);

            Zoom = Zoom < 0.1f ? 0.1f : Zoom;
            visibleZoom = MathsHelper.Lerp(visibleZoom, Zoom, LerpModifier);

            Rotation = MathsHelper.ModuloF(Rotation, 2 * 3.141592653589793f);
            visibleRotation = MathsHelper.Lerp(visibleRotation, Rotation, LerpModifier);

            Viewport viewport = GameManager.GraphicsDeviceManager.GraphicsDevice.Viewport;

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

        public (Vector2, Vector2) GetCameraBounds() {
            Viewport viewport = GameManager.GraphicsDeviceManager.GraphicsDevice.Viewport;
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
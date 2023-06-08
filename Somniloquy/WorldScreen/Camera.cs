namespace Somniloquy {
    using System;
    using System.Collections.Generic;

    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;

    public class Camera
    {
        public float LerpModifier { get; set; }
        public Vector2 Position { get; set; } = Vector2.Zero;
        private Vector2 visiblePosition = Vector2.Zero;
        public float Zoom { get; set; } = 1.0f;
        private float visibleZoom = 1.0f;
        public float Rotation { get; set; } = 0.0f;
        private float visibleRotation = 0.0f;
        public Matrix Transform { get; private set; }

        private float Lerp(float origin, float target) {
            return origin * (1 - LerpModifier) + target * LerpModifier;
        }

        private float Modulo(float dividend, float divisor) {
            return (dividend%divisor + divisor) % divisor;
        }

        public void UpdateTransformation() {
            visiblePosition.X = Lerp(visiblePosition.X, Position.X);
            visiblePosition.Y = Lerp(visiblePosition.Y, Position.Y);

            Zoom = Zoom < 0.1f ? 0.1f : Zoom;
            visibleZoom = Lerp(visibleZoom, Zoom);

            Rotation = Modulo(Rotation, 2 * 3.141592653589793f);
            visibleRotation = Lerp(visibleRotation, Rotation);

            Viewport viewport = ResourceManager.GraphicsDeviceManager.GraphicsDevice.Viewport;
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
    }
}
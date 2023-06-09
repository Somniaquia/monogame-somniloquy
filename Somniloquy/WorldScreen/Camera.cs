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
        public float Zoom = 1.0f;
        private float visibleZoom = 1.0f;
        public float Rotation = 0.0f;
        private float visibleRotation = 0.0f;
        public Matrix Transform { get; private set; }

        public void Move(Vector2 displacement) {
            Position += displacement / Zoom * 4;
        }

        public void UpdateTransformation() {
            visiblePosition.X = Commons.Lerp(visiblePosition.X, Position.X, LerpModifier);
            visiblePosition.Y = Commons.Lerp(visiblePosition.Y, Position.Y, LerpModifier);

            Zoom = Zoom < 0.1f ? 0.1f : Zoom;
            visibleZoom = Commons.Lerp(visibleZoom, Zoom, LerpModifier);

            Rotation = Commons.ModuloF(Rotation, 2 * 3.141592653589793f);
            visibleRotation = Commons.Lerp(visibleRotation, Rotation, LerpModifier);

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
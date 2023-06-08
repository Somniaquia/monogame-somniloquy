namespace Somniloquy {
    using System;
    using System.Collections.Generic;

    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;

    public class Camera
    {
        public Vector2 Position { get; private set; } = Vector2.Zero;
        public float Zoom { get; private set; } = 1.0f;
        public float Rotation { get; private set; } = 0.0f;
        public Matrix Transform { get; private set; }

        public void Move(Vector2 displacement) { Position += displacement; }
        public void ChangeZoom(float increment) { Zoom += increment; }
        public void Rotate(float increment) { Rotation += increment; }

        public Matrix GetTransformation()
        {
            Viewport viewport = ResourceManager.GraphicsDeviceManager.GraphicsDevice.Viewport;
            Transform =
                Matrix.CreateTranslation(new Vector3(-Position.X, -Position.Y, 0)) *
                Matrix.CreateRotationZ(Rotation) *
                Matrix.CreateScale(new Vector3(Zoom, Zoom, 1)) *
                Matrix.CreateTranslation(new Vector3(viewport.Width * 0.5f, viewport.Height * 0.5f, 0)
            );

            return Transform;
        }
    }
}
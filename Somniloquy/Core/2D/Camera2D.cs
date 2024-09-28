namespace Somniloquy {
    using System;
    using System.Collections.Generic;

    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using MonoGame.Extended;

    public class Camera2D {
        public const float LerpModifier =  0.1f;

        public Vector2 TargetPosition = Vector2.Zero;
        public float TargetZoom = 1f;
        public float TargetRotation = 0.0f;

        public Vector2 Position = Vector2.Zero;
        public float Zoom = 1f;
        public float Rotation = 0.0f;
        public Matrix Transform;

        public Camera2D(float zoom) {
            TargetZoom = zoom;
            Zoom = zoom;
        }

        public void MoveCamera(Vector2 displacement) {
            TargetPosition += displacement * 10 / MathF.Sqrt(TargetZoom);
        }

        public void ZoomCamera(float delta) {
            TargetZoom *= MathF.Pow(MathF.E, delta);
            TargetZoom = MathF.Min(MathF.Max(TargetZoom, 1f), 64f);
        }

        public void UpdateTransformation() {
            Position.X = Utils.Lerp(Position.X, TargetPosition.X, LerpModifier);
            Position.Y = Utils.Lerp(Position.Y, TargetPosition.Y, LerpModifier);

            Zoom = Utils.Lerp(Zoom, TargetZoom, LerpModifier);

            TargetRotation = Utils.ModuloF(TargetRotation, 2 * MathF.PI);
            Rotation = Utils.Lerp(Rotation, TargetRotation, LerpModifier);

            Viewport viewport = SQ.GD.Viewport;

            Transform =
                Matrix.CreateTranslation(new Vector3(-Position.X, -Position.Y, 0)) *
                Matrix.CreateRotationZ(Rotation) *
                Matrix.CreateScale(new Vector3(Zoom, Zoom, 1)) *
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
            Viewport viewport = SQ.GD.Viewport;
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
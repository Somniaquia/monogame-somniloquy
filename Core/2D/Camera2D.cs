namespace Somniloquy {
    using System;
    using System.Collections.Generic;

    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    

    public class Camera2D {
        public const float LerpModifier =  0.1f;

        public Vector2 TargetCenterPosInWorld = Vector2.Zero;
        public float TargetZoom = 1f;
        public float TargetRotation = 0.0f;

        public Vector2 CenterPosInWorld = Vector2.Zero;
        public float Zoom = 1f;
        public float Rotation = 0.0f;
        public Matrix Transform;
        public RectangleF ViewportInWorld = RectangleF.Empty;

        public Vector2? GlobalMousePos;
        public Vector2? PreviousGlobalMousePos;

        public Camera2D(float zoom) {
            TargetZoom = zoom;
            Zoom = zoom;
        }

        public void MoveCamera(Vector2 displacement) {
            TargetCenterPosInWorld += displacement * 10 / MathF.Sqrt(TargetZoom);
        }

        public void ZoomCamera(float delta) {
            TargetZoom *= MathF.Pow(MathF.E, delta);
            TargetZoom = MathF.Min(MathF.Max(TargetZoom, 1f), 64f);
        }

        public void Update() {
            CenterPosInWorld.X = Util.Lerp(CenterPosInWorld.X, TargetCenterPosInWorld.X, LerpModifier);
            CenterPosInWorld.Y = Util.Lerp(CenterPosInWorld.Y, TargetCenterPosInWorld.Y, LerpModifier);

            Zoom = Util.Lerp(Zoom, TargetZoom, LerpModifier);

            TargetRotation = Util.PosMod(TargetRotation, 2 * MathF.PI);
            Rotation = Util.Lerp(Rotation, TargetRotation, LerpModifier);

            Viewport viewport = SQ.GD.Viewport;

            Transform =
                Matrix.CreateTranslation(new Vector3(-CenterPosInWorld.X, -CenterPosInWorld.Y, 0)) *
                Matrix.CreateRotationZ(Rotation) *
                Matrix.CreateScale(new Vector3(Zoom, Zoom, 1)) *
                Matrix.CreateTranslation(new Vector3(viewport.Width * 0.5f, viewport.Height * 0.5f, 0)
            );
            
            ViewportInWorld = ToWorldPos(viewport.Bounds);

            PreviousGlobalMousePos = GlobalMousePos == null ? ToWorldPos(InputManager.GetMousePosition()) : GlobalMousePos;
            GlobalMousePos = ToWorldPos(InputManager.GetMousePosition());
        }

        public Vector2 ToWorldPos(Vector2 screenPos) {
            return Vector2.Transform(screenPos, Matrix.Invert(Transform));
        }

        public RectangleF ToWorldPos(RectangleF screenRectangle) {
            return new RectangleF(
                ToScreenPos(screenRectangle.Location),
                ToScreenPos(screenRectangle.Location + screenRectangle.Size) - ToScreenPos(screenRectangle.Location)
            );
        }

        public Vector2 ToScreenPos(Vector2 worldPos) {
            return Vector2.Transform(worldPos, Transform);
        }

        public RectangleF ToScreenPos(RectangleF worldRectangle) {
            return new RectangleF(
                ToScreenPos(worldRectangle.Location),
                ToScreenPos(worldRectangle.Location + worldRectangle.Size) - ToScreenPos(worldRectangle.Location)
            );
        }

        public void Draw(Texture2D texture, Rectangle worldRectangle, Rectangle source, Color color) {
            SQ.SB.Draw(texture, (Rectangle)ToScreenPos(worldRectangle), source, color);
        }

        public void Draw(Texture2D texture, Vector2 worldPos, Rectangle source, Color color) {
            SQ.SB.Draw(texture, (Rectangle)ToScreenPos(new RectangleF(worldPos, source.Size.ToVector2())), source, color);
        }

        public void DrawPixel(Vector2I worldPos, Color color) {
            SQ.SB.Draw(SQ.SB.Pixel, (Rectangle)ToScreenPos(new Rectangle(worldPos, new(1, 1))), color);
        }
    }
}
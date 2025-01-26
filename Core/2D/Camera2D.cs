namespace Somniloquy {
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;

    public class Camera2D {
        public SQSpriteBatch SB;
        public const float LerpModifier =  0.075f;

        public Vector2 TargetCenterPosInWorld = Vector2.Zero;
        public float TargetZoom = 1f;
        public float TargetRotation = 0.0f;

        public Vector2 CenterPosInWorld = Vector2.Zero;
        public float Zoom = 1f;
        public float MaxZoom = float.PositiveInfinity;
        public float MinZoom = 0;
        public float Rotation = 0.0f;
        public Matrix Transform;
        public RectangleF VisibleBounds = RectangleF.Empty;

        public Vector2? GlobalMousePos;
        public Vector2? PreviousGlobalMousePos;

        public static Queue<float> MouseSpeedSamples = new();
        public float AverageMouseSpeed;

        public void MoveCamera(Vector2 displacement) {
            float theta = MathF.Atan2(displacement.Y, displacement.X) - Rotation;
            displacement = new(displacement.Length() * MathF.Cos(theta), displacement.Length() * MathF.Sin(theta));
            // TargetCenterPosInWorld += displacement * 10 / MathF.Sqrt(TargetZoomInverse);
            TargetCenterPosInWorld += displacement * 10 / TargetZoom;
        }

        public void ZoomCamera(float delta) {
            TargetZoom *= MathF.Pow(MathF.E, delta);
            TargetZoom = MathF.Min(MathF.Max(TargetZoom, MinZoom), MaxZoom);
        }

        public void RotateCamera(float delta) {
            TargetRotation += delta;
        }

        public void LoadContent() {
            SB = new(SQ.GD);
            // DebugInfo.Subscribe(() => $"Zoom: {Zoom}");
        }

        public void Update() {
            CenterPosInWorld.X = Util.Lerp(CenterPosInWorld.X, TargetCenterPosInWorld.X, LerpModifier);
            CenterPosInWorld.Y = Util.Lerp(CenterPosInWorld.Y, TargetCenterPosInWorld.Y, LerpModifier);

            Zoom = Util.Lerp(Zoom, TargetZoom, LerpModifier);

            // TargetRotation = Util.PosMod(TargetRotation, 2 * MathF.PI);
            Rotation = Util.Lerp(Rotation, TargetRotation, LerpModifier);

            Rectangle bounds = SQ.GD.Viewport.Bounds;

            Transform =
                Matrix.CreateTranslation(new Vector3(-CenterPosInWorld.X, -CenterPosInWorld.Y, 0)) *
                Matrix.CreateRotationZ(Rotation) *
                Matrix.CreateScale(new Vector3(Zoom, Zoom, 1)) *
                Matrix.CreateTranslation(new Vector3(bounds.Width * 0.5f, bounds.Height * 0.5f, 0)
            );
            
            var topLeft = ToWorldPos(bounds.TopLeft());
            var topRight = ToWorldPos(bounds.TopRight());
            var bottomLeft = ToWorldPos(bounds.BottomLeft());
            var bottomRight = ToWorldPos(bounds.BottomRight());
            
            var left = Util.Min(topLeft.X, topRight.X, bottomLeft.X, bottomRight.X);
            var right = Util.Max(topLeft.X, topRight.X, bottomLeft.X, bottomRight.X);
            var up = Util.Min(topLeft.Y, topRight.Y, bottomLeft.Y, bottomRight.Y);
            var down = Util.Max(topLeft.Y, topRight.Y, bottomLeft.Y, bottomRight.Y);

            VisibleBounds = new RectangleF(left, up, right - left, down - up);

            PreviousGlobalMousePos = GlobalMousePos == null ? ToWorldPos(InputManager.GetMousePosition()) : GlobalMousePos;
            GlobalMousePos = ToWorldPos(InputManager.GetMousePosition());
            
            if (PreviousGlobalMousePos is null || GlobalMousePos is null) return;
            float currentMouseSpeed = (GlobalMousePos.Value - PreviousGlobalMousePos.Value).Length() / (float)SQ.GameTime.ElapsedGameTime.TotalSeconds;
            MouseSpeedSamples.Enqueue(currentMouseSpeed);

            if (MouseSpeedSamples.Count > 20) {
                MouseSpeedSamples.Dequeue();
                AverageMouseSpeed = MouseSpeedSamples.Average(i => i);
            } else {
                AverageMouseSpeed = currentMouseSpeed;
            }
        }

        public Vector2 ToWorldPos(Vector2 screenPos) {
            return Vector2.Transform(screenPos, Matrix.Invert(Transform));
        }

        public RectangleF ToWorldPos(RectangleF screenRectangle) {
            return new RectangleF(
                ToWorldPos(screenRectangle.Location),
                ToWorldPos(screenRectangle.Location + screenRectangle.Size) - ToWorldPos(screenRectangle.Location)
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

        // TODO: Make drawing work with rotation with a RenderTarget
        public void Draw(Texture2D texture, RectangleF worldRectangle, Rectangle source, Color color) {
            SB.Draw(texture, (Rectangle)worldRectangle, source, color);
        }

        public void Draw(Texture2D texture, RectangleF worldRectangle, Color color) {
            SB.Draw(texture, (Rectangle)worldRectangle, color);
        }

        public void Draw(Texture2D texture, Vector2 worldPos, Rectangle source, Color color) {
            SB.Draw(texture, (Rectangle)new RectangleF(worldPos, source.Size.ToVector2()), source, color);
        }

        public void DrawPoint(Vector2I worldPos, Color color) {
            SB.Draw(SB.Pixel, new Rectangle(worldPos, new(1, 1)), color);
        }

        public void DrawFilledRectangle(RectangleF destination, Color color) {
            SB.Draw(SB.Pixel, (Rectangle)destination, null, color, 0f, Vector2.Zero, SpriteEffects.None, 0f);
        }
 
        public void DrawLine(Vector2I start, Vector2I end, Color color, int width = 0, bool scale = false) {
            if (scale) {
                PixelActions.ApplyLineAction(start, end, width, (Vector2I position) => {
                    DrawPoint(position, color);
                });
            } else {
                SQ.SB.DrawLine((Vector2I)ToScreenPos(start), (Vector2I)ToScreenPos(end), color);
            }
        }

        public void DrawCircle(Vector2I center, int radius, Color color, bool filled) {
            PixelActions.ApplyCircleAction(center, radius, filled, (Vector2I position) => {
                DrawPoint(position, color);
            });
        }
    }
}
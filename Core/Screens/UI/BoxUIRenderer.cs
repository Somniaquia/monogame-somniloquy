namespace Somniloquy {
    using Microsoft.Xna.Framework;

    public abstract class BoxUIRenderer {
        public abstract void Draw(RectangleF rectangle);
    }

    public class BoxUIDebugRenderer : BoxUIRenderer {
        public override void Draw(RectangleF rectangle) {
            SQ.SB.DrawLine((Vector2I)rectangle.TopLeft(), (Vector2I)rectangle.TopRight(), Color.Red * 0.5f, 1);
            SQ.SB.DrawLine((Vector2I)rectangle.TopRight(), (Vector2I)rectangle.BottomRight(), Color.Green * 0.5f, 1);
            SQ.SB.DrawLine((Vector2I)rectangle.BottomRight(), (Vector2I)rectangle.BottomLeft(), Color.Blue * 0.5f, 1);
            SQ.SB.DrawLine((Vector2I)rectangle.BottomLeft(), (Vector2I)rectangle.TopLeft(), Color.Purple * 0.5f, 1);
        }
    }
}
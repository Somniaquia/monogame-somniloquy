namespace Somniloquy {
    using Microsoft.Xna.Framework;

    public abstract class BoxUIRenderer {
        public abstract void Draw(RectangleF rectangle);
    }

    public class BoxUIDebugRenderer : BoxUIRenderer {
        public override void Draw(RectangleF rectangle) {
            SQ.SB.DrawLine((Vector2I)rectangle.TopLeft(), (Vector2I)rectangle.TopRight(), Color.Red, 0);
            SQ.SB.DrawLine((Vector2I)rectangle.TopRight(), (Vector2I)rectangle.BottomRight(), Color.Green, 0);
            SQ.SB.DrawLine((Vector2I)rectangle.BottomRight(), (Vector2I)rectangle.BottomLeft(), Color.Blue, 0);
            SQ.SB.DrawLine((Vector2I)rectangle.BottomLeft(), (Vector2I)rectangle.TopLeft(), Color.Purple, 0);
        }
    }

    public class BoxUIDefaultRenderer : BoxUIRenderer {
        public Color Color;

        public BoxUIDefaultRenderer() : this(Color.White) { }
        public BoxUIDefaultRenderer(Color color) {
            Color = color;
        }

        public override void Draw(RectangleF rectangle) {
            SQ.SB.DrawRectangle(rectangle, Color);
        }
    }
}
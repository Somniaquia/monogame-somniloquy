namespace Somniloquy {
    using System.Linq;
    using Microsoft.Xna.Framework;

    public class TextLabel : BoxUI {
        public string Text;
        public Color DefaultColor = Color.White;
        public bool Editable;
        public Axis LetterAxis; // is each letter standing upright are or lying 90 degrees

        public TextLabel(BoxUI parent, float margin = 0, float padding = 0, string text = "", Axis letterAxis = Axis.Horizontal, bool editable = false) : base(parent, margin, padding) {
            Text = text;
            Editable = editable;
            LetterAxis = letterAxis;
        }

        public TextLabel(Rectangle boundaries, string text = "", Axis letterAxis = Axis.Horizontal, bool editable = false) : base(boundaries) {
            Text = text;
            Editable = editable;
            LetterAxis = letterAxis;
        }
            
        public override float GetContentLength(Axis axis) {
            // TODO: Flexible indentation (wrapping text inside max axisLength)
            float length = base.GetContentLength(axis);
            if (LetterAxis == axis) {
                Vector2 dimensions = SQ.Misaki.MeasureString(Text);
                length += axis == MainAxis ? dimensions.X : dimensions.Y;
            } else {
                if (axis == MainAxis) {
                    length += Text.Split('\n').Max(line => line.Length) * SQ.Misaki.LineSpacing;
                } else {
                    length += (Text.Count(s => s == '\n') + 1) * SQ.Misaki.LineSpacing;
                }
            }

            return length;
        }

        public override void Draw() {
            base.Draw();
            SQ.SB.DrawString(SQ.Misaki, Text, Boundaries.TopLeft() + Padding.TopLeft(), DefaultColor);
        }
    }
}
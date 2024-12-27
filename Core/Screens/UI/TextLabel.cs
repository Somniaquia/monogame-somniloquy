namespace Somniloquy {
    using System.Linq;
    using Microsoft.Xna.Framework;

    public class TextLabel : BoxScreen {
        public string Text;
        public bool Editable;
        public Axis AlignAxis; // Line up letters in either horizontal or vertical
        public Axis LetterAxis; // is each letter standing upright are or lying 90 degrees

        public TextLabel(BoxScreen parent, string text, bool editable = false, Axis textAxis = Axis.Horizontal, Axis letterAxis = Axis.Vertical, float margin = 0) : base(parent, margin: margin) {
            Text = text;
            Editable = editable;
            AlignAxis = textAxis;
            LetterAxis = letterAxis;

            Renderer = null;
        }

        public TextLabel(Rectangle boundaries, string text, bool editable = false, Axis textAxis = Axis.Horizontal, Axis letterAxis = Axis.Vertical) : base(boundaries) {
            Text = text;
            Editable = editable;
            AlignAxis = textAxis;
            LetterAxis = letterAxis;

            Renderer = null;
        }
            
        public override float GetMinimumLength(Axis axis) {
            // 0 0 0 -> 0           This is stupidly overengineered and I hope that I have done this right
            // 0 0 1 -> 1
            // 0 1 0 -> 1
            // 0 1 1 -> 0
            // 1 1 0 -> 1
            // 1 1 1 -> 0
            // 1 0 0 -> 0
            // 1 0 1 -> 1

            bool a = AlignAxis == Axis.Vertical;
            bool b = LetterAxis == Axis.Vertical;
            bool c = axis == Axis.Vertical;

            bool result = c ^ (a ^ b);

            // TODO: Flexible indentation (wrapping text inside max axisLength)
            int length = 0;
            if (result) {
                length = Text.Count(t => t == '\n') * SQ.Misaki.LineSpacing;
            } else {
                int maxLineLength = Text.Split('\n').Max(line => line.Length);
                length = Text.Count(t => t == '\n') * SQ.Misaki.LineSpacing;
            }

            return length + base.GetMinimumLength(axis);
        }

        public override void Draw() {
            SQ.SB.DrawString(SQ.Misaki, Text, Boundaries.TopLeft(), Color.White);
            base.Draw();
        }
    }
}
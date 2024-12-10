namespace Somniloquy {
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Microsoft.Xna.Framework.Input;

    public enum Axis { Horizontal, Vertical }
    public enum Align { Start, Center, End }
    
    public struct Sides {
        public float Left, Right, Up, Down;
        
        public static Sides None => new Sides(0, 0, 0, 0);

        public Sides(float all) : this(all, all, all, all) { }
        public Sides(float horizontal, float vertical) : this(horizontal, horizontal, vertical, vertical) { }
        
        public Sides(float left, float right, float up, float down) {
            Left = left;
            Right = right;
            Up = up;
            Down = down;
        }

        public float GetSide(Axis axis, bool start) {
            if (axis == Axis.Horizontal) {
                if (start) return Left;
                else return Right;
            } else {
                if (start) return Up;
                else return Down;
            }
        }

        public Sides GetPart(Axis axis, bool perpendicular = false) {
            if (axis == Axis.Horizontal ^ perpendicular) {
                return new Sides(Left, Right, 0, 0);
            } else {
                return new Sides(0, 0, Up, Down);
            }
        }
    }

    public abstract class Screen {
        public List<Screen> Children = new();
        public bool Focusable = true;
        public bool Selectable = false;

        public virtual void LoadContent() { }

        /// <summary>
        /// The base update function focuses the screen if the mouse is inside it. Call base.Update() at top!
        /// </summary>
        public virtual void Update() {
            if (Focusable) {
                if (MouseWithinBoundaries() && !InputManager.IsMouseButtonDown(MouseButtons.LeftButton)) {
                    ScreenManager.FocusedScreen = this;
                }
            }

            foreach (var child in Children) {
                child.Update();
            }
        }

        public abstract bool MouseWithinBoundaries();

        public virtual void Draw() {
            foreach (var child in Children) {
                child.Draw();
            }
        }

        public bool IsFocused() {
            return ScreenManager.FocusedScreen == this;
        }

        public bool IsSelected() {
            return ScreenManager.SelectedScreen == this;
        }
    }

    public abstract class BoxUIRenderer {
        public abstract void Draw(RectangleF rectangle);
    }

    public class BoxUIDebugRenderer : BoxUIRenderer {
        public override void Draw(RectangleF rectangle) {
            SQ.SB.DrawLine((Vector2I)rectangle.TopLeft(), (Vector2I)rectangle.TopRight(), Color.Red * 0.5f, 0);
            SQ.SB.DrawLine((Vector2I)rectangle.TopRight(), (Vector2I)rectangle.BottomRight(), Color.Green * 0.5f, 0);
            SQ.SB.DrawLine((Vector2I)rectangle.BottomRight(), (Vector2I)rectangle.BottomLeft(), Color.Blue * 0.5f, 0);
            SQ.SB.DrawLine((Vector2I)rectangle.BottomLeft(), (Vector2I)rectangle.TopLeft(), Color.Purple * 0.5f, 0);
        }
    }

    /// <summary>
    /// Boxy Screen elements
    /// </summary>
    public class BoxScreen : Screen {
        public BoxScreen Parent;
        public RectangleF Boundaries;
        public Matrix Transform;
        public BoxUIRenderer Renderer = new BoxUIDebugRenderer();

        public Sides Margin = Sides.None;
        public Sides Padding = Sides.None;

        public Axis Axis = Axis.Horizontal;
        public Align AxisAlign = Align.Start;
        public Align PerpendicularAlign = Align.Start;

        public float FillStrength = 0f; // 0 for fitting to minimum length

        public RectangleF CalculateBoundaries() {
            Debug.Assert(Parent is not null);
            RectangleF availableSpace = Util.ShrinkRectangle(Parent.Boundaries, Parent.Padding.GetPart(Parent.Axis, true));
            List<BoxScreen> Siblings = Parent.Children.OfType<BoxScreen>().ToList();
            
            float availableLength = Parent.Axis == Axis.Horizontal ? availableSpace.Width : availableSpace.Height;
            float accumulatedFillStrength = 0;
            float accumulatedLength = 0;

            if (Siblings.Count > 1) {
                availableLength -= Util.Max(Parent.Padding.GetSide(Parent.Axis, true), Siblings[0].Margin.GetSide(Parent.Axis, true));
                availableLength -= Util.Max(Parent.Padding.GetSide(Parent.Axis, false), Siblings.Last().Margin.GetSide(Parent.Axis, false));

                for (int i = 0; i < Siblings.Count - 1; i++) {
                    availableLength -= Util.Max(Siblings[i].Margin.GetSide(Parent.Axis, false), Siblings[i + 1].Margin.GetSide(Parent.Axis, true));
                    accumulatedFillStrength += Siblings[i].FillStrength;
                }
                
                accumulatedFillStrength += Siblings.Last().FillStrength;

                
                int j = 0;
                accumulatedLength += Util.Max(Parent.Padding.GetSide(Parent.Axis, true), Siblings[0].Margin.GetSide(Parent.Axis, true));

                for (j = 0; j < Siblings.FindIndex(j => j == this); j++) {
                    accumulatedLength += availableLength * (Siblings[j].FillStrength / accumulatedFillStrength);
                    accumulatedLength += Util.Max(Siblings[j].Margin.GetSide(Parent.Axis, false), Siblings[j + 1].Margin.GetSide(Parent.Axis, true));
                }
            }

            if (Parent.Axis == Axis.Horizontal) {
                var paddingUp = Util.Max(Parent.Padding.Up, Margin.Up);
                var paddingDown = Util.Max(Parent.Padding.Down, Margin.Down);
                Boundaries = new RectangleF(Parent.Boundaries.X + accumulatedLength, Parent.Boundaries.Y + paddingUp, availableLength * FillStrength / accumulatedFillStrength, Parent.Boundaries.Height - paddingUp - paddingDown);
            } else {
                var paddingLeft = Util.Max(Parent.Padding.Left, Margin.Left);
                var paddingRight = Util.Max(Parent.Padding.Right, Margin.Right);
                Boundaries = new RectangleF(Parent.Boundaries.X + paddingLeft, Parent.Boundaries.Y + accumulatedLength, Parent.Boundaries.Width - paddingLeft - paddingRight, availableLength * FillStrength / accumulatedFillStrength);
            }

            return Boundaries;
        }

        public BoxScreen(BoxScreen parent, float fillStrength = 1f, float margin = 0, float padding = 0) : this(parent, new Sides(margin), new Sides(padding), fillStrength) {}

        public BoxScreen(BoxScreen parent, Sides margin, Sides padding, float fillStrength = 1f, Axis axis = Axis.Horizontal, Align axisAlign = Align.Start, Align perpendicularAlign = Align.Start) {
            Parent = parent;
            Margin = margin;
            Padding = padding;
            FillStrength = fillStrength;
            Axis = axis;
            AxisAlign = axisAlign;
            PerpendicularAlign = perpendicularAlign;

            parent.Children.Add(this);
            foreach (var screen in parent.Children.OfType<BoxScreen>()) {
                screen.CalculateBoundaries();
            }
        }

        public BoxScreen(Rectangle boundaries) {
            Boundaries = boundaries;
            Transform = Matrix.CreateTranslation(-boundaries.X, -boundaries.Y, 0f) * 
                        Matrix.CreateScale(1f / boundaries.Width, 1f / boundaries.Height, 1f);
        }
        
        public BoxScreen(Rectangle boundaries, Sides padding, Axis axis, Align axisAlign = Align.Start, Align perpendicularAlign = Align.Start) {
            Boundaries = boundaries;
            Transform = Matrix.CreateTranslation(-boundaries.X, -boundaries.Y, 0f) * 
                        Matrix.CreateScale(1f / boundaries.Width, 1f / boundaries.Height, 1f);

            Padding = padding;
            Axis = axis;
            AxisAlign = axisAlign;
            PerpendicularAlign = perpendicularAlign;
        }

        public override bool MouseWithinBoundaries() {
            return Util.IsWithinBoundaries(InputManager.GetMousePosition(), Boundaries);
        }

        public virtual float GetMinimumLength(Axis axis) {
            float length = 0;
            foreach (var child in Children) {
                if (child is BoxScreen rectangleChild) {
                    length += rectangleChild.GetMinimumLength(axis);
                } 
            }

            float parentPadding = Parent is null ? 0 : Parent.Padding.Left + Parent.Padding.Right;
            return length + Util.Max(Margin.Left + Margin.Right, parentPadding);
        }

        public override void Update() { // TODO: Dynamic resizing
            base.Update();
        }

        public override void Draw() {
            if (Renderer is not null) {
                Renderer.Draw(Boundaries);
            }
            base.Draw();
        }
    }

    public class TextLabel : BoxScreen {
        public string Text;
        public bool Editable;
        public Axis AlignAxis; // Line up letters in either horizontal or vertical
        public Axis LetterAxis; // is each letter standing upright are or lying 90 degrees

        public TextLabel(BoxScreen parent, string text, bool editable = false, Axis textAxis = Axis.Horizontal, Axis letterAxis = Axis.Vertical, float padding = 10) : base(parent, padding) {
            Text = text;
            Editable = editable;
            AlignAxis = textAxis;
            LetterAxis = letterAxis;
        }

        public TextLabel(Rectangle boundaries, string text, bool editable = false, Axis textAxis = Axis.Horizontal, Axis letterAxis = Axis.Vertical) : base(boundaries) {
            Text = text;
            Editable = editable;
            AlignAxis = textAxis;
            LetterAxis = letterAxis;
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

            base.Draw();
        }
    }
}
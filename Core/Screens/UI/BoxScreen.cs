namespace Somniloquy {
    using System.Collections.Generic;
    using Microsoft.Xna.Framework;
    
    // Elements implementing this interface can have children. Neat! 
    // But well is this interface necessary
    // idk
    public interface IFertile {
        public abstract void InsertElement(int index, BoxScreen screen);
        public abstract List<Screen> GetChildren();
    }

    /// <summary>
    /// Boxy Screen elements
    /// </summary>
    public abstract class BoxScreen : Screen {
        public BoxScreen Parent;
        public RectangleF Boundaries;
        public Matrix Transform;
        public BoxUIRenderer Renderer = new BoxUIDebugRenderer();

        public Sides Margin = Sides.None;
        public Sides Padding = Sides.None;

        public Axis Axis = Axis.Horizontal;
        public Align AxisAlign = Align.Begin;
        public Align PerpendicularAlign = Align.Begin;

        public BoxScreen() {}
        public BoxScreen(BoxScreen parent, float margin = 0, float padding = 0) : this(parent, new Sides(margin), new Sides(padding), Axis.Horizontal, Align.Begin, Align.Begin) { }
        
        public BoxScreen(BoxScreen parent, Sides margin, Sides padding, Axis axis, Align axisAlign, Align perpendicularAlign) {
            Parent = parent;
            Margin = margin;
            Padding = padding;
            Axis = axis;
            AxisAlign = axisAlign;
            PerpendicularAlign = perpendicularAlign;

            if (parent is IFertile fertileParent) {
                fertileParent.InsertElement(fertileParent.GetChildren().Count, this);
            }
        }

        public BoxScreen(Rectangle boundaries) {
            Boundaries = boundaries;
            Transform = Matrix.CreateTranslation(-boundaries.X, -boundaries.Y, 0f) * 
                        Matrix.CreateScale(1f / boundaries.Width, 1f / boundaries.Height, 1f);
        }
        
        public BoxScreen(Rectangle boundaries, Sides padding, Axis axis, Align axisAlign, Align perpendicularAlign) {
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
}
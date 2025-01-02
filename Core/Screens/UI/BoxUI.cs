namespace Somniloquy {
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;

    public enum Axis { Horizontal, Vertical }
    public enum Align { Begin, Center, End, Even, Custom }

    /// <summary>
    /// Boxy Screen elements
    /// </summary>
    public class BoxUI : Screen {
        public BoxUI Parent;
        public RectangleF Boundaries;
        public Matrix Transform;
        public BoxUIRenderer Renderer = new BoxUIDebugRenderer();

        public Sides Margin = Sides.None;
        public Sides Padding = Sides.None;

        public Axis MainAxis = Axis.Vertical;
        public Axis PerpendicularAxis => Util.Perpendicular(MainAxis);
        
        public Align MainAxisAlign = Align.Begin;
        public List<float> DivisionLocations = new();
        public Align PerpendicularAxisAlign = Align.Center;
        
        public RenderTarget2D ContentRenderTaret;
        public float ScrollValue;

        public bool MainAxisFill = false;
        public bool PerpendicularAxisFill = true;

        public BoxUI() {}

        public BoxUI(BoxUI parent, float margin = 20, float padding = 20) : this(parent, new Sides(margin), new Sides(padding), Axis.Horizontal, Align.Begin, Align.Center) { }
        public BoxUI(BoxUI parent, Sides margin, Sides padding, Axis axis, Align mainAxisAlign, Align perpendicularAxisAlign) {
            Parent = parent;
            Margin = margin;
            Padding = padding;
            MainAxis = axis;
            MainAxisAlign = mainAxisAlign;
            PerpendicularAxisAlign = perpendicularAxisAlign;
        }

        public BoxUI(RectangleF boundaries) : this(boundaries, Sides.None, Axis.Vertical, Align.Begin, Align.Center) { }
        public BoxUI(RectangleF boundaries, Sides padding, Axis axis, Align mainAxisAlign, Align perpendicularAxisAlign) {
            Boundaries = boundaries;
            Transform = Matrix.CreateTranslation(-boundaries.X, -boundaries.Y, 0f) * 
                        Matrix.CreateScale(1f / boundaries.Width, 1f / boundaries.Height, 1f);

            Padding = padding;
            MainAxis = axis;
            MainAxisAlign = mainAxisAlign;
            PerpendicularAxisAlign = perpendicularAxisAlign;

            ScreenManager.AddScreen(this);
        }

        public override bool MouseWithinBoundaries() {
            return Util.IsWithinBoundaries(InputManager.GetMousePosition(), Boundaries);
        }

        public BoxUI AddChild(BoxUI child) => InsertChild(Children.Count, child);
        public BoxUI InsertChild(int index, BoxUI child) {
            Children.Insert(index, child);
            PositionChildren();
            return child;
        }

        public void PositionChildren() {
            var children = Children.OfType<BoxUI>().ToList();
            
            if (MainAxisAlign == Align.Custom) {

            } else {
                float contentLength = GetContentLength(MainAxis);
                float MaxLength = GetMaxLength(MainAxis);

                if (contentLength <= MaxLength) {
                    if (MainAxisAlign == Align.Begin) {
                        float position = MainAxis == Axis.Horizontal ? Boundaries.X : Boundaries.Y;
                        position += Util.Max(Padding.GetSide(MainAxis, false), children[0].Margin.GetSide(MainAxis, true));
                        position += ScrollValue;

                        for (int i = 0; i < children.Count - 1; i++) {
                            children[i].SetBoundariesAxis(MainAxis, position, position + children[i].GetContentLength(MainAxis));
                            position += children[i].GetContentLength(MainAxis);
                            position += Util.Max(children[i].Margin.GetSide(MainAxis, true), children[i + 1].Margin.GetSide(MainAxis, false));
                        }
                        children[^1].SetBoundariesAxis(MainAxis, position, position + children[^1].GetContentLength(MainAxis));
                    } else if (MainAxisAlign == Align.End) {

                    } else if (MainAxisAlign == Align.Center) {
                        
                    } else if (MainAxisAlign == Align.Even) {

                    }
                } else { // Mostly same behavior, adds a scrollbar and place children in a seperate place, having a renderrtarget
                    
                }

                // Perpendicular positioning
                MaxLength = GetMaxLength(PerpendicularAxis);
                
                foreach (var child in children) {
                    contentLength = child.GetContentLength(PerpendicularAxis);

                    if (contentLength < MaxLength) {
                        if (PerpendicularAxisAlign == Align.Begin) {

                        } else if (PerpendicularAxisAlign == Align.End) {

                        } else if (PerpendicularAxisAlign is Align.Center or Align.Even) {
                            var begin = Boundaries.GetSide(PerpendicularAxis, true) 
                                + Util.Max(Padding.GetSide(PerpendicularAxis, true), child.Margin.GetSide(PerpendicularAxis, true));
                            var end = Boundaries.GetSide(PerpendicularAxis, false) 
                                - Util.Max(Padding.GetSide(PerpendicularAxis, false), child.Margin.GetSide(PerpendicularAxis, false));
                            
                            child.SetBoundariesAxis(PerpendicularAxis, begin, end);
                        }
                    } else {
                        
                    }
                }
            }

            Parent?.PositionChildren();
        }

        

        // An element cannot be expanded if it is the Root element, a child of a divisionPositioner
        public float GetMaxLength(Axis axis) {
            if (Parent is null || Parent.GetAxisAlign(axis) is Align.Custom) {
                return axis == Axis.Horizontal ? Boundaries.Width : Boundaries.Height;
            } else {
                return float.PositiveInfinity;
            }
        }

        public Align GetAxisAlign(Axis axis) {
            return axis == MainAxis ? MainAxisAlign : PerpendicularAxisAlign;
        }

        public virtual float GetContentLength(Axis axis) {
            var children = Children.OfType<BoxUI>().ToList();
            if (children.Count == 0) return Padding.GetSideSum(axis);

            if (MainAxis == axis) {
                float length = Util.Max(Padding.GetSide(axis, false), children[0].Margin.GetSide(axis, true))
                    + Util.Max(children[^1].Margin.GetSide(axis, false), Padding.GetSide(axis, true));

                for (int i = 0; i < children.Count - 1; i++) {
                    length += children[i].GetContentLength(axis);
                    length += Util.Max(children[i].Margin.GetSide(axis, true), children[i + 1].Margin.GetSide(axis, false));
                }
                length += children[^1].GetContentLength(axis);

                return length;
            } else {
                return children.Max(child => child.GetContentLength(axis));
            }
        }

        public void SetBoundariesAxis(Axis axis, float begin, float end) {
            if (axis == Axis.Horizontal) {
                Boundaries = new(begin, Boundaries.Y, end - begin, Boundaries.Height); 
            } else {
                Boundaries = new(Boundaries.X, begin, Boundaries.Width, end - begin); 
            }
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
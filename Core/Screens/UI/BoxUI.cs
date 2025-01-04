namespace Somniloquy {
    using System;
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
        public BoxUIRenderer Renderer = new BoxUIDefaultRenderer();

        public Sides Margin;
        public Sides Padding;

        public Axis MainAxis;
        public Axis PerpendicularAxis => Util.Perpendicular(MainAxis);
        
        public Align MainAxisAlign = Align.Begin;
        public Align MainAxisAlignOverflow = Align.Begin;
        public List<float> DivisionLocations = new();
        public Align PerpendicularAxisAlign = Align.Center;
        
        public RenderTarget2D ContentRenderTarget;
        public float ScrollValue;

        public bool MainAxisFill = false;
        public bool PerpendicularAxisFill = true;

        public BoxUI() {}

        public BoxUI(BoxUI parent, float margin = 0, float padding = 0) : this(parent, new Sides(margin), new Sides(padding), Axis.Horizontal, Align.Begin, Align.Center) { }
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
                float maxLength = GetMaxLength(MainAxis);

                if (contentLength > maxLength) {
                    ScrollValue = Math.Clamp(ScrollValue, 0, contentLength - maxLength);
                    if (ContentRenderTarget == null || ContentRenderTarget.Width != (int)maxLength) {
                        ContentRenderTarget = new RenderTarget2D(SQ.GD, (int)maxLength, (int)Boundaries.Height);
                    }
                } else {
                    ScrollValue = 0;
                    ContentRenderTarget?.Dispose();
                    ContentRenderTarget = null;
                }

                var alignMode = contentLength > maxLength ? MainAxisAlignOverflow : MainAxisAlign;

                if (alignMode == Align.Begin) {
                    float position = MainAxis == Axis.Horizontal ? Boundaries.X : Boundaries.Y;
                    position += ScrollValue;

                    for (int i = 0; i < children.Count; i++) {
                        position += GetSeperationByIndex(MainAxis, i);
                        var thisLength = children[i].GetContentLength(MainAxis);
                        thisLength = children[i].MainAxisFill ? maxLength - contentLength + thisLength : thisLength;
                        children[i].SetBoundariesAxis(MainAxis, position, position += thisLength);
                    }
                } else if (alignMode == Align.End) {
                    
                } else if (alignMode == Align.Center) {
                    
                } else if (alignMode == Align.Even) {

                }

                // Perpendicular positioning
                maxLength = GetMaxLength(PerpendicularAxis);
                
                foreach (var child in children) {
                    contentLength = child.GetContentLength(PerpendicularAxis);

                    if (contentLength < maxLength) {
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
                        throw new Exception("AAAAAAAA Unimplemented!!");
                    }
                }
            }

            Parent?.PositionChildren();
        }

        public float GetSeperationByIndex(Axis axis, int i) {
            float start = i == 0 ? Padding.GetSide(axis, false) : ((BoxUI)Children[i - 1]).Margin.GetSide(axis, false);
            float end = i == Children.Count ? Padding.GetSide(axis, true) : ((BoxUI)Children[i]).Margin.GetSide(axis, true);

            return Util.Max(start, end);
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
                float length = 0;

                for (int i = 0; i < children.Count; i++) {
                    length += GetSeperationByIndex(axis, i);
                    length += children[i].GetContentLength(axis);
                }
                length += GetSeperationByIndex(axis, children.Count);

                return length;
            } else {
                return children.Max(child => Util.Max(child.Margin.GetSide(axis, true), Padding.GetSide(axis, true)) + child.GetContentLength(axis) + Util.Max(child.Margin.GetSide(axis, false), Padding.GetSide(axis, false)));
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
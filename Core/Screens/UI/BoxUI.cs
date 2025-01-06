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
        public BoxUI Root; public bool RepositioningNeeded = false;
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
        
        public bool Overflowed;
        public RenderTarget2D ContentRenderTarget;
        public float ScrollValue;
        public float SmoothScrollValue;

        public bool MainAxisFill = false;
        public bool PerpendicularAxisFill = true;

        public bool MainAxisShrink = false;
        public bool PerpendicularAxisShrink = false;

        public BoxUI() {}

        public BoxUI(BoxUI parent, float margin = 0, float padding = 0) : this(parent, new Sides(margin), new Sides(padding), Axis.Horizontal, Align.Begin, Align.Center) { }
        public BoxUI(BoxUI parent, Sides margin, Sides padding, Axis axis, Align mainAxisAlign, Align perpendicularAxisAlign) {
            Root = parent.Root;
            Parent = parent;
            Margin = margin;
            Padding = padding;
            MainAxis = axis;
            MainAxisAlign = mainAxisAlign;
            PerpendicularAxisAlign = perpendicularAxisAlign;
        }

        public BoxUI(RectangleF boundaries) : this(boundaries, Sides.None, Axis.Vertical, Align.Begin, Align.Center) { }
        public BoxUI(RectangleF boundaries, Sides padding, Axis axis, Align mainAxisAlign, Align perpendicularAxisAlign) {
            Root = this;
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
            return child;
        }

        public void PositionChildren() {
            var children = Children.OfType<BoxUI>().ToList();
            
            if (MainAxisAlign == Align.Custom) {

            } else {
                float contentLength = GetContentLength(MainAxis, this);
                float maxLength = GetMaxLength(MainAxis);
                Overflowed = contentLength > maxLength;

                if (Overflowed) {
                    ScrollValue = Math.Clamp(ScrollValue, 0, contentLength - maxLength);
                    if (ContentRenderTarget == null || ContentRenderTarget.Bounds.GetAxisLength(MainAxis) != (int)maxLength) {
                        ContentRenderTarget = MainAxis == Axis.Horizontal ? new RenderTarget2D(SQ.GD, (int)maxLength, (int)Boundaries.Height) : new RenderTarget2D(SQ.GD, (int)Boundaries.Width, (int)maxLength);
                    }
                } else {
                    ScrollValue = 0;
                    ContentRenderTarget?.Dispose();
                    ContentRenderTarget = null;
                }

                var alignMode = Overflowed ? MainAxisAlignOverflow : MainAxisAlign;

                if (alignMode == Align.Begin) {
                    float position = MainAxis == Axis.Horizontal ? Boundaries.X : Boundaries.Y;
                    position -= ScrollValue;

                    for (int i = 0; i < children.Count; i++) {
                        position += GetSeperationByIndex(MainAxis, i);
                        var thisLength = children[i].GetContentLength(MainAxis, this);
                        if (children[i].MainAxisFill) thisLength = maxLength - (contentLength - thisLength);
                        children[i].SetBoundariesAxis(MainAxis, position, position += thisLength);
                    }
                } else if (alignMode == Align.End) {
                    
                } else if (alignMode == Align.Center) {
                    
                } else if (alignMode == Align.Even) {

                }
            }

            float perpendicularMaxLength = GetMaxLength(PerpendicularAxis);
            
            foreach (var child in children) {
                var contentLength = child.GetContentLength(PerpendicularAxis, this);

                if (PerpendicularAxisAlign == Align.Begin) {

                } else if (PerpendicularAxisAlign == Align.End) {

                } else if (PerpendicularAxisAlign is Align.Center or Align.Even) {
                    var begin = Boundaries.GetSide(PerpendicularAxis, true)
                        + Util.Max(Padding.GetSide(PerpendicularAxis, true), child.Margin.GetSide(PerpendicularAxis, true));
                    var end = Boundaries.GetSide(PerpendicularAxis, false) 
                        - Util.Max(Padding.GetSide(PerpendicularAxis, false), child.Margin.GetSide(PerpendicularAxis, false));
                    
                    child.SetBoundariesAxis(PerpendicularAxis, begin, end);
                }
            }

            foreach (var child in children) {
                child.PositionChildren();
            }
        }

        public float GetSeperationByIndex(Axis axis, int i) {
            float start = i == 0 ? Padding.GetSide(axis, false) : ((BoxUI)Children[i - 1]).Margin.GetSide(axis, false);
            float end = i == Children.Count ? Padding.GetSide(axis, true) : ((BoxUI)Children[i]).Margin.GetSide(axis, true);

            return Util.Max(start, end);
        }

        public float GetMaxLength(Axis axis) {
            return axis == Axis.Horizontal ? Boundaries.Width : Boundaries.Height;
        }

        public Align GetAxisAlign(Axis axis) {
            return axis == MainAxis ? MainAxisAlign : PerpendicularAxisAlign;
        }

        public virtual float GetContentLength(Axis axis, BoxUI caller) {
            if (caller != this && axis == MainAxis && MainAxisShrink) return 0;
            if (caller != this && axis == PerpendicularAxis && PerpendicularAxisShrink) return 0;

            var children = Children.OfType<BoxUI>().ToList();
            if (children.Count == 0) return Padding.GetSideSum(axis);

            if (MainAxis == axis) {
                float length = 0;

                for (int i = 0; i < children.Count; i++) {
                    length += GetSeperationByIndex(axis, i);
                    length += children[i].GetContentLength(axis, this);
                }
                length += GetSeperationByIndex(axis, children.Count);

                return length;
            } else {
                return children.Max(child => Util.Max(child.Margin.GetSide(axis, true), Padding.GetSide(axis, true)) + child.GetContentLength(axis, this) + Util.Max(child.Margin.GetSide(axis, false), Padding.GetSide(axis, false)));
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

            if (Focused && Overflowed) {
                ScrollValue = Math.Clamp(ScrollValue - InputManager.ScrollWheelDelta /10f, 0, GetContentLength(MainAxis, this) - Boundaries.GetAxisLength(MainAxis));
                SmoothScrollValue = Util.Lerp(SmoothScrollValue, ScrollValue, 0.075f);
            }
        }

        public override void Draw() => Draw(Vector2.Zero);
        public virtual void Draw(Vector2 displacement) {
            Renderer?.Draw(Boundaries.Displace(displacement));

            if (!Overflowed) {
                foreach (var child in Children.OfType<BoxUI>()) {
                    child.Draw(displacement);
                } 
            } else {
                SQ.SB.End();
                SQ.GD.SetRenderTarget(ContentRenderTarget);
                SQ.GD.Clear(Color.Transparent);
                SQ.SB.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);

                var scrollDisplacement = MainAxis == Axis.Horizontal ? new Vector2(SmoothScrollValue, 0) : new Vector2(0, SmoothScrollValue);
                foreach (var child in Children.OfType<BoxUI>()) { // .Where(child => Util.IntersectsOrAdjacent(child.Boundaries.Displace(scrollDisplacement), Boundaries))
                    child.Draw(-Boundaries.TopLeft());
                }

                SQ.SB.End();
                SQ.GD.SetRenderTarget(null);
                SQ.SB.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
                SQ.SB.Draw(ContentRenderTarget, (Rectangle)Boundaries, Color.White);
            }

            if (Overflowed) new BoxUIDebugRenderer().Draw(Boundaries);
        }
    }
}
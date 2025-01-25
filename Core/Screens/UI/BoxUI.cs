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
        
        public Align MainAxisAlign;
        public Align MainAxisAlignOverflow;
        public List<float> DivisionLocations = new();
        public Align PerpendicularAxisAlign;
        
        public bool Overflowed;
        public RenderTarget2D ContentRenderTarget;
        public float ScrollValue;
        public float SmoothScrollValue;

        // Contrary to other positioning flags, fill and shrink flags apply to themselves instead of children
        public bool MainAxisFill, PerpendicularAxisFill; // Fill: Fits to the parent. The parent should have minimum length or should be fitting to parent
        public float MainAxisMin = 10, PerpendicularAxisMin = 10; 
        
        public bool MainAxisShrink, PerpendicularAxisShrink; 
        public float MainAxisMax = float.MaxValue, PerpendicularAxisMax = float.MaxValue;

        public BoxUI() {}

        public BoxUI(BoxUI parent, float margin = 0, float padding = 0) : this(parent, new Sides(margin), new Sides(padding), Axis.Horizontal, Align.Begin, Align.Begin) { }
        public BoxUI(BoxUI parent, Sides margin, Sides padding, Axis axis, Align mainAxisAlign, Align perpendicularAxisAlign) {
            Root = parent.Root;
            Root.RepositioningNeeded = true;
            Parent = parent;
            Parent.Children.Add(this);
            Margin = margin;
            Padding = padding;
            MainAxis = axis;
            MainAxisAlign = mainAxisAlign;
            PerpendicularAxisAlign = perpendicularAxisAlign;
        }

        public BoxUI(RectangleF boundaries) : this(boundaries, Sides.None, Axis.Vertical, Align.Begin, Align.Begin) { }
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

        public void PositionChildren() {
            var children = Children.OfType<BoxUI>().ToList();
            
            if (MainAxisAlign == Align.Custom) {

            } else {
                float contentLength = GetContentLength(MainAxis, this);
                float maxLength = GetMaxLength(MainAxis);
                Overflowed = contentLength > maxLength;

                if (Overflowed) {
                    ScrollValue = Math.Clamp(ScrollValue, 0, contentLength - maxLength);
                    if (ContentRenderTarget == null || ContentRenderTarget.Bounds.GetAxisLength(PerpendicularAxis) != (int)GetContentLength(PerpendicularAxis, this)) {
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

                    for (int i = 0; i < children.Count; i++) {
                        position += GetSeperationByIndex(MainAxis, i);
                        var thisLength = children[i].GetContentLength(MainAxis, this);
                        if (children[i].MainAxisFill) thisLength = maxLength - (contentLength - thisLength);
                        children[i].SetBoundariesAxis(MainAxis, position, position += thisLength);
                    }
                } else if (alignMode == Align.End) {
                    float position = MainAxis == Axis.Horizontal ? Boundaries.Right : Boundaries.Bottom;

                    for (int i = 0; i < children.Count; i++) {
                        position -= GetSeperationByIndex(MainAxis, i);
                        var thisLength = children[i].GetContentLength(MainAxis, this);
                        if (children[i].MainAxisFill) thisLength = maxLength - (contentLength - thisLength);
                        children[i].SetBoundariesAxis(MainAxis, position - thisLength, position);
                        position -= thisLength;
                    }
                } else if (alignMode == Align.Center) {
                    float position = (MainAxis == Axis.Horizontal ? Boundaries.X : Boundaries.Y) + (maxLength - contentLength) / 2;

                    for (int i = 0; i < children.Count; i++) {
                        position += GetSeperationByIndex(MainAxis, i);
                        var thisLength = children[i].GetContentLength(MainAxis, this);
                        if (children[i].MainAxisFill) thisLength = maxLength - (contentLength - thisLength);
                        children[i].SetBoundariesAxis(MainAxis, position, position += thisLength);
                    }
                } else if (alignMode == Align.Even) {

                }
            }

            float perpendicularMaxLength = GetMaxLength(PerpendicularAxis);
            
            foreach (var child in children) {
                var contentLength = child.GetContentLength(PerpendicularAxis, this);
                
                if (child.PerpendicularAxisFill) {
                    float begin = Boundaries.GetSide(PerpendicularAxis, true) + GetSeperationByIndex(PerpendicularAxis, 0);
                    float end = Boundaries.GetSide(PerpendicularAxis, false) - GetSeperationByIndex(PerpendicularAxis, 1);
                    child.SetBoundariesAxis(PerpendicularAxis, begin, end);
                } else if (PerpendicularAxisAlign == Align.Begin) {
                    float begin = Boundaries.GetSide(PerpendicularAxis, true) + GetSeperationByIndex(PerpendicularAxis, 0);
                    float end = begin + contentLength;
                    child.SetBoundariesAxis(PerpendicularAxis, begin, end);
                } else if (PerpendicularAxisAlign == Align.End) {
                    float end = Boundaries.GetSide(PerpendicularAxis, false) - GetSeperationByIndex(PerpendicularAxis, 1);
                    float begin = end - contentLength;
                    child.SetBoundariesAxis(PerpendicularAxis, begin, end);
                } else if (PerpendicularAxisAlign is Align.Center or Align.Even) {
                    var leftOver = (Boundaries.GetAxisLength(PerpendicularAxis) - contentLength) / 2;
                    var begin = Boundaries.GetSide(PerpendicularAxis, true) + leftOver;
                    var end = Boundaries.GetSide(PerpendicularAxis, false) - leftOver;
                    
                    child.SetBoundariesAxis(PerpendicularAxis, begin, end);
                }
            }

            foreach (var child in children) {
                child.PositionChildren();
            }

            RepositioningNeeded = false;
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
            var children = Children.OfType<BoxUI>().ToList();
            if (children.Count == 0) return Padding.GetSideSum(axis);

            if (MainAxis == axis) {
                float length = 0;

                for (int i = 0; i < children.Count; i++) {
                    length += GetSeperationByIndex(axis, i);
                    length += children[i].GetContentLength(axis, this);
                }
                length += GetSeperationByIndex(axis, children.Count);
                if (caller != this && MainAxisShrink && Parent.Children.Count > 1) {
                    return 0;
                } else {
                    return Math.Min(Math.Max(MainAxisMin, length), MainAxisMax);
                }
            } else {
                float length = children.Max(child => Util.Max(child.Margin.GetSide(axis, true), Padding.GetSide(axis, true)) + child.GetContentLength(axis, this) + Util.Max(child.Margin.GetSide(axis, false), Padding.GetSide(axis, false)));
                if (caller != this && PerpendicularAxisShrink && Parent.Children.Count > 1) {
                    return 0;
                } else {
                    return Math.Min(Math.Max(PerpendicularAxisMin, length), PerpendicularAxisMax);
                }
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

            if (Overflowed) {
                if (Focused) ScrollValue = Math.Clamp(ScrollValue - InputManager.ScrollWheelDelta /10f, 0, GetContentLength(MainAxis, this) - Boundaries.GetAxisLength(MainAxis));
                SmoothScrollValue = Util.Lerp(SmoothScrollValue, ScrollValue, 0.075f);
            }
        }

        public override void Draw() => Draw(Vector2.Zero);
        public virtual void Draw(Vector2 displacement) {
            if (!Overflowed) {
                Renderer?.Draw(Boundaries.Displace(displacement));

                foreach (var child in Children.OfType<BoxUI>()) {
                    child.Draw(displacement);
                } 
            } else {
                SQ.SB.End();
                SQ.GD.SetRenderTarget(ContentRenderTarget);
                SQ.GD.Clear(Color.Transparent);
                SQ.SB.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);

                var scrollDisplacement = MainAxis == Axis.Horizontal ? new Vector2(-SmoothScrollValue, 0) : new Vector2(0, -SmoothScrollValue);
                
                // TODO: Optimize performance in directory with tons of files inside
                foreach (var child in Children.OfType<BoxUI>()) {
                    if (Util.IntersectsOrAdjacent(child.Boundaries.Displace(scrollDisplacement), Boundaries)) {
                        child.Draw(-Boundaries.TopLeft() + scrollDisplacement);
                    }
                }

                SQ.SB.End();
                SQ.GD.SetRenderTarget(null);
                SQ.SB.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
                SQ.SB.Draw(ContentRenderTarget, (Rectangle)Boundaries, Color.White);
                
                new BoxUIDebugRenderer().Draw(Boundaries);
            }
        }
    }
}